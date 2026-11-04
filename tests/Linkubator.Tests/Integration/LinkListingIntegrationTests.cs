using System.Net;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationRequests = Linkubator.Application.Dtos.Requests;
using ApplicationResults = Linkubator.Application.Results;
using CurrentUser = Linkubator.Application.Contracts.ICurrentUser;
using LinkService = Linkubator.Application.Contracts.ILinkService;
using SearchTextNormalizer = Linkubator.Infrastructure.Search.SearchTextNormalizer;
using SqliteConnectionFactory = Linkubator.Infrastructure.Sqlite.SqliteConnectionFactory;
using TestHost = Microsoft.AspNetCore.Hosting;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public sealed class LinkListingIntegrationTests : IAsyncLifetime
{
    private const string DemoEmail = "user@linkubator.test";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";
    private readonly SqliteConnectionFactory _connections = new($"Data Source=listing-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
    private SqliteConnection _keeper = null!;
    private ConfiguredWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private long _userId;
    private long _collectionId;
    private long _otherCollectionId;
    private long _foreignUserId;
    private long _foreignCollectionId;

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
        _collectionId = await AddCollection(_userId, "Lecturas");
        _otherCollectionId = await AddCollection(_userId, "Otros");
        _foreignUserId = await _keeper.ExecuteScalarAsync<long>(
            "INSERT INTO User (Email, Alias, SecurityStamp) VALUES ('other@linkubator.test', 'other-user', @Stamp); SELECT last_insert_rowid();",
            new { Stamp = Guid.NewGuid().ToString("N") });
        _foreignCollectionId = await AddCollection(_foreignUserId, "Colección ajena");
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _keeper.DisposeAsync();
    }

    [Fact]
    public async Task GetLinks_WhenResourceAndCollectionErrorsCoincide_ShowsBothInOneAlert()
    {
        TestCurrentUser currentUser = new(_userId);
        await using WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<CurrentUser>(_ => currentUser)));
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        using HttpResponseMessage redirect = await client.GetAsync("/app/enlaces/999999/editar");
        currentUser.IsAuthenticated = false;
        using HttpResponseMessage response = await client.GetAsync("/app/enlaces");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string alert = Regex.Match(html, "<div\\b[^>]*class=\"app-alert app-alert--error\"[^>]*>.*?</div>", RegexOptions.Singleline).Value;

        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        Assert.Equal("/app/enlaces", redirect.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("class=\"app-main\"", html, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(html, "class=\"app-alert app-alert--error\""));
        Assert.Contains("No se ha podido acceder al elemento solicitado.", alert, StringComparison.Ordinal);
        Assert.Contains("No se han podido cargar las colecciones. Vuelve a intentarlo.", alert, StringComparison.Ordinal);
        Assert.True(html.IndexOf("class=\"search-form\"", StringComparison.Ordinal) < html.IndexOf(alert, StringComparison.Ordinal));
        Assert.Matches("(?s)<section aria-labelledby=\"links-list-title\">.*?<h3 id=\"links-list-title\">Enlaces</h3>.*?class=\"app-alert app-alert--error\".*?<div class=\"main-wrapper\">", html);
    }

    [Fact]
    public async Task GetLinks_WhenSidebarCollectionsFail_ShowsErrorWithoutFalseCounts()
    {
        TestCurrentUser currentUser = new(_userId) { IsAuthenticated = false };
        await using WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<CurrentUser>(_ => currentUser)));
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        using HttpResponseMessage response = await client.GetAsync("/app/enlaces");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string sidebar = Regex.Match(html, "(?s)<aside id=\"app-sidebar\".*?</aside>").Value;

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("No se han podido cargar las colecciones. Vuelve a intentarlo.", sidebar, StringComparison.Ordinal);
        Assert.Contains("href=\"/app/colecciones\"", sidebar, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"count-pill\"", sidebar, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetLinks_WhenListingFails_PreservesResourceNoticeAndAllErrorsInOneAlert()
    {
        await using WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<LinkService, FailingLinkService>()));
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        using HttpResponseMessage redirect = await client.GetAsync("/app/enlaces/-1/editar");
        using HttpResponseMessage response = await client.GetAsync("/app/enlaces");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string alert = Regex.Match(html, "<div\\b[^>]*class=\"app-alert app-alert--error\"[^>]*>.*?</div>", RegexOptions.Singleline).Value;

        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(Regex.Matches(html, "class=\"app-alert app-alert--error\""));
        Assert.Contains("No se ha podido acceder al elemento solicitado.", alert, StringComparison.Ordinal);
        Assert.Contains("Primer error de listado.", alert, StringComparison.Ordinal);
        Assert.Contains("SECOND_LIST_ERROR", alert, StringComparison.Ordinal);
        Assert.Matches("(?s)<section aria-labelledby=\"links-list-title\">.*?<h3 id=\"links-list-title\">Enlaces</h3>.*?class=\"app-alert app-alert--error\"", html);
        Assert.DoesNotContain("class=\"form-summary\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("No se pueden mostrar los resultados", html, StringComparison.Ordinal);
        Assert.Empty(CardTitles(html));
    }

    [Fact]
    public async Task GetLinks_WithListPreference_KeepsListPresentationAndActions()
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");
        using HttpRequestMessage request = new(HttpMethod.Get, "/app/enlaces");
        request.Headers.Add("Cookie", "links-view=list");

        using HttpResponseMessage response = await _client.SendAsync(request);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("links-grid links-grid--list", html, StringComparison.Ordinal);
        Assert.Equal(["Lectura 01"], CardTitles(html));
        Assert.Contains("Editar enlace Lectura 01", html, StringComparison.Ordinal);
        Assert.Contains("Mover enlace Lectura 01", html, StringComparison.Ordinal);
        Assert.Contains("Eliminar enlace Lectura 01", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/app", "cards", false, "private", "Privado", "Este enlace es privado")]
    [InlineData("/app", "list", false, "private", "Privado", "Este enlace es privado")]
    [InlineData("/app/enlaces", "cards", false, "private", "Privado", "Este enlace es privado")]
    [InlineData("/app/enlaces", "list", false, "private", "Privado", "Este enlace es privado")]
    [InlineData("/app/enlaces?c={0}", "cards", false, "private", "Privado", "Este enlace es privado")]
    [InlineData("/app/enlaces?c={0}", "list", false, "private", "Privado", "Este enlace es privado")]
    [InlineData("/app", "cards", true, "public", "Público", "Este enlace es público")]
    [InlineData("/app", "list", true, "public", "Público", "Este enlace es público")]
    [InlineData("/app/enlaces", "cards", true, "public", "Público", "Este enlace es público")]
    [InlineData("/app/enlaces", "list", true, "public", "Público", "Este enlace es público")]
    [InlineData("/app/enlaces?c={0}", "cards", true, "public", "Público", "Este enlace es público")]
    [InlineData("/app/enlaces?c={0}", "list", true, "public", "Público", "Este enlace es público")]
    public async Task GetPrivateLinks_RendersOwnPrivacyStatusRegardlessOfCollection(
        string path, string view, bool isPublic, string badgeClass, string label, string tooltip)
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");
        await _keeper.ExecuteAsync(
            @"UPDATE Collection SET IsPublic = @CollectionIsPublic WHERE Id = @CollectionId AND UserId = @UserId;
              UPDATE Link SET IsPublic = @IsPublic WHERE CollectionId = @CollectionId AND UserId = @UserId;",
            new { UserId = _userId, CollectionId = _collectionId, IsPublic = isPublic, CollectionIsPublic = !isPublic });
        using HttpRequestMessage request = new(HttpMethod.Get,
            string.Format(System.Globalization.CultureInfo.InvariantCulture, path, _collectionId));
        request.Headers.Add("Cookie", $"links-view={view}");

        using HttpResponseMessage response = await _client.SendAsync(request);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string card = Regex.Match(html, "<article\\b[^>]*aria-label=\"Enlace: Lectura 01\".*?</article>", RegexOptions.Singleline).Value;

        Assert.Single(Regex.Matches(card, "class=\"link-card__(?:public|private)\""));
        Assert.Matches($"(?s)<span class=\"link-card__{badgeClass}\" title=\"{tooltip}\">\\s*<svg[^>]*aria-hidden=\"true\"[^>]*>.*?</svg>\\s*{label}\\s*</span>", card);
    }

    [Theory]
    [InlineData("/app")]
    [InlineData("/app/enlaces")]
    [InlineData("/app/enlaces?c={0}")]
    public async Task GetPrivateCards_WithoutImage_UsesDecorativeBrandSymbol(string path)
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");

        string html = await _client.GetStringAsync(string.Format(System.Globalization.CultureInfo.InvariantCulture, path, _collectionId));

        Assert.Matches("(?s)<div class=\"link-card__brand-placeholder\" aria-hidden=\"true\">\\s*<span class=\"brand-mark\">\\s*<img class=\"brand-icon\"[^>]*src=\"/brand/symbol(?:\\.[a-z0-9]+)?\\.svg[^>]*alt=\"\"[^>]*>\\s*</span>\\s*</div>", html);
        Assert.DoesNotContain("link-card__thumb-fake-overlay", html, StringComparison.Ordinal);
        Assert.Contains("<h4>Lectura 01</h4>", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/app")]
    [InlineData("/app/enlaces")]
    [InlineData("/app/enlaces?c={0}")]
    public async Task GetPrivateCards_WithImage_PreservesRealImageInsteadOfBrandPlaceholder(string path)
    {
        const string imageUrl = "https://images.example.test/cover.jpg";
        await AddLinks(_userId, _collectionId, 1, "Lectura");
        await _keeper.ExecuteAsync(
            "UPDATE Link SET Image = @Image WHERE UserId = @UserId;",
            new { Image = imageUrl, UserId = _userId });

        string html = await _client.GetStringAsync(string.Format(System.Globalization.CultureInfo.InvariantCulture, path, _collectionId));

        Assert.Contains($"<img src=\"{imageUrl}\" alt=\"Lectura 01\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("link-card__brand-placeholder", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetSidebarCollection_WithLinks_ShowsOnlyOwnedCollectionCardsAcrossPages()
    {
        await AddLinks(_userId, _collectionId, 17, "Lectura");
        await AddLinks(_userId, _otherCollectionId, 1, "Otra");
        await AddLinks(_foreignUserId, _foreignCollectionId, 1, "Privada ajena");
        using HttpResponseMessage dashboard = await _client.GetAsync("/app");
        string sidebar = WebUtility.HtmlDecode(await dashboard.Content.ReadAsStringAsync());
        string anchor = Regex.Match(sidebar, "<a\\b(?=[^>]*aria-label=\"Lecturas,)[^>]*>").Value;
        string destination = Regex.Match(anchor, "href=\"([^\"]+)\"").Groups[1].Value;

        using HttpResponseMessage first = await _client.GetAsync(destination);
        string firstHtml = await first.Content.ReadAsStringAsync();
        using HttpResponseMessage second = await _client.GetAsync(destination + "&p=2");
        string secondHtml = WebUtility.HtmlDecode(await second.Content.ReadAsStringAsync());

        Assert.Equal($"/app/enlaces?c={_collectionId}", destination);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(Enumerable.Range(2, 16).Reverse().Select(index => $"Lectura {index:00}"), CardTitles(firstHtml));
        Assert.Equal(["Lectura 01"], CardTitles(secondHtml));
        Assert.Contains("<h2>Lecturas</h2>", firstHtml, StringComparison.Ordinal);
        Assert.Contains($"href=\"/app/enlaces?c={_collectionId}\" class=\"is-active\"", firstHtml, StringComparison.Ordinal);
        Assert.Contains($"href=\"/app/enlaces?c={_collectionId}&amp;p=2\"", firstHtml, StringComparison.Ordinal);
        Assert.Contains($"href=\"/app/enlaces?c={_collectionId}\">Anterior</a>", secondHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("Privada ajena", WebUtility.HtmlDecode(firstHtml), StringComparison.Ordinal);
        Assert.DoesNotContain("Colección ajena", WebUtility.HtmlDecode(firstHtml), StringComparison.Ordinal);
        Assert.Equal(19, await _keeper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link;"));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 1, 0)]
    [InlineData(15, 15, 0)]
    [InlineData(16, 16, 0)]
    [InlineData(17, 16, 2)]
    [InlineData(31, 16, 2)]
    [InlineData(32, 16, 2)]
    [InlineData(33, 16, 3)]
    public async Task GetLinks_WithDifferentCounts_UsesExactPageBoundaries(int count, int expectedCards, int expectedPageLinks)
    {
        await AddLinks(_userId, _collectionId, count, "Lectura");

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}");
        string html = await response.Content.ReadAsStringAsync();
        string pagination = Regex.Match(html, "<nav\\b[^>]*class=\"pagination\"[^>]*>.*?</nav>", RegexOptions.Singleline).Value;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedCards, CardTitles(html).Length);
        Assert.Equal(expectedPageLinks, Regex.Matches(pagination, ">[0-9]+</(?:a|span)>").Count);
    }

    [Theory]
    [InlineData(16, 1, 16)]
    [InlineData(17, 2, 1)]
    [InlineData(32, 2, 16)]
    [InlineData(33, 3, 1)]
    public async Task GetLinks_WithLastPage_ReturnsExactRemainingLinks(int count, int page, int expectedCards)
    {
        await AddLinks(_userId, _collectionId, count, "Lectura");

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}&p={page}");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Enumerable.Range(1, expectedCards).Reverse().Select(index => $"Lectura {index:00}"), CardTitles(html));
    }

    [Theory]
    [InlineData(16, 2)]
    [InlineData(32, 3)]
    public async Task GetLinks_WithPageAfterExactBoundary_ShowsLastFullPage(int count, int page)
    {
        await AddLinks(_userId, _collectionId, count, "Lectura");
        using HttpResponseMessage baselineResponse = await _client.GetAsync($"/app/enlaces?c={_collectionId}&p={page - 1}");
        string baseline = await baselineResponse.Content.ReadAsStringAsync();

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}&p={page}");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(16, CardTitles(html).Length);
        Assert.Equal(CardTitles(baseline), CardTitles(html));
        Assert.DoesNotContain("Los valores no son correctos.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDashboard_WithMoreLinks_PreservesTwelveRecentLinks()
    {
        await AddLinks(_userId, _collectionId, 17, "Lectura");

        using HttpResponseMessage response = await _client.GetAsync("/app");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Enumerable.Range(6, 12).Reverse().Select(index => $"Lectura {index:00}"), CardTitles(html));
    }

    [Fact]
    public async Task GetLinks_WithDifferentDates_OrdersByDateThenDescendingId()
    {
        await AddLinks(_userId, _collectionId, 3, "Lectura");
        await _keeper.ExecuteAsync(
            "UPDATE Link SET CreatedAt = @CreatedAt WHERE CollectionId = @CollectionId AND Title = 'Lectura 01';",
            new { CreatedAt = DateTime.UtcNow, CollectionId = _collectionId });

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(["Lectura 01", "Lectura 03", "Lectura 02"], CardTitles(html));
    }

    [Theory]
    [InlineData("cancion")]
    [InlineData("CANCIÓN")]
    public async Task GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection(string query)
    {
        await AddLinks(_userId, _collectionId, 17, "Canción");
        await AddLinks(_userId, _collectionId, 1, "Diferente");
        await AddLinks(_userId, _otherCollectionId, 1, "Canción de otra");
        await AddLinks(_foreignUserId, _foreignCollectionId, 1, "Canción privada");
        string route = $"/app/enlaces?c={_collectionId}&q={Uri.EscapeDataString(query)}";

        using HttpResponseMessage first = await _client.GetAsync(route);
        string firstHtml = await first.Content.ReadAsStringAsync();
        using HttpResponseMessage second = await _client.GetAsync(route + "&p=2");
        string secondHtml = await second.Content.ReadAsStringAsync();
        string searchForm = Regex.Match(firstHtml, "<form\\b[^>]*class=\"search-form\"[^>]*>.*?</form>", RegexOptions.Singleline).Value;

        Assert.Equal(Enumerable.Range(2, 16).Reverse().Select(index => $"Canción {index:00}"), CardTitles(firstHtml));
        Assert.Equal(["Canción 01"], CardTitles(secondHtml));
        Assert.Contains($"href=\"/app/enlaces?c={_collectionId}&amp;q={Uri.EscapeDataString(query)}&amp;p=2\"", firstHtml, StringComparison.Ordinal);
        Assert.Contains($"name=\"c\" value=\"{_collectionId}\"", searchForm, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"p\"", searchForm, StringComparison.Ordinal);
        Assert.Contains($"value=\"{query}\"", WebUtility.HtmlDecode(searchForm), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task GetLinks_WithoutCollection_RendersCreateToolWithoutPreselection(int count)
    {
        await AddLinks(_userId, _collectionId, count, "Lectura");

        using HttpResponseMessage response = await _client.GetAsync("/app/enlaces");
        string html = await response.Content.ReadAsStringAsync();
        string anchor = Regex.Match(html, "<a\\b(?=[^>]*class=\"[^\"]*\\btoolbar-create-link\\b)[^>]*>").Value;
        string destination = WebUtility.HtmlDecode(Regex.Match(anchor, "href=\"([^\"]+)\"").Groups[1].Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("class=\"view-switch__button toolbar-tool toolbar-create-link\"", anchor, StringComparison.Ordinal);
        Assert.Equal("/app/enlaces/nuevo?ru=%2Fapp%2Fenlaces", destination);
    }

    [Theory]
    [InlineData("sidebar-create-link", "primary-button")]
    [InlineData("toolbar-create-link", "view-switch__button toolbar-tool")]
    public async Task GetLinks_WithCollectionAndPage_CreateActionsPreserveContext(string buttonClass, string expectedStyle)
    {
        await AddLinks(_userId, _collectionId, 17, "Lectura");
        string origin = $"/app/enlaces?c={_collectionId}&q=Lectura&p=2";

        using HttpResponseMessage source = await _client.GetAsync(origin);
        string html = await source.Content.ReadAsStringAsync();
        string anchor = Regex.Match(html, $"<a\\b(?=[^>]*class=\"[^\"]*\\b{buttonClass}\\b)[^>]*>").Value;
        string destination = WebUtility.HtmlDecode(Regex.Match(anchor, "href=\"([^\"]+)\"").Groups[1].Value);
        using HttpResponseMessage form = await _client.GetAsync(destination);
        string formHtml = WebUtility.HtmlDecode(await form.Content.ReadAsStringAsync());
        string selected = Regex.Match(formHtml, $"<option\\b(?=[^>]*value=\"{_collectionId}\")[^>]*>").Value;

        Assert.Equal(HttpStatusCode.OK, form.StatusCode);
        Assert.Contains($"class=\"{expectedStyle} {buttonClass}\"", anchor, StringComparison.Ordinal);
        Assert.Contains("selected=\"selected\"", selected, StringComparison.Ordinal);
        Assert.Contains($"name=\"ru\" value=\"{origin}\"", formHtml, StringComparison.Ordinal);
        Assert.Contains($"class=\"secondary-button\" href=\"{origin}\"", formHtml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("list", true)]
    [InlineData("cards", false)]
    public async Task GetLinks_WithCollectionSearchAndPage_RestoresPresentationWithoutChangingResults(string preference, bool isList)
    {
        await AddLinks(_userId, _collectionId, 17, "Lectura");
        await AddLinks(_userId, _otherCollectionId, 1, "Lectura de otra");
        await AddLinks(_foreignUserId, _foreignCollectionId, 1, "Lectura ajena");
        string origin = $"/app/enlaces?c={_collectionId}&q=Lectura&p=2";
        using HttpRequestMessage request = new(HttpMethod.Get, origin);
        request.Headers.Add("Cookie", $"links-view={preference}");

        using HttpResponseMessage response = await _client.SendAsync(request);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string container = Regex.Match(html, "<div\\b(?=[^>]*id=\"saved-links\")[^>]*>").Value;
        string listButton = Regex.Match(html, "<button\\b(?=[^>]*data-link-view=\"list\")[^>]*>").Value;
        string searchForm = Regex.Match(html, "<form\\b[^>]*class=\"search-form\"[^>]*>.*?</form>", RegexOptions.Singleline).Value;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(container);
        Assert.Equal(isList, container.Contains("links-grid--list", StringComparison.Ordinal));
        Assert.Contains($"aria-pressed=\"{isList.ToString().ToLowerInvariant()}\"", listButton, StringComparison.Ordinal);
        Assert.Equal(["Lectura 01"], CardTitles(html));
        Assert.Contains($"href=\"/app/enlaces?c={_collectionId}&q=Lectura\">Anterior</a>", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"c\" value=\"{_collectionId}\"", searchForm, StringComparison.Ordinal);
        Assert.Contains($"ru={Uri.EscapeDataString(origin)}", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public async Task GetLinks_WithForeignOrMissingCollection_IgnoresFilterWithoutDisclosingPrivateData(int offset)
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");
        await AddLinks(_userId, _otherCollectionId, 1, "Otra");
        await AddLinks(_foreignUserId, _foreignCollectionId, 1, "Privada ajena");

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_foreignCollectionId + offset}&userId={_foreignUserId}");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Otra 01", "Lectura 01"], CardTitles(html));
        Assert.Contains("<h2>Mis enlaces</h2>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Privada ajena", WebUtility.HtmlDecode(html), StringComparison.Ordinal);
        Assert.DoesNotContain("Colección ajena", WebUtility.HtmlDecode(html), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("c=not-an-id")]
    [InlineData("c=9223372036854775808")]
    [InlineData("c=not-an-id&p=abc")]
    public async Task GetLinks_WithInvalidParameters_ShowsAccessibleErrorInsteadOfUnfilteredCards(string query)
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");

        using HttpResponseMessage response = await _client.GetAsync("/app/enlaces?" + query);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Los valores no son correctos.", html, StringComparison.Ordinal);
        Assert.Matches("(?s)<section aria-labelledby=\"links-list-title\">.*?<h3 id=\"links-list-title\">Enlaces</h3>.*?<div class=\"app-alert app-alert--error\"[^>]*>.*?Los valores no son correctos\\.", html);
        Assert.DoesNotContain("class=\"form-summary\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("No se ha podido cargar el listado de enlaces.", html, StringComparison.Ordinal);
        Assert.Matches("(?s)<section aria-labelledby=\"links-list-title\">.*?class=\"app-alert app-alert--error\".*?<div class=\"empty-state\" role=\"status\" aria-live=\"polite\">\\s*<h2>No se pueden mostrar los resultados</h2>\\s*<p>Corrige los errores indicados para ver el listado\\.</p>", html);
        Assert.Matches("(?s)<div class=\"empty-state\"[^>]*>.*?<a class=\"secondary-button\" href=\"/app/enlaces\">Volver al listado</a>.*?</div>", html);
        Assert.DoesNotContain("No se han encontrado enlaces.", html, StringComparison.Ordinal);
        Assert.Empty(CardTitles(html));
        Assert.DoesNotContain("data-link-view-switch", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("abc", 1)]
    [InlineData("0", 1)]
    [InlineData("-1", 1)]
    [InlineData("abc&p=2", 1)]
    [InlineData("000000000000000000000000000002", 2)]
    [InlineData("999", 2)]
    [InlineData("2147483648", 2)]
    [InlineData("999999999999999999999999999", 2)]
    public async Task GetLinks_WithInvalidOrOutOfRangePage_UsesExpectedFilteredPage(string page, int expectedPage)
    {
        await AddLinks(_userId, _collectionId, 18, "Lectura");
        await AddLinks(_userId, _otherCollectionId, 1, "Lectura otra");
        await AddLinks(_foreignUserId, _foreignCollectionId, 1, "Lectura ajena");
        string query = $"c={_collectionId}&q=Lectura";
        using HttpResponseMessage baselineResponse = await _client.GetAsync($"/app/enlaces?{query}&p={expectedPage}");
        string baseline = WebUtility.HtmlDecode(await baselineResponse.Content.ReadAsStringAsync());

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?{query}&p={page}");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedPage == 1 ? 16 : 2, CardTitles(html).Length);
        Assert.Equal(CardTitles(baseline), CardTitles(html));
        Assert.Equal(expectedPage == 1
            ? Enumerable.Range(3, 16).Reverse().Select(index => $"Lectura {index:00}")
            : new[] { "Lectura 02", "Lectura 01" }, CardTitles(html));
        Assert.Contains("value=\"Lectura\"", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"c\" value=\"{_collectionId}\"", html, StringComparison.Ordinal);
        Assert.Contains($"aria-current=\"page\">{expectedPage}</span>", html, StringComparison.Ordinal);
        Assert.Contains($"/app/enlaces?c={_collectionId}&q=Lectura", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Los valores no son correctos.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetLinks_WithOutOfRangePageAndNoMatches_ShowsFilteredEmptyState()
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}&q=inexistente&p=999");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No se han encontrado enlaces.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("No se pueden mostrar los resultados", html, StringComparison.Ordinal);
        Assert.DoesNotContain(">Volver al listado</a>", html, StringComparison.Ordinal);
        Assert.Contains("value=\"inexistente\"", html, StringComparison.Ordinal);
        Assert.Contains($"name=\"c\" value=\"{_collectionId}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Los valores no son correctos.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetLinks_WithOversizedSearch_ShowsErrorWithoutResults()
    {
        await AddLinks(_userId, _collectionId, 1, "Control");
        using HttpResponseMessage control = await _client.GetAsync("/app/enlaces");
        Assert.Single(CardTitles(await control.Content.ReadAsStringAsync()));

        string query = new('a', 101);
        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}&p=abc&q={query}");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string form = Regex.Match(html, "(?s)<form class=\"search-form\".*?</form>").Value;

        Assert.Contains("El texto de búsqueda supera la longitud máxima permitida.", form, StringComparison.Ordinal);
        Assert.Contains("id=\"links-search-query-error\" role=\"alert\"", form, StringComparison.Ordinal);
        Assert.Contains("href=\"#links-search-query\"", form, StringComparison.Ordinal);
        Assert.Contains("aria-invalid=\"true\" aria-describedby=\"links-search-query-error\"", form, StringComparison.Ordinal);
        Assert.Contains($"value=\"{query}\"", form, StringComparison.Ordinal);
        Assert.Contains($"name=\"c\" value=\"{_collectionId}\"", form, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"app-alert app-alert--error\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("No se ha podido cargar el listado de enlaces.", html, StringComparison.Ordinal);
        Assert.DoesNotContain(">Volver al listado</a>", html, StringComparison.Ordinal);
        Assert.Matches("(?s)<section aria-labelledby=\"links-list-title\">.*?<div class=\"empty-state\" role=\"status\" aria-live=\"polite\">\\s*<h2>No se pueden mostrar los resultados</h2>\\s*<p>Corrige los errores indicados para ver el listado\\.</p>", html);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(CardTitles(html));
        Assert.DoesNotContain("data-link-view-switch", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetLinks_WithResourceNoticeAndInvalidFilters_ShowsSeparateSearchErrorAndCombinedAlert()
    {
        using HttpResponseMessage redirect = await _client.GetAsync("/app/enlaces/-1/editar");
        string query = new('a', 101);
        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c=abc&q={query}");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string form = Regex.Match(html, "(?s)<form class=\"search-form\".*?</form>").Value;
        string alert = Regex.Match(html, "<div\\b[^>]*class=\"app-alert app-alert--error\"[^>]*>.*?</div>", RegexOptions.Singleline).Value;

        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(Regex.Matches(html, "class=\"app-alert app-alert--error\""));
        Assert.Contains("No se ha podido acceder al elemento solicitado.", alert, StringComparison.Ordinal);
        Assert.Contains("Los valores no son correctos.", alert, StringComparison.Ordinal);
        Assert.DoesNotContain("El texto de búsqueda supera", alert, StringComparison.Ordinal);
        Assert.Contains("El texto de búsqueda supera la longitud máxima permitida.", form, StringComparison.Ordinal);
        Assert.Contains($"value=\"{query}\"", form, StringComparison.Ordinal);
        Assert.DoesNotContain("Los valores no son correctos.", form, StringComparison.Ordinal);
        Assert.Contains("No se pueden mostrar los resultados", html, StringComparison.Ordinal);
        Assert.Contains("Corrige los errores indicados para ver el listado.", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/app/enlaces\">Volver al listado</a>", html, StringComparison.Ordinal);
        Assert.Empty(CardTitles(html));
    }

    [Fact]
    public async Task GetLinks_WithInvalidCollectionFilter_ReturnLinkClearsFiltersAndShowsLinks()
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");
        string query = new('a', 101);
        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c=abc&q={query}&p=3");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string returnUrl = Regex.Match(html, "<a\\b[^>]*href=\"([^\"]+)\"[^>]*>Volver al listado</a>").Groups[1].Value;

        Assert.Equal("/app/enlaces", returnUrl);
        using HttpResponseMessage listing = await _client.GetAsync(returnUrl);
        string listingHtml = WebUtility.HtmlDecode(await listing.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, listing.StatusCode);
        Assert.Equal(["Lectura 01"], CardTitles(listingHtml));
        Assert.DoesNotContain("Los valores no son correctos.", listingHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("El texto de búsqueda supera", listingHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetLinks_WithRepeatedAndUnsupportedParameters_UsesOnlyFirstApplicableValues()
    {
        await AddLinks(_userId, _collectionId, 17, "Lectura");
        await AddLinks(_userId, _otherCollectionId, 1, "Otra");

        using HttpResponseMessage response = await _client.GetAsync(
            $"/app/enlaces?c={_collectionId}&c={_otherCollectionId}&p=2&p=1&q=Lectura&q=Otra&e=invalid&unknown=invalid");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Lectura 01"], CardTitles(html));
        Assert.DoesNotContain("Los valores no son correctos.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetLinks_WithEmptyCollection_ShowsEmptyStateNotOtherCollectionLinks()
    {
        await AddLinks(_userId, _otherCollectionId, 1, "Otra");

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("No se han encontrado enlaces.", html, StringComparison.Ordinal);
        Assert.Empty(CardTitles(html));
        Assert.DoesNotContain("No se ha podido cargar el listado", html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-link-view-switch", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("editar")]
    [InlineData("mover")]
    [InlineData("eliminar")]
    public async Task GetLinks_WithCardActions_PreservesCollectionAndPageInFormAndCancel(string action)
    {
        await AddLinks(_userId, _collectionId, 17, "Lectura");
        string origin = $"/app/enlaces?c={_collectionId}&p=2";
        using HttpResponseMessage response = await _client.GetAsync(origin);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string destination = Regex.Match(html, $"href=\"([^\"]+/{action}\\?ru=[^\"]+)\"").Groups[1].Value;

        using HttpResponseMessage form = await _client.GetAsync(destination);
        string formHtml = WebUtility.HtmlDecode(await form.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, form.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{origin}\"", formHtml, StringComparison.Ordinal);
        Assert.Contains($"class=\"secondary-button\" href=\"{origin}\"", formHtml, StringComparison.Ordinal);
    }

    private static string[] CardTitles(string html)
        => Regex.Matches(html, "<article\\b[^>]*aria-label=\"Enlace: ([^\"]*)\"")
            .Select(match => WebUtility.HtmlDecode(match.Groups[1].Value)).ToArray();

    [Fact]
    public async Task PostLinks_WithAntiforgeryToken_ReturnsNotFoundWithoutChangingData()
    {
        using HttpResponseMessage form = await _client.GetAsync("/app/enlaces/nuevo");
        string html = await form.Content.ReadAsStringAsync();
        string input = Regex.Match(html, "<input\\b(?=[^>]*name=\"__RequestVerificationToken\")[^>]*>").Value;
        string token = WebUtility.HtmlDecode(Regex.Match(input, "value=\"([^\"]+)\"").Groups[1].Value);

        using HttpResponseMessage response = await _client.PostAsync("/app/enlaces", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, await _keeper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link;"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("&q=%20%20")]
    [InlineData("&q=---%2A%22")]
    public async Task GetLinks_WithNoSearchTerms_PreservesCollectionAndPagination(string query)
    {
        await AddLinks(_userId, _collectionId, 17, "Control");
        await AddLinks(_userId, _otherCollectionId, 1, "Excluded");

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}{query}");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Enumerable.Range(2, 16).Reverse().Select(index => $"Control {index:00}"), CardTitles(html));
        Assert.Contains($"name=\"c\" value=\"{_collectionId}\"", html, StringComparison.Ordinal);
        string href = Regex.Match(html, "<a[^>]*href=\"([^\"]+)\"[^>]*>2</a>").Groups[1].Value;
        Assert.NotEmpty(href);
        using HttpResponseMessage second = await _client.GetAsync(WebUtility.HtmlDecode(href));
        Assert.Equal(new[] { "Control 01" }, CardTitles(await second.Content.ReadAsStringAsync()));
    }

    private Task<long> AddCollection(long userId, string name)
        => _keeper.ExecuteScalarAsync<long>(
            "INSERT INTO Collection (UserId, Name, Slug, IsPublic) VALUES (@UserId, @Name, @Slug, 0); SELECT last_insert_rowid();",
            new { UserId = userId, Name = name, Slug = Guid.NewGuid().ToString("N") });

    private async Task AddLinks(long userId, long collectionId, int count, string prefix)
    {
        string name = await _keeper.ExecuteScalarAsync<string>("SELECT Name FROM Collection WHERE Id = @Id;", new { Id = collectionId })
            ?? throw new InvalidOperationException("The test collection must exist before seeding links.");
        for (int index = 1; index <= count; index++)
        {
            string title = $"{prefix} {index:00}";
            string url = $"https://example.com/{collectionId}/{Guid.NewGuid():N}";
            long id = await _keeper.ExecuteScalarAsync<long>(
                @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt)
                  VALUES (@UserId, @CollectionId, @Url, @Url, @Title, 0, @CreatedAt); SELECT last_insert_rowid();",
                new { UserId = userId, CollectionId = collectionId, Url = url, Title = title, CreatedAt = DateTime.UnixEpoch });
            await _keeper.ExecuteAsync(
                @"INSERT INTO LinkSearch (rowid, UserId, UrlOriginal, Title, CollectionName)
                  VALUES (@Id, @UserId, @Url, @Title, @CollectionName);",
                new { Id = id, UserId = userId, Url = url, Title = SearchTextNormalizer.Normalize(title), CollectionName = SearchTextNormalizer.Normalize(name) });
        }
    }

    private sealed class FailingLinkService : LinkService
    {
        public Task<ApplicationResults.Result<ApplicationDtos.LinkPageDto>> ListLinksPage(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default)
            => Task.FromResult(ApplicationResults.Result<ApplicationDtos.LinkPageDto>.Failure(
                ApplicationResults.ApplicationError.Domain("FIRST_LIST_ERROR", message: "Primer error de listado."),
                ApplicationResults.ApplicationError.Domain("SECOND_LIST_ERROR")));

        public Task<ApplicationResults.Result<ApplicationDtos.LinkDto>> GetLink(ApplicationRequests.LinkGetRequest? request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ApplicationResults.Result<ApplicationDtos.LinkDto>> CreateLink(ApplicationRequests.LinkCreateRequest? request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ApplicationResults.Result<ApplicationDtos.LinkDto>> UpdateLink(ApplicationRequests.LinkUpdateRequest? request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ApplicationResults.Result<ApplicationDtos.LinkDto>> MoveLink(ApplicationRequests.LinkMoveRequest? request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ApplicationResults.Result> DeleteLink(ApplicationRequests.LinkDeleteRequest? request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ApplicationResults.Result<IReadOnlyList<ApplicationDtos.LinkDto>>> ListLinks(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class TestCurrentUser(long userId) : CurrentUser
    {
        public long UserId { get; } = userId;

        public bool IsAuthenticated { get; set; } = true;
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
