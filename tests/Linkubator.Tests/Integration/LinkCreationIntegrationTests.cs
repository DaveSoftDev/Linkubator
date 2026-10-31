using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using LinkDto = Linkubator.Application.Dtos.Models.LinkDto;
using SqliteConnectionFactory = Linkubator.Infrastructure.Sqlite.SqliteConnectionFactory;
using TestHost = Microsoft.AspNetCore.Hosting;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public sealed class LinkCreationIntegrationTests : IAsyncLifetime
{
    private const string CreateRoute = "/app/enlaces/nuevo";
    private const string QuickAddRoute = "/app?handler=QuickAdd";
    private const string DemoEmail = "user@linkubator.test";
    private const string TempDataCookieName = ".AspNetCore.Mvc.CookieTempDataProvider";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";
    private const string UnsupportedUrlSchemeCode = "UrlSchemeNotAllowed";
    private const string UnsupportedUrlSchemeMessage = "Introduce una dirección que empiece por https:// o http://.";
    private const string InvalidUrlMessage = "La URL no es válida.";
    private const string UrlRequiredMessage = "La URL es obligatoria.";
    private const string InvalidImageMessage = "La dirección de la imagen no es válida.";
    private readonly SqliteConnectionFactory _connections = new($"Data Source=links-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
    private SqliteConnection _keeper = null!;
    private ConfiguredWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private long _userId;
    private long _collectionId;

    public async Task InitializeAsync()
    {
        _keeper = _connections.CreateOpenConnection();
        _factory = new ConfiguredWebApplicationFactory(_keeper.ConnectionString);
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        _userId = await _keeper.ExecuteScalarAsync<long>("SELECT Id FROM User WHERE Email = @Email;", new { Email = DemoEmail });
        _collectionId = await AddCollection(_userId, "Destino", isPublic: false);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _keeper.DisposeAsync();
    }

    [Fact]
    public async Task GetNewLink_WithManyCollections_RendersAllOwnedOptionsInOrder()
    {
        await AddCollections(18);
        long foreignUserId = await AddOtherUser();
        await AddCollection(foreignUserId, "Colección ajena", false);

        using HttpResponseMessage response = await _client.GetAsync(CreateRoute + "?userId=" + foreignUserId);
        string html = await response.Content.ReadAsStringAsync();
        string options = GetSelect(html);
        string[] names = Regex.Matches(options, "<option value=\"[0-9]+\"[^>]*>(.*?)</option>")
            .Select(match => WebUtility.HtmlDecode(match.Groups[1].Value)).ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(20, names.Length);
        Assert.Equal("Bandeja de entrada", names[0]);
        Assert.Equal("Destino", names[^1]);
        Assert.DoesNotContain("Colección ajena", options, StringComparison.Ordinal);
        Assert.DoesNotContain("checked", GetInput(html, "Input.IsPublic"), StringComparison.Ordinal);
        Assert.DoesNotContain("<dialog", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task GetNewLink_WithValidIdentity_RendersAccessibleFormUsingCollectionStyles()
    {
        using HttpResponseMessage response = await _client.GetAsync(CreateRoute);
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("class=\"collection-form\"", html, StringComparison.Ordinal);
        Assert.Contains("method=\"post\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"__RequestVerificationToken\"", html, StringComparison.Ordinal);
        Assert.Contains("for=\"link-collection\"", html, StringComparison.Ordinal);
        Assert.Contains("aria-describedby=\"link-collection-error\"", html, StringComparison.Ordinal);
        Assert.Contains("for=\"link-url\"", html, StringComparison.Ordinal);
        Assert.Contains("disabled", GetInput(html, "Input.IsPublic"), StringComparison.Ordinal);
        Assert.Contains("noindex, nofollow", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.UserId\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"ru\" value=\"/app/enlaces\"", html, StringComparison.Ordinal);
        Assert.Contains("class=\"collection-form__cancel\" href=\"/app/enlaces\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/app/enlaces")]
    [InlineData("/app/enlaces?q=pendiente&p=1")]
    public async Task PostNewLink_FromGeneralListing_UsesChosenCollectionAndReturnsToOrigin(string origin)
    {
        using HttpResponseMessage source = await _client.GetAsync(origin);
        string sourceHtml = await source.Content.ReadAsStringAsync();
        string anchor = Regex.Match(sourceHtml, "<a\\b(?=[^>]*class=\"[^\"]*\\btoolbar-create-link\\b)[^>]*>").Value;
        string destination = WebUtility.HtmlDecode(Regex.Match(anchor, "href=\"([^\"]+)\"").Groups[1].Value);

        using HttpResponseMessage form = await _client.GetAsync(destination);
        string formHtml = await form.Content.ReadAsStringAsync();
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields["ru"] = WebUtility.HtmlDecode(Regex.Match(GetInput(formHtml, "ru"), "value=\"([^\"]*)\"").Groups[1].Value);
        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(Regex.Match(GetInput(formHtml, "__RequestVerificationToken"), "value=\"([^\"]+)\"").Groups[1].Value);

        using HttpResponseMessage response = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));
        LinkDto created = await _keeper.QuerySingleAsync<LinkDto>("SELECT CollectionId, IsPublic FROM Link;");

        Assert.Equal(HttpStatusCode.OK, form.StatusCode);
        Assert.DoesNotContain("collectionId=", destination, StringComparison.Ordinal);
        Assert.DoesNotContain("selected=\"selected\"", GetSelect(formHtml), StringComparison.Ordinal);
        Assert.Contains($"value=\"{_collectionId}\"", GetSelect(formHtml), StringComparison.Ordinal);
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{origin}\"", WebUtility.HtmlDecode(formHtml), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(origin, response.Headers.Location?.OriginalString);
        Assert.Equal(_collectionId, created.CollectionId);
        Assert.False(created.IsPublic);
    }

    [Theory]
    [InlineData("sidebar-create-link", false)]
    [InlineData("toolbar-create-link", false)]
    [InlineData("sidebar-create-link", true)]
    [InlineData("toolbar-create-link", true)]
    public async Task PostNewLink_FromCollection_PrefillsDestinationAndReturnsToOrigin(string buttonClass, bool isPublic)
    {
        long collectionId = await AddCollection(_userId, "Contexto", isPublic);
        string origin = $"/app/enlaces?c={collectionId}&q=pendiente&p=1";
        using HttpResponseMessage source = await _client.GetAsync(origin);
        string sourceHtml = await source.Content.ReadAsStringAsync();
        string anchor = Regex.Match(sourceHtml, $"<a\\b(?=[^>]*class=\"[^\"]*\\b{buttonClass}\\b)[^>]*>").Value;
        string destination = WebUtility.HtmlDecode(Regex.Match(anchor, "href=\"([^\"]+)\"").Groups[1].Value);

        using HttpResponseMessage form = await _client.GetAsync(destination);
        string formHtml = await form.Content.ReadAsStringAsync();
        string selected = Regex.Match(GetSelect(formHtml), "<option\\b(?=[^>]*selected=\"selected\")[^>]*>").Value;
        Dictionary<string, string> fields = NewFields(collectionId);
        fields["Input.CollectionId"] = Regex.Match(selected, "value=\"([^\"]+)\"").Groups[1].Value;
        fields["ru"] = WebUtility.HtmlDecode(Regex.Match(GetInput(formHtml, "ru"), "value=\"([^\"]*)\"").Groups[1].Value);
        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(Regex.Match(GetInput(formHtml, "__RequestVerificationToken"), "value=\"([^\"]+)\"").Groups[1].Value);

        using HttpResponseMessage response = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));
        LinkDto created = await _keeper.QuerySingleAsync<LinkDto>("SELECT CollectionId, IsPublic FROM Link;");

        Assert.Equal(HttpStatusCode.OK, form.StatusCode);
        Assert.Contains($"collectionId={collectionId}", destination, StringComparison.Ordinal);
        Assert.Contains($"value=\"{collectionId}\"", selected, StringComparison.Ordinal);
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{origin}\"", WebUtility.HtmlDecode(formHtml), StringComparison.Ordinal);
        Assert.DoesNotContain("checked", GetInput(formHtml, "Input.IsPublic"), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(origin, response.Headers.Location?.OriginalString);
        Assert.Equal(collectionId, created.CollectionId);
        Assert.False(created.IsPublic);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("invalid")]
    [InlineData("9223372036854775808")]
    public async Task GetNewLink_WithInvalidCollectionHint_DoesNotPreselectOrCreate(string collectionId)
    {
        using HttpResponseMessage response = await _client.GetAsync(CreateRoute + "?collectionId=" + Uri.EscapeDataString(collectionId));
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("selected=\"selected\"", GetSelect(html), StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10000)]
    public async Task GetNewLink_WithForeignOrMissingCollectionHint_DoesNotPreselectOrDisclose(int offset)
    {
        long otherUserId = await AddOtherUser();
        long foreignId = await AddCollection(otherUserId, "Colección privada ajena", true);

        using HttpResponseMessage response = await _client.GetAsync($"{CreateRoute}?collectionId={foreignId + offset}&userId={otherUserId}");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("selected=\"selected\"", GetSelect(html), StringComparison.Ordinal);
        Assert.DoesNotContain("Colección privada ajena", html, StringComparison.Ordinal);
        Assert.Contains("disabled", GetInput(html, "Input.IsPublic"), StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task PostNewLink_WithChangedCollection_UsesPostedDestinationInsteadOfInitialHint()
    {
        long selectedId = await AddCollection(_userId, "Nueva selección", false);
        Dictionary<string, string> fields = NewFields(selectedId);
        fields["ru"] = $"/app/enlaces?c={_collectionId}";

        using HttpResponseMessage response = await Post(fields, $"{CreateRoute}?collectionId={_collectionId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(selectedId, await _keeper.ExecuteScalarAsync<long>("SELECT CollectionId FROM Link;"));
        Assert.Equal($"/app/enlaces?c={_collectionId}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("?returnUrl=%2Fapp", "/app/enlaces")]
    [InlineData("?ru=%2Fapp&returnUrl=%2Fapp%2Fcolecciones", "/app")]
    public async Task GetNewLink_WithReturnParameterNames_UsesOnlyRu(string query, string expected)
    {
        using HttpResponseMessage response = await _client.GetAsync(CreateRoute + query);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{expected}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"ReturnUrl\"", html, StringComparison.Ordinal);
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{expected}\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/app", "/app")]
    [InlineData("/APP/", "/APP/")]
    [InlineData("/app/colecciones?q=Destino&p=1#lista", "/app/colecciones?q=Destino&p=1#lista")]
    [InlineData("/app/enlaces?q=uno%26dos&c=2&p=3", "/app/enlaces?q=uno%26dos&c=2&p=3")]
    [InlineData("https://example.com/app", "/app/enlaces")]
    [InlineData("//example.com/app", "/app/enlaces")]
    [InlineData("/app/colecciones/../enlaces/nuevo", "/app/enlaces")]
    [InlineData("/app/%2e%2e/outside", "/app/enlaces")]
    [InlineData("/app\\..\\outside", "/app/enlaces")]
    [InlineData("/app/colecciones/nueva", "/app/enlaces")]
    [InlineData(CreateRoute, "/app/enlaces")]
    [InlineData("/app?x=\r\nLocation: https://example.com", "/app/enlaces")]
    public async Task GetNewLink_WithReturnUrl_RendersValidatedCancelAndPostDestination(string returnUrl, string expected)
    {
        using HttpResponseMessage response = await _client.GetAsync(CreateRoute + "?ru=" + Uri.EscapeDataString(returnUrl));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{expected}\"", html, StringComparison.Ordinal);
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{expected}\"", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Theory]
    [InlineData("/app")]
    [InlineData("/app/colecciones")]
    [InlineData("/app/colecciones?q=Destino&p=1")]
    [InlineData("/app/enlaces")]
    [InlineData("/app/enlaces?q=Destino&p=1")]
    public async Task PostNewLink_FromSidebar_ReturnsToOriginWithOneTimeConfirmation(string origin)
    {
        using HttpResponseMessage source = await _client.GetAsync(origin);
        string sourceHtml = await source.Content.ReadAsStringAsync();
        string createLink = Regex.Match(sourceHtml, "<a\\b[^>]*class=\"primary-button sidebar-create-link\"[^>]*>").Value;
        string destination = WebUtility.HtmlDecode(Regex.Match(createLink, "href=\"([^\"]+)\"").Groups[1].Value);
        using HttpResponseMessage form = await _client.GetAsync(destination);
        string formHtml = await form.Content.ReadAsStringAsync();
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields["Input.Url"] = " example.com/path?q=uno&next=a+b#fragment ";
        fields["ru"] = WebUtility.HtmlDecode(Regex.Match(GetInput(formHtml, "ru"), "value=\"([^\"]*)\"").Groups[1].Value);
        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(Regex.Match(GetInput(formHtml, "__RequestVerificationToken"), "value=\"([^\"]+)\"").Groups[1].Value);
        fields["CreatedLinkUrl"] = "javascript:alert(1)";

        using HttpResponseMessage response = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));
        using HttpResponseMessage returned = await _client.GetAsync(response.Headers.Location!);
        string html = await returned.Content.ReadAsStringAsync();
        string notice = Regex.Match(html, "<div\\b[^>]*class=\"app-alert app-alert--success\"[^>]*>.*?</div>", RegexOptions.Singleline).Value;
        using HttpResponseMessage reloaded = await _client.GetAsync(origin);
        string reloadedHtml = await reloaded.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(origin, response.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
        Assert.Contains("role=\"status\"", notice, StringComparison.Ordinal);
        Assert.Contains("Enlace creado.", notice, StringComparison.Ordinal);
        Assert.Contains("href=\"https://example.com/path?q=uno&next=a+b#fragment\"", WebUtility.HtmlDecode(notice), StringComparison.Ordinal);
        Assert.Contains("&amp;next=", notice, StringComparison.Ordinal);
        Assert.Contains("target=\"_blank\" rel=\"noopener noreferrer\"", notice, StringComparison.Ordinal);
        Assert.Contains(">Ver enlace</a>", notice, StringComparison.Ordinal);
        Assert.DoesNotContain("javascript:alert", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Enlace creado.", reloadedHtml, StringComparison.Ordinal);
        Assert.Equal(1, await LinkCount());
        Assert.Equal("https://example.com/path?q=uno&next=a+b#fragment", await _keeper.ExecuteScalarAsync<string>("SELECT UrlOriginal FROM Link;"));
    }

    [Theory]
    [InlineData("/app/enlaces?q=uno&c=2&p=3", "/app/enlaces?q=uno&c=2&p=3")]
    [InlineData("https://example.com/app", "/app/enlaces")]
    [InlineData("//example.com/app", "/app/enlaces")]
    [InlineData("/app/../../outside", "/app/enlaces")]
    [InlineData("/app\\..\\outside", "/app/enlaces")]
    [InlineData("/app?x=\r\nLocation: https://example.com", "/app/enlaces")]
    [InlineData(CreateRoute, "/app/enlaces")]
    public async Task PostNewLink_WithPostedReturnUrl_UsesValidatedDestination(string returnUrl, string expected)
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields["ru"] = returnUrl;

        using HttpResponseMessage response = await Post(fields);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expected, response.Headers.Location?.OriginalString);
        Assert.Equal(1, await LinkCount());
    }

    [Theory]
    [InlineData("/app")]
    [InlineData("/app/colecciones?q=Destino&p=1")]
    public async Task PostNewLink_WithValidationError_KeepsOriginForCancelAndRetry(string origin)
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields["ru"] = origin;
        fields["Input.Url"] = "javascript:alert(1)";
        fields["Input.Title"] = "Título pendiente";

        using HttpResponseMessage response = await Post(fields);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{origin}\"", html, StringComparison.Ordinal);
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{origin}\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"#link-url\"", html, StringComparison.Ordinal);
        Assert.Contains("value=\"Título pendiente\"", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());

        fields["Input.Url"] = "https://example.com/retry";
        fields["__RequestVerificationToken"] = Regex.Match(GetInput(html, "__RequestVerificationToken"), "value=\"([^\"]+)\"").Groups[1].Value;
        using HttpResponseMessage retry = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));

        Assert.Equal(HttpStatusCode.Redirect, retry.StatusCode);
        Assert.Equal(origin, retry.Headers.Location?.OriginalString);
        Assert.Equal(1, await LinkCount());
    }

    [Theory]
    [InlineData("/app")]
    [InlineData("/app/colecciones?q=Destino&p=1")]
    public async Task PostNewLink_WithPrivacyRejection_ReturnsToOriginWithErrorNotice(string origin)
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields["Input.IsPublic"] = "true";
        fields["ru"] = origin;

        using HttpResponseMessage response = await Post(fields);
        using HttpResponseMessage returned = await _client.GetAsync(response.Headers.Location!);
        string html = await returned.Content.ReadAsStringAsync();
        string notice = Regex.Match(html, "<div\\b[^>]*class=\"app-alert app-alert--error\"[^>]*>.*?</div>", RegexOptions.Singleline).Value;

        Assert.Equal(origin, response.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
        Assert.Contains("role=\"status\"", notice, StringComparison.Ordinal);
        Assert.Contains("No se puede hacer público un enlace de una colección privada.", WebUtility.HtmlDecode(notice), StringComparison.Ordinal);
        Assert.DoesNotContain("Enlace creado.", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetNewLink_WithCollection_ExposesItsPrivacyForTheSelector(bool isPublic)
    {
        long collectionId = await AddCollection(_userId, "Selección", isPublic);

        using HttpResponseMessage response = await _client.GetAsync(CreateRoute);
        string html = await response.Content.ReadAsStringAsync();
        string option = Regex.Match(GetSelect(html), $"<option\\b(?=[^>]*value=\"{collectionId}\")[^>]*>").Value;

        Assert.Contains($"data-public=\"{isPublic.ToString().ToLowerInvariant()}\"", option, StringComparison.Ordinal);
        Assert.Contains("disabled", GetInput(html, "Input.IsPublic"), StringComparison.Ordinal);
        Assert.DoesNotContain("checked", GetInput(html, "Input.IsPublic"), StringComparison.Ordinal);
        Assert.Contains("aria-describedby=\"link-public-hint link-public-error\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task PostNewLink_WithUrlError_RestoresCollectionAndPrivacy(bool publicCollection, bool publicLink)
    {
        long collectionId = await AddCollection(_userId, "Selección", publicCollection);
        Dictionary<string, string> fields = NewFields(collectionId);
        fields["Input.Url"] = "javascript:alert(1)";
        fields["Input.IsPublic"] = publicLink.ToString(CultureInfo.InvariantCulture);

        using HttpResponseMessage response = await Post(fields);
        string html = await response.Content.ReadAsStringAsync();
        string option = Regex.Match(GetSelect(html), $"<option\\b(?=[^>]*value=\"{collectionId}\")[^>]*>").Value;
        string checkbox = GetInput(html, "Input.IsPublic");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("selected", option, StringComparison.Ordinal);
        Assert.Equal(!publicCollection, checkbox.Contains("disabled", StringComparison.Ordinal));
        Assert.Equal(publicLink, checkbox.Contains("checked", StringComparison.Ordinal));
        Assert.Contains("href=\"#link-url\"", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://example.com/path?q=uno&next=a+b#fragment")]
    [InlineData("https://example.com/área?q=colección&value=%26")]
    [InlineData("https://example.com/\"><script>alert(1)</script>")]
    [InlineData("not a valid URL yet")]
    public async Task GetNewLink_WithUrlQuery_PrefillsEncodedUrlWithoutCreatingLink(string url)
    {
        using HttpResponseMessage response = await _client.GetAsync(CreateRoute + "?url=" + Uri.EscapeDataString(url));
        string html = await response.Content.ReadAsStringAsync();
        string input = GetInput(html, "Input.Url");
        string value = WebUtility.HtmlDecode(Regex.Match(input, "value=\"([^\"]*)\"").Groups[1].Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(url, value);
        Assert.DoesNotContain("<script>alert(1)</script>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("input-validation-error", input, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostNewLink_WithValidData_PersistsOwnerValuesAndSearchProjection(bool isPublic)
    {
        long collectionId = await AddCollection(_userId, "Creación", isPublic);
        Dictionary<string, string> fields = NewFields(collectionId);
        fields["Input.Url"] = " example.com/articulo ";
        fields["Input.Title"] = "Título <b>propio</b>";
        fields["Input.Description"] = "Descripción propia";
        fields["Input.Image"] = "/cover.png";
        fields["Input.IsPublic"] = isPublic.ToString(CultureInfo.InvariantCulture);
        fields["UserId"] = (await AddOtherUser()).ToString(CultureInfo.InvariantCulture);

        using HttpResponseMessage response = await Post(fields);
        using SqliteConnection connection = _connections.CreateOpenConnection();
        LinkDto link = await connection.QuerySingleAsync<LinkDto>("SELECT Id, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, CreatedAt FROM Link;");
        long ownerId = await connection.ExecuteScalarAsync<long>("SELECT UserId FROM Link WHERE Id = @Id;", new { link.Id });
        string? indexedTitle = await connection.ExecuteScalarAsync<string>("SELECT Title FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;", new { link.Id, UserId = _userId });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app/enlaces", response.Headers.Location?.OriginalString);
        Assert.Equal(_userId, ownerId);
        Assert.Equal(collectionId, link.CollectionId);
        Assert.Equal("https://example.com/articulo", link.UrlOriginal);
        Assert.Equal("Título <b>propio</b>", link.Title);
        Assert.Equal("Descripción propia", link.Description);
        Assert.Equal("https://example.com/cover.png", link.Image);
        Assert.Equal(isPublic, link.IsPublic);
        Assert.Equal("Título <b>propio</b>", indexedTitle);
        Assert.Equal(1, await LinkCount());
    }

    [Fact]
    public async Task PostNewLink_WithoutOptionalValues_CreatesPrivateLink()
    {
        using HttpResponseMessage response = await Post(NewFields(_collectionId));
        using SqliteConnection connection = _connections.CreateOpenConnection();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link WHERE IsPublic = 0 AND Title IS NULL AND Description IS NULL AND Image IS NULL;"));
    }

    [Theory]
    [InlineData("Input.Url", "javascript:alert(1)", "link-url", UnsupportedUrlSchemeMessage)]
    [InlineData("Input.Url", "https://intranet", "link-url", InvalidUrlMessage)]
    [InlineData("Input.Url", "", "link-url", UrlRequiredMessage)]
    [InlineData("Input.Image", "file:///image.png", "link-image", UnsupportedUrlSchemeMessage)]
    [InlineData("Input.Image", "https://127.0.0.1/image.png", "link-image", InvalidImageMessage)]
    public async Task PostNewLink_WithInvalidField_PreservesInputAndAssociatesError(
        string field,
        string value,
        string fieldId,
        string expectedMessage)
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields[field] = value;
        fields["Input.Title"] = "Texto <script>alert(1)</script>";

        using HttpResponseMessage response = await Post(fields);
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"href=\"#{fieldId}\"", html, StringComparison.Ordinal);
        Assert.Contains($"data-valmsg-for=\"{field}\"", html, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Matches(WebUtility.HtmlDecode(html), Regex.Escape(expectedMessage)).Count);
        Assert.Equal(value, WebUtility.HtmlDecode(Regex.Match(GetInput(html, field), "value=\"([^\"]*)\"").Groups[1].Value));
        Assert.Contains("Texto <script>alert(1)</script>", WebUtility.HtmlDecode(html), StringComparison.Ordinal);
        Assert.DoesNotContain("<script>alert(1)</script>", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Theory]
    [InlineData("Input.Url", "javascript:alert(1)", "link-url")]
    [InlineData("Input.Image", "file:///image.png", "link-image")]
    public async Task PostNewLink_WithUnsupportedScheme_ShowsFriendlyMessageWithoutCreatingLink(string field, string value, string fieldId)
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields[field] = value;

        using HttpResponseMessage response = await Post(fields);
        string html = await response.Content.ReadAsStringAsync();
        string decodedHtml = WebUtility.HtmlDecode(html);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"href=\"#{fieldId}\"", html, StringComparison.Ordinal);
        Assert.Contains($"data-valmsg-for=\"{field}\"", html, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Matches(decodedHtml, Regex.Escape(UnsupportedUrlSchemeMessage)).Count);
        Assert.DoesNotContain(UnsupportedUrlSchemeCode, decodedHtml, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Theory]
    [InlineData("Input.Title", 301, "link-title")]
    [InlineData("Input.Description", 1001, "link-description")]
    public async Task PostNewLink_WithOversizedText_AssociatesErrorWithoutCreatingLink(string field, int length, string fieldId)
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields[field] = new string('x', length);

        using HttpResponseMessage response = await Post(fields);
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Matches($"<span[^>]*data-valmsg-for=\"{field}\"[^>]*>[^<]+</span>", WebUtility.HtmlDecode(html));
        Assert.Contains(new string('x', length), html, StringComparison.Ordinal);
        Assert.Contains($"href=\"#{fieldId}\"", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task PostNewLink_WithEquivalentUrl_ShowsExistingCollectionAndDoesNotDuplicate()
    {
        using HttpResponseMessage created = await Post(NewFields(_collectionId));
        long otherCollectionId = await AddCollection(_userId, "Otra colección", false);
        Dictionary<string, string> fields = NewFields(otherCollectionId);
        fields["Input.Url"] = "http://example.com/path#fragment";

        string before = await LinkSnapshot();

        using HttpResponseMessage response = await Post(fields);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("La colección asociada es 'Destino'", html, StringComparison.Ordinal);
        Assert.Contains("href=\"#link-url\"", html, StringComparison.Ordinal);
        Assert.Equal(1, await LinkCount());
        Assert.Equal(before, await LinkSnapshot());
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("1.0")]
    [InlineData(" 1 ")]
    [InlineData("9223372036854775808")]
    [InlineData("999999")]
    public async Task PostNewLink_WithInvalidOrMissingCollection_ReturnsFieldErrorWithoutCreatingLink(string collectionId)
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields["Input.CollectionId"] = collectionId;

        using HttpResponseMessage response = await Post(fields);
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-collection\"", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task PostNewLink_WithForeignCollection_DoesNotCreateOrDiscloseForeignData()
    {
        long foreignUserId = await AddOtherUser();
        long foreignCollectionId = await AddCollection(foreignUserId, "Nombre privado ajeno", false);

        using HttpResponseMessage response = await Post(NewFields(foreignCollectionId));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-collection\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Nombre privado ajeno", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task PostNewLink_WithPublicRequestInPrivateCollection_RedirectsAndDoesNotCreateLink()
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields["Input.IsPublic"] = "true";

        using HttpResponseMessage response = await Post(fields);
        IEnumerable<string> setCookieHeaders = response.Headers.GetValues("Set-Cookie");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app/enlaces", response.Headers.Location?.OriginalString);
        Assert.Contains(setCookieHeaders, header => header.StartsWith(TempDataCookieName + "=", StringComparison.Ordinal));
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task PostNewLink_WithPublicRequestInPrivateCollection_ShowsAccessibleErrorOnListing()
    {
        Dictionary<string, string> fields = NewFields(_collectionId);
        fields["Input.IsPublic"] = "true";

        using HttpResponseMessage response = await Post(fields);
        IEnumerable<string> setCookieHeaders = response.Headers.GetValues("Set-Cookie");
        string tempDataCookie = setCookieHeaders.Single(header => header.StartsWith(TempDataCookieName + "=", StringComparison.Ordinal))
            .Split(';', 2)[0];
        using HttpRequestMessage listingRequest = new(HttpMethod.Get, response.Headers.Location);
        listingRequest.Headers.Add("Cookie", tempDataCookie);

        using HttpResponseMessage listing = await _client.SendAsync(listingRequest);
        string html = await listing.Content.ReadAsStringAsync();
        string decodedHtml = WebUtility.HtmlDecode(html);
        string alert = Regex.Match(html, "<div\\b(?=[^>]*class=\\\"app-alert app-alert--error\\\")(?=[^>]*role=\\\"status\\\")(?=[^>]*aria-live=\\\"polite\\\")[^>]*>.*?</div>", RegexOptions.Singleline).Value;

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listing.StatusCode);
        Assert.Contains("No se puede hacer público un enlace de una colección privada", WebUtility.HtmlDecode(alert), StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task GetLinks_WithoutPrivacyError_DoesNotRenderPrivacyNotice()
    {
        using HttpResponseMessage listing = await _client.GetAsync("/app/enlaces");
        string html = WebUtility.HtmlDecode(await listing.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, listing.StatusCode);
        Assert.DoesNotContain("No se puede hacer público un enlace de una colección privada", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostNewLink_WithoutAntiforgeryToken_RejectsWithoutCreatingLink()
    {
        using HttpResponseMessage response = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(NewFields(_collectionId)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task GetDashboard_WithCreateAction_LinksToCanonicalForm()
    {
        using HttpResponseMessage response = await _client.GetAsync("/app");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Contains($"href=\"{CreateRoute}\"", html, StringComparison.Ordinal);
        Assert.Contains("data-quick-add-advanced", html, StringComparison.Ordinal);
        Assert.Contains("handler=QuickAdd", html, StringComparison.Ordinal);
        Assert.Contains("name=\"QuickUrl\"", html, StringComparison.Ordinal);
        Assert.Contains("type=\"submit\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"__RequestVerificationToken\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostQuickAdd_WithInbox_IgnoresClientMetadataAndCreatesPrivateLink(bool publicCollection)
    {
        await AddCollection(_userId, "Antes de bandeja", false);
        long foreignUserId = await AddOtherUser();
        long foreignCollectionId = await AddCollection(foreignUserId, "Ajena", true);
        await _keeper.ExecuteAsync(
            "UPDATE Collection SET IsPublic = @IsPublic WHERE UserId = @UserId AND Slug = 'bandeja-de-entrada';",
            new { IsPublic = publicCollection, UserId = _userId });
        Dictionary<string, string> fields = new()
        {
            ["QuickUrl"] = " example.com/rapido ",
            ["UserId"] = foreignUserId.ToString(CultureInfo.InvariantCulture),
            ["CollectionId"] = foreignCollectionId.ToString(CultureInfo.InvariantCulture),
            ["Input.CollectionId"] = foreignCollectionId.ToString(CultureInfo.InvariantCulture),
            ["IsPublic"] = "true",
            ["Input.IsPublic"] = "true",
            ["Title"] = "Título no solicitado",
            ["Input.Title"] = "Otro título",
            ["Description"] = "Descripción no solicitada",
            ["Image"] = "https://example.com/image.png",
        };

        using HttpResponseMessage response = await Post(fields, QuickAddRoute);
        LinkDto link = await _keeper.QuerySingleAsync<LinkDto>(
            "SELECT Id, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, CreatedAt FROM Link;");
        long inboxId = await _keeper.ExecuteScalarAsync<long>(
            "SELECT Id FROM Collection WHERE UserId = @UserId AND Slug = 'bandeja-de-entrada';", new { UserId = _userId });
        long ownerId = await _keeper.ExecuteScalarAsync<long>("SELECT UserId FROM Link;");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app", response.Headers.Location?.OriginalString);
        Assert.Equal(_userId, ownerId);
        Assert.Equal(inboxId, link.CollectionId);
        Assert.Equal("https://example.com/rapido", link.UrlOriginal);
        Assert.False(link.IsPublic);
        Assert.Null(link.Title);
        Assert.Null(link.Description);
        Assert.Null(link.Image);
        Assert.Equal(1, await LinkCount());
    }

    [Fact]
    public async Task PostQuickAdd_WithoutOwnInbox_UsesFirstOwnedCollectionInListOrder()
    {
        await _keeper.ExecuteAsync(
            "UPDATE Collection SET Name = 'Zeta', Slug = 'zeta' WHERE UserId = @UserId AND Slug = 'bandeja-de-entrada';",
            new { UserId = _userId });
        long firstCollectionId = await AddCollection(_userId, "Alfabética", true);
        long foreignUserId = await AddOtherUser();
        long foreignInboxId = await AddCollection(foreignUserId, "Bandeja de entrada", false);
        await _keeper.ExecuteAsync("UPDATE Collection SET Slug = 'bandeja-de-entrada' WHERE Id = @Id;", new { Id = foreignInboxId });

        using HttpResponseMessage response = await Post(new() { ["QuickUrl"] = "https://example.com/fallback" }, QuickAddRoute);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(firstCollectionId, await _keeper.ExecuteScalarAsync<long>("SELECT CollectionId FROM Link;"));
        Assert.Equal(0, await _keeper.ExecuteScalarAsync<int>("SELECT IsPublic FROM Link;"));
        Assert.Equal(1, await LinkCount());
    }

    [Theory]
    [InlineData("")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://intranet")]
    [InlineData("file:///local/path")]
    public async Task PostQuickAdd_WithInvalidUrl_ShowsErrorAndPreservesUrl(string url)
    {
        using HttpResponseMessage response = await Post(new() { ["QuickUrl"] = url }, QuickAddRoute);
        string html = await response.Content.ReadAsStringAsync();
        string value = WebUtility.HtmlDecode(Regex.Match(GetInput(html, "QuickUrl"), "value=\"([^\"]*)\"").Groups[1].Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#quick-url\"", html, StringComparison.Ordinal);
        Assert.Contains("role=\"alert\"", html, StringComparison.Ordinal);
        Assert.Equal(url, value);
        Assert.Contains("Todav&#xED;a no tienes enlaces guardados.", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task PostQuickAdd_WithDuplicateUrl_ShowsErrorWithoutCreatingAnotherLink()
    {
        using HttpResponseMessage created = await Post(NewFields(_collectionId));

        string before = await LinkSnapshot();

        using HttpResponseMessage response = await Post(new() { ["QuickUrl"] = "http://example.com/path#fragment" }, QuickAddRoute);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("La colección asociada es 'Destino'", html, StringComparison.Ordinal);
        Assert.Contains("href=\"#quick-url\"", html, StringComparison.Ordinal);
        Assert.Contains("http://example.com/path#fragment", html, StringComparison.Ordinal);
        Assert.Equal(1, await LinkCount());
        Assert.Equal(before, await LinkSnapshot());
    }

    [Fact]
    public async Task PostQuickAdd_WithoutCollections_ShowsErrorWithoutCreatingLink()
    {
        await _keeper.ExecuteAsync("DELETE FROM Collection WHERE UserId = @UserId;", new { UserId = _userId });

        using HttpResponseMessage response = await Post(new() { ["QuickUrl"] = "https://example.com/no-collection" }, QuickAddRoute);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No tienes colecciones disponibles. Crea una colección para añadir enlaces.", html, StringComparison.Ordinal);
        Assert.Contains("https://example.com/no-collection", html, StringComparison.Ordinal);
        Assert.Equal(0, await LinkCount());
    }

    [Fact]
    public async Task PostQuickAdd_WithoutAntiforgeryToken_RejectsWithoutCreatingLink()
    {
        using HttpResponseMessage response = await _client.PostAsync(
            QuickAddRoute, new FormUrlEncodedContent(new Dictionary<string, string> { ["QuickUrl"] = "https://example.com/path" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await LinkCount());
    }

    [Theory]
    [InlineData(CreateRoute)]
    [InlineData("/app")]
    public async Task PutLinkCreation_ReturnsNotFoundWithoutWriting(string route)
    {
        string before = await LinkSnapshot();
        using HttpRequestMessage request = new(HttpMethod.Put, route)
        {
            Content = new FormUrlEncodedContent(NewFields(_collectionId)),
        };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(before, await LinkSnapshot());
    }

    private async Task<string> LinkSnapshot()
    {
        string[] links = (await _keeper.QueryAsync<string>("SELECT json_object('Id',Id,'CollectionId',CollectionId,'Url',UrlOriginal,'Normalized',UrlNormalized,'Title',Title,'Description',Description,'Image',Image,'Public',IsPublic) FROM Link ORDER BY Id;")).ToArray();
        string[] projections = (await _keeper.QueryAsync<string>("SELECT json_object('Id',rowid,'UserId',UserId,'Url',UrlOriginal,'Title',Title,'Description',Description,'CollectionName',CollectionName) FROM LinkSearch ORDER BY rowid;")).ToArray();
        return string.Join("\n", links.Concat(projections));
    }

    private static Dictionary<string, string> NewFields(long collectionId)
        => new()
        {
            ["Input.CollectionId"] = collectionId.ToString(CultureInfo.InvariantCulture),
            ["Input.Url"] = "https://example.com/path",
        };

    [Theory]
    [InlineData("/app", "/app")]
    [InlineData("/APP/", "/APP/")]
    [InlineData(CreateRoute, "/app/enlaces")]
    [InlineData(CreateRoute + "?ru=%2Fapp", "/app/enlaces")]
    [InlineData("/app/colecciones", "/app/colecciones")]
    [InlineData("/app/colecciones?q=Destino&p=1", "/app/colecciones?q=Destino&p=1")]
    [InlineData("/app/colecciones/nueva", "/app/enlaces")]
    [InlineData("/app/colecciones/999999/editar", "/app/enlaces")]
    [InlineData("/app/colecciones/999999/eliminar", "/app/enlaces")]
    [InlineData("/app/404", "/app/enlaces")]
    public async Task GetPrivatePage_WithSidebar_RendersAccessibleCreateLinkBeforeNavigation(string route, string expectedOrigin)
    {
        using HttpResponseMessage response = await _client.GetAsync(route);
        string html = await response.Content.ReadAsStringAsync();
        string sidebar = Regex.Match(html, "<aside\\b[^>]*id=\"app-sidebar\"[^>]*>.*?</aside>", RegexOptions.Singleline).Value;
        string createLink = Regex.Match(sidebar, "<a\\b[^>]*class=\"primary-button sidebar-create-link\"[^>]*>.*?</a>", RegexOptions.Singleline).Value;

        Assert.Contains($"href=\"{CreateRoute}?ru={Uri.EscapeDataString(expectedOrigin)}\"", createLink, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Crear enlace\"", createLink, StringComparison.Ordinal);
        Assert.Contains("title=\"Crear enlace\"", createLink, StringComparison.Ordinal);
        Assert.Contains("class=\"sidebar-create-link__label\">Crear enlace</span>", createLink, StringComparison.Ordinal);
        Assert.Contains("aria-hidden=\"true\"", createLink, StringComparison.Ordinal);
        Assert.Contains("class=\"sidebar-actions\"", sidebar, StringComparison.Ordinal);
        Assert.True(sidebar.IndexOf("sidebar-create-link", StringComparison.Ordinal) < sidebar.IndexOf("class=\"sidebar-searches\"", StringComparison.Ordinal));
        Assert.True(sidebar.IndexOf("class=\"sidebar-searches\"", StringComparison.Ordinal) < sidebar.IndexOf("<nav", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("/app", "true", true)]
    [InlineData(CreateRoute, "true", true)]
    [InlineData("/app/colecciones", "true", true)]
    [InlineData("/app/404", "true", true)]
    [InlineData("/app", "false", false)]
    [InlineData(CreateRoute, "false", false)]
    [InlineData("/app", "invalid", false)]
    [InlineData("/app", "", false)]
    public async Task GetPrivatePage_WithSidebarPreference_RendersSavedStateBeforeJavaScript(string route, string preference, bool collapsed)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, route);
        request.Headers.Add("Cookie", $"sidebar-collapsed={preference}");

        using HttpResponseMessage response = await _client.SendAsync(request);
        string html = await response.Content.ReadAsStringAsync();
        string layout = Regex.Match(html, "<div\\b[^>]*class=\"app-layout[^\"]*\"[^>]*>").Value;
        string toggle = WebUtility.HtmlDecode(Regex.Match(html, "<button\\b[^>]*class=\"sidebar-collapse\"[^>]*>").Value);

        Assert.Equal(collapsed, layout.Contains("sidebar-collapsed", StringComparison.Ordinal));
        Assert.Contains($"aria-expanded=\"{(!collapsed).ToString().ToLowerInvariant()}\"", toggle, StringComparison.Ordinal);
        Assert.Contains(collapsed ? "Expandir menú lateral" : "Colapsar menú lateral", toggle, StringComparison.Ordinal);
        Assert.DoesNotContain("is-open", layout, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPrivatePage_WithoutSidebarPreference_RendersExpanded()
    {
        using HttpResponseMessage response = await _client.GetAsync(CreateRoute);
        string html = await response.Content.ReadAsStringAsync();

        Assert.Contains("class=\"app-layout\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"app-layout sidebar-collapsed\"", html, StringComparison.Ordinal);
    }

    private async Task<HttpResponseMessage> Post(Dictionary<string, string> fields, string route = CreateRoute)
    {
        using HttpResponseMessage form = await _client.GetAsync(CreateRoute);
        string html = await form.Content.ReadAsStringAsync();
        string token = GetInput(html, "__RequestVerificationToken");
        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(Regex.Match(token, "value=\"([^\"]+)\"").Groups[1].Value);
        return await _client.PostAsync(route, new FormUrlEncodedContent(fields));
    }

    private static string GetInput(string html, string name)
        => Regex.Match(html, $"<input\\b(?=[^>]*name=\"{Regex.Escape(name)}\")[^>]*>").Value;

    private static string GetSelect(string html)
        => Regex.Match(html, "<select\\b(?=[^>]*id=\"link-collection\")[^>]*>.*?</select>", RegexOptions.Singleline).Value;

    private async Task<int> LinkCount()
    {
        using SqliteConnection connection = _connections.CreateOpenConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link;");
    }

    private async Task<long> AddCollection(long userId, string name, bool isPublic)
    {
        using SqliteConnection connection = _connections.CreateOpenConnection();
        return await connection.ExecuteScalarAsync<long>(
            "INSERT INTO Collection (UserId, Name, Slug, IsPublic) VALUES (@UserId, @Name, @Slug, @IsPublic); SELECT last_insert_rowid();",
            new { UserId = userId, Name = name, Slug = Guid.NewGuid().ToString("N"), IsPublic = isPublic });
    }

    private async Task AddCollections(int count)
    {
        for (int index = 0; index < count; index++)
        {
            await AddCollection(_userId, $"Colección {index:D2}", false);
        }
    }

    private async Task<long> AddOtherUser()
    {
        using SqliteConnection connection = _connections.CreateOpenConnection();
        return await connection.ExecuteScalarAsync<long>(
            "INSERT INTO User (Email, Alias, SecurityStamp) VALUES (@Email, @Alias, @Stamp); SELECT last_insert_rowid();",
            new { Email = "other@linkubator.test", Alias = "other-user", Stamp = Guid.NewGuid().ToString("N") });
    }

    private sealed class ConfiguredWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(TestHost.IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", connectionString);
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
        }
    }
}
