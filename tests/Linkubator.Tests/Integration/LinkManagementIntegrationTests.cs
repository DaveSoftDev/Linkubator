using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Dapper;
using Linkubator.Infrastructure.Search;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using TestHost = Microsoft.AspNetCore.Hosting;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public sealed class LinkManagementIntegrationTests : IAsyncLifetime
{
    private const string DemoEmail = "user@linkubator.test";
    private const string TempDataCookieName = ".AspNetCore.Mvc.CookieTempDataProvider";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";
    private const string UnsupportedUrlSchemeCode = "UrlSchemeNotAllowed";
    private const string UnsupportedUrlSchemeMessage = "Introduce una dirección que empiece por https:// o http://.";
    private const string CollectionSelectionInvalidMessage = "La colección seleccionada no es válida.";
    private readonly SqliteConnectionFactory _connections = new($"Data Source=link-management-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
    private SqliteConnection _keeper = null!;
    private ConfiguredWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private long _userId;

    public async Task InitializeAsync()
    {
        _keeper = _connections.CreateOpenConnection();
        _factory = new ConfiguredWebApplicationFactory(_keeper.ConnectionString);
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        _userId = await _keeper.ExecuteScalarAsync<long>(
            "SELECT Id FROM User WHERE Email = @Email;",
            new { Email = DemoEmail });
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _keeper.DisposeAsync();
    }

    [Theory]
    [InlineData("list", true)]
    [InlineData("cards", false)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public async Task GetDashboard_WithViewPreference_RendersSavedPresentationAndAccessibleControls(string preference, bool isList)
    {
        long collectionId = await AddCollection(_userId, "Vista", false);
        await AddLink(collectionId, "Enlace de prueba", null, null, false);
        using HttpRequestMessage request = new(HttpMethod.Get, "/app");
        request.Headers.Add("Cookie", $"links-view={preference}");

        using HttpResponseMessage response = await _client.SendAsync(request);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string container = Regex.Match(html, "<div\\b(?=[^>]*id=\"dashboard-links\")[^>]*>").Value;
        string cardsButton = Regex.Match(html, "<button\\b(?=[^>]*data-link-view=\"cards\")[^>]*>").Value;
        string listButton = Regex.Match(html, "<button\\b(?=[^>]*data-link-view=\"list\")[^>]*>").Value;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(container);
        Assert.NotEmpty(cardsButton);
        Assert.NotEmpty(listButton);
        Assert.Equal(isList, container.Contains("links-grid--list", StringComparison.Ordinal));
        Assert.Contains($"aria-pressed=\"{isList.ToString().ToLowerInvariant()}\"", listButton, StringComparison.Ordinal);
        Assert.Contains($"aria-pressed=\"{(!isList).ToString().ToLowerInvariant()}\"", cardsButton, StringComparison.Ordinal);
        Assert.Contains("aria-controls=\"dashboard-links\"", listButton, StringComparison.Ordinal);
        Assert.Contains("data-view-preference-path=\"/app\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-cookie-path", html, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(html, "<article\\b[^>]*class=\"link-card\""));
    }

    [Fact]
    public async Task GetDashboard_WithoutLinks_DoesNotOfferPresentationSwitch()
    {
        using HttpResponseMessage response = await _client.GetAsync("/app");
        string html = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("data-link-view-switch", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"dashboard-links\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("list", true)]
    [InlineData("cards", false)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public async Task GetLinks_WithViewPreference_RendersSavedPresentationAndAccessibleControls(string preference, bool isList)
    {
        long collectionId = await AddCollection(_userId, "Vista", false);
        await AddLink(collectionId, "Enlace de prueba", null, null, false);
        using HttpRequestMessage request = new(HttpMethod.Get, "/app/enlaces");
        request.Headers.Add("Cookie", $"links-view={preference}");

        using HttpResponseMessage response = await _client.SendAsync(request);
        string html = await response.Content.ReadAsStringAsync();
        string container = Regex.Match(html, "<div\\b(?=[^>]*id=\"saved-links\")[^>]*>").Value;
        string cardsButton = Regex.Match(html, "<button\\b(?=[^>]*data-link-view=\"cards\")[^>]*>").Value;
        string listButton = Regex.Match(html, "<button\\b(?=[^>]*data-link-view=\"list\")[^>]*>").Value;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(container);
        Assert.NotEmpty(cardsButton);
        Assert.NotEmpty(listButton);
        Assert.Equal(isList, container.Contains("links-grid--list", StringComparison.Ordinal));
        Assert.Contains("data-link-view-container", container, StringComparison.Ordinal);
        Assert.Contains($"aria-pressed=\"{isList.ToString().ToLowerInvariant()}\"", listButton, StringComparison.Ordinal);
        Assert.Contains($"aria-pressed=\"{(!isList).ToString().ToLowerInvariant()}\"", cardsButton, StringComparison.Ordinal);
        Assert.Contains("aria-controls=\"saved-links\"", listButton, StringComparison.Ordinal);
        Assert.Contains("data-view-preference-path=\"/app\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-cookie-path", html, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(html, "<article\\b[^>]*class=\"link-card\""));
    }

    [Theory]
    [InlineData("/app", false, false)]
    [InlineData("/app", true, false)]
    [InlineData("/app", true, true)]
    [InlineData("/app/enlaces", false, false)]
    [InlineData("/app/enlaces", true, false)]
    [InlineData("/app/enlaces", true, true)]
    public async Task GetLinkCards_WithPrivacy_ShowsPublicBadgeOnlyForPublicLink(string route, bool publicCollection, bool publicLink)
    {
        long collectionId = await AddCollection(_userId, "Estado", publicCollection);
        await AddLink(collectionId, "Estado del enlace", null, null, publicLink);

        using HttpResponseMessage response = await _client.GetAsync(route);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string card = Regex.Match(html, "<article\\b[^>]*aria-label=\"Enlace: Estado del enlace\"[^>]*>.*?</article>", RegexOptions.Singleline).Value;
        string badge = Regex.Match(card, "<span\\b[^>]*class=\"link-card__public\"[^>]*>.*?</span>", RegexOptions.Singleline).Value;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(card);
        Assert.Equal(publicLink, badge.Contains("Público", StringComparison.Ordinal));
        Assert.Equal(publicLink, badge.Contains("title=\"Este enlace es público\"", StringComparison.Ordinal));
        Assert.Equal(publicLink, badge.Contains("aria-hidden=\"true\"", StringComparison.Ordinal));
        Assert.DoesNotContain("Privado", card, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("editar", "Editar enlace", "id=\"link-edit-form\"", "Editar enlace")]
    [InlineData("mover", "Mover enlace", "Elegir colección de destino", "Mover a otra colección")]
    [InlineData("eliminar", "Eliminar enlace", "¿Seguro que quieres eliminar el enlace", "Eliminar enlace")]
    public async Task GetDashboard_CardActions_OpenMatchingLinkFormWithoutChangingData(string action, string label, string formMarker, string tooltip)
    {
        const string origin = "/app?section=recent";
        long collectionId = await AddCollection(_userId, "Colección propia", false);
        long firstLinkId = await AddLink(collectionId, "Primer enlace", null, null, false, url: "https://example.com/primero");
        long secondLinkId = await AddLink(collectionId, "Segundo enlace", null, null, false, url: "https://example.com/segundo");
        long foreignUserId = await AddOtherUser();
        long foreignCollectionId = await AddCollection(foreignUserId, "Colección ajena", false);
        long foreignLinkId = await AddLink(foreignCollectionId, "Enlace privado ajeno", null, null, false, foreignUserId);

        using HttpResponseMessage response = await _client.GetAsync(origin);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        string firstCard = Regex.Match(html, "<article\\b[^>]*aria-label=\"Enlace: Primer enlace\"[^>]*>.*?</article>", RegexOptions.Singleline).Value;
        string secondCard = Regex.Match(html, "<article\\b[^>]*aria-label=\"Enlace: Segundo enlace\"[^>]*>.*?</article>", RegexOptions.Singleline).Value;
        string firstAction = Regex.Match(firstCard, $"<a\\b(?=[^>]*aria-label=\"{label} Primer enlace\")[^>]*>").Value;
        string secondAction = Regex.Match(secondCard, $"<a\\b(?=[^>]*aria-label=\"{label} Segundo enlace\")[^>]*>").Value;
        string destination = Regex.Match(firstAction, "href=\"([^\"]+)\"").Groups[1].Value;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"/app/enlaces/{firstLinkId}/{action}?ru={Uri.EscapeDataString(origin)}", destination);
        Assert.Contains($"title=\"{tooltip}\"", firstAction, StringComparison.Ordinal);
        Assert.Contains($"href=\"/app/enlaces/{secondLinkId}/{action}?ru={Uri.EscapeDataString(origin)}\"", secondAction, StringComparison.Ordinal);
        Assert.Contains($"title=\"{tooltip}\"", secondAction, StringComparison.Ordinal);
        Assert.Equal(
            ["Editar enlace Primer enlace", "Mover enlace Primer enlace", "Eliminar enlace Primer enlace"],
            Regex.Matches(firstCard, "<a\\b[^>]*aria-label=\"((?:Editar|Mover|Eliminar) enlace Primer enlace)\"[^>]*>")
                .Select(match => match.Groups[1].Value).ToArray());
        Assert.DoesNotContain("class=\"link-card__actions\" aria-hidden", firstCard, StringComparison.Ordinal);
        Assert.DoesNotContain("Enlace privado ajeno", html, StringComparison.Ordinal);
        Assert.DoesNotContain($"/app/enlaces/{foreignLinkId}/{action}", html, StringComparison.Ordinal);

        using HttpResponseMessage formResponse = await _client.GetAsync(destination);
        string form = WebUtility.HtmlDecode(await formResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, formResponse.StatusCode);
        Assert.Contains(formMarker, form, StringComparison.Ordinal);
        Assert.Contains("Primer enlace", form, StringComparison.Ordinal);
        Assert.Contains($"name=\"ru\" value=\"{origin}\"", form, StringComparison.Ordinal);
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{origin}\"", form, StringComparison.Ordinal);
        Assert.Equal(1, await LinkCount(firstLinkId));
        Assert.Equal(1, await LinkCount(secondLinkId));
    }

    [Theory]
    [InlineData("/app", "/app")]
    [InlineData("/APP/", "/APP/")]
    [InlineData("/app?section=recent#links", "/app?section=recent#links")]
    [InlineData("/app/enlaces?q=uno%26dos&c=2&p=3", "/app/enlaces?q=uno%26dos&c=2&p=3")]
    [InlineData("https://example.com/app", "/app/enlaces")]
    [InlineData("//example.com/app", "/app/enlaces")]
    [InlineData("/app/../../outside", "/app/enlaces")]
    [InlineData("/app/%2e%2e/outside", "/app/enlaces")]
    [InlineData("/app\\..\\outside", "/app/enlaces")]
    [InlineData("/application", "/app/enlaces")]
    [InlineData("/app/colecciones", "/app/enlaces")]
    [InlineData("/app?x=\r\nLocation: https://example.com", "/app/enlaces")]
    public async Task GetEditLink_WithReturnUrl_RendersValidatedCancelAndPostDestination(string returnUrl, string expected)
    {
        long collectionId = await AddCollection(_userId, "Colección", false);
        long linkId = await AddLink(collectionId, "Título", null, null, false);

        using HttpResponseMessage response = await _client.GetAsync(EditRoute(linkId) + "?ru=" + Uri.EscapeDataString(returnUrl));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{expected}\"", html, StringComparison.Ordinal);
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{expected}\"", html, StringComparison.Ordinal);
        Assert.Equal(1, await LinkCount(linkId));
    }

    [Theory]
    [InlineData("/app", "/app")]
    [InlineData("/app/enlaces?q=uno%26dos&c=2&p=3", "/app/enlaces?q=uno%26dos&c=2&p=3")]
    [InlineData("https://example.com/app", "/app/enlaces")]
    [InlineData("//example.com/app", "/app/enlaces")]
    [InlineData("/app/../../outside", "/app/enlaces")]
    [InlineData("/app\\..\\outside", "/app/enlaces")]
    [InlineData("/app?x=\r\nLocation: https://example.com", "/app/enlaces")]
    public async Task PostEditLink_WithReturnUrl_ReturnsToValidatedOrigin(string returnUrl, string expected)
    {
        long collectionId = await AddCollection(_userId, "Colección", false);
        long linkId = await AddLink(collectionId, "Título", null, null, false);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", collectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Título actualizado"),
            new("ru", returnUrl));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expected, response.Headers.Location?.OriginalString);
        Assert.Equal("Título actualizado", (await GetLink(linkId))?.Title);
    }

    [Fact]
    public async Task PostEditLink_WithValidationError_KeepsOriginForCancelAndRetry()
    {
        const string returnUrl = "/app?section=recent";
        long collectionId = await AddCollection(_userId, "Colección", false);
        long linkId = await AddLink(collectionId, "Título", null, null, false);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", collectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Cambio pendiente"),
            new("Input.Image", "file:///invalid.png"),
            new("ru", returnUrl));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{returnUrl}\"", html, StringComparison.Ordinal);
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{returnUrl}\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"#link-image\"", html, StringComparison.Ordinal);
        Assert.Equal("Título", (await GetLink(linkId))?.Title);
    }

    [Fact]
    public async Task PostMoveLink_WithDashboardOrigin_ReturnsToDashboard()
    {
        long sourceId = await AddCollection(_userId, "Origen", false);
        long destinationId = await AddCollection(_userId, "Destino", false);
        long linkId = await AddLink(sourceId, "Título", null, null, false);

        using HttpResponseMessage response = await PostWithToken(
            MoveRoute(linkId),
            MoveRoute(linkId),
            new("Input.DestinationCollectionId", destinationId.ToString(CultureInfo.InvariantCulture)),
            new("ru", "/app"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app", response.Headers.Location?.OriginalString);
        Assert.Equal(destinationId, (await GetLink(linkId))?.CollectionId);
    }

    [Fact]
    public async Task PostDeleteLink_WithDashboardOrigin_ReturnsToDashboard()
    {
        long collectionId = await AddCollection(_userId, "Colección", false);
        long linkId = await AddLink(collectionId, "Título", null, null, false);

        using HttpResponseMessage response = await PostWithToken(
            DeleteRoute(linkId),
            DeleteRoute(linkId),
            new KeyValuePair<string, string>("ru", "/app"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app", response.Headers.Location?.OriginalString);
        Assert.Equal(0, await LinkCount(linkId));
    }

    [Fact]
    public async Task PostDeleteLink_WithForeignLinkAndDashboardOrigin_ShowsNoticeOnDashboard()
    {
        long foreignUserId = await AddOtherUser();
        long collectionId = await AddCollection(foreignUserId, "Colección ajena", false);
        long linkId = await AddLink(collectionId, "Título privado", null, null, false, foreignUserId);

        using HttpResponseMessage response = await PostWithToken(
            "/app",
            DeleteRoute(linkId),
            new KeyValuePair<string, string>("ru", "/app"));
        using HttpResponseMessage dashboard = await _client.GetAsync("/app");
        string html = WebUtility.HtmlDecode(await dashboard.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app", response.Headers.Location?.OriginalString);
        Assert.Contains("No se ha podido eliminar el enlace.", html, StringComparison.Ordinal);
        Assert.Contains("role=\"status\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Título privado", html, StringComparison.Ordinal);
        Assert.Equal(1, await LinkCount(linkId));
    }

    [Fact]
    public async Task GetEditLink_LoadsEditableFieldsButDoesNotOfferUrlAsInput()
    {
        long collectionId = await AddCollection(_userId, "Colección pública", true);
        long otherCollectionId = await AddCollection(_userId, "Otra colección", false);
        long linkId = await AddLink(collectionId, "Título", "Descripción", "https://example.com/image.png", false);

        using HttpResponseMessage response = await _client.GetAsync(EditRoute(linkId));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        LinkRow? link = await GetLink(linkId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("name=\"Input.CollectionId\"", html, StringComparison.Ordinal);
        Assert.Contains($"data-current-collection-id=\"{collectionId.ToString(CultureInfo.InvariantCulture)}\"", html, StringComparison.Ordinal);
        Assert.Matches(
            $"<option\\b(?=[^>]*value=\"{collectionId.ToString(CultureInfo.InvariantCulture)}\")(?=[^>]*selected=\"selected\")[^>]*>",
            html);
        Assert.Contains($"value=\"{otherCollectionId.ToString(CultureInfo.InvariantCulture)}\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Title\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Description\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Image\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.IsPublic\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Input.Url\"", html, StringComparison.Ordinal);
        Assert.Contains("https://example.com/original", html, StringComparison.Ordinal);
        Assert.Contains("noindex, nofollow", html, StringComparison.Ordinal);
        Assert.Equal("Título", link?.Title);
    }

    [Fact]
    public async Task GetLinkManagementPages_WithForeignLink_ShowsGenericUnavailableResource()
    {
        long foreignUserId = await AddOtherUser();
        long foreignCollectionId = await AddCollection(foreignUserId, "Colección privada ajena", false);
        long foreignLinkId = await AddLink(foreignCollectionId, "Título privado", null, null, false, foreignUserId);

        foreach (long requestedLinkId in new[] { foreignLinkId, 999999L })
        {
            foreach (string route in new[] { EditRoute(requestedLinkId), MoveRoute(requestedLinkId), DeleteRoute(requestedLinkId) })
            {
                using HttpResponseMessage response = await _client.GetAsync(route);
                string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("No se ha podido acceder al elemento solicitado.", html, StringComparison.Ordinal);
                Assert.DoesNotContain("Título privado", html, StringComparison.Ordinal);
                Assert.DoesNotContain("Colección privada ajena", html, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public async Task GetLinkManagementPages_WithInvalidIdDoesNotLoadOrChangeALink()
    {
        long collectionId = await AddCollection(_userId, "Colección propia", false);
        long linkId = await AddLink(collectionId, "Título", null, null, false);

        foreach (string route in new[] { EditRoute(0), MoveRoute(0), DeleteRoute(0) })
        {
            using HttpResponseMessage response = await _client.GetAsync(route);
            string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("El identificador del enlace no es válido.", html, StringComparison.Ordinal);
            Assert.DoesNotContain("name=\"Input.Url\"", html, StringComparison.Ordinal);
            Assert.Equal("Título", (await GetLink(linkId))?.Title);
        }
    }

    [Fact]
    public async Task PostEditLink_UpdatesEditableFieldsAndKeepsOriginalUrl()
    {
        long collectionId = await AddCollection(_userId, "Colección pública", true);
        long linkId = await AddLink(collectionId, "Anterior", "Descripción anterior", "https://example.com/old.png", false);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", collectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Nuevo título"),
            new("Input.Description", "Nueva descripción"),
            new("Input.Image", "https://example.com/new.png"),
            new("Input.IsPublic", "true"));
        LinkRow? link = await GetLink(linkId);
        LinkSearchRow? projection = await GetLinkSearch(linkId);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app/enlaces", response.Headers.Location?.OriginalString);
        Assert.Equal("https://example.com/original", link?.UrlOriginal);
        Assert.Equal("Nuevo título", link?.Title);
        Assert.Equal("Nueva descripción", link?.Description);
        Assert.Equal("https://example.com/new.png", link?.Image);
        Assert.Equal(1, link?.IsPublic);
        Assert.Equal(SearchTextNormalizer.Normalize("Nuevo título"), projection?.Title);
        Assert.Equal(SearchTextNormalizer.Normalize("Nueva descripción"), projection?.Description);
    }

    [Fact]
    public async Task PostEditLink_WhenPublicStateIsUncheckedMakesLinkPrivate()
    {
        long collectionId = await AddCollection(_userId, "Colección pública", true);
        long linkId = await AddLink(collectionId, "Anterior", "Descripción anterior", null, true);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", collectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Título actualizado"),
            new("Input.IsPublic", "false"));
        LinkRow? link = await GetLink(linkId);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app/enlaces", response.Headers.Location?.OriginalString);
        Assert.Equal("Título actualizado", link?.Title);
        Assert.Equal(0, link?.IsPublic);
    }

    [Fact]
    public async Task PostEditLink_WithUnparseablePublicStateDoesNotChangeLink()
    {
        long collectionId = await AddCollection(_userId, "Colección pública", true);
        long linkId = await AddLink(collectionId, "Anterior", "Descripción anterior", "https://example.com/old.png", true);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", collectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Cambio no aplicado"),
            new("Input.Description", "Cambio no aplicado"),
            new("Input.Image", "https://example.com/new.png"),
            new("Input.IsPublic", "not-a-boolean"));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-public\"", html, StringComparison.Ordinal);
        Assert.Matches("<a class=\"form-summary__link\" href=\"#link-public\">[^<]+</a>", html);
        Assert.Equal(original, await GetLink(linkId));
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Fact]
    public async Task PostEditLink_WhenCollectionChanges_MovesAndMakesLinkPrivateWithOtherEdits()
    {
        long sourceCollectionId = await AddCollection(_userId, "Colección origen", true);
        long destinationCollectionId = await AddCollection(_userId, "Colección destino", true);
        long linkId = await AddLink(sourceCollectionId, "Anterior", "Descripción anterior", "https://example.com/old.png", true);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", destinationCollectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Nuevo título"),
            new("Input.Description", "Nueva descripción"),
            new("Input.Image", "https://example.com/new.png"),
            new("Input.IsPublic", "true"));
        LinkRow? link = await GetLink(linkId);
        LinkSearchRow? projection = await GetLinkSearch(linkId);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app/enlaces", response.Headers.Location?.OriginalString);
        Assert.Equal(destinationCollectionId, link?.CollectionId);
        Assert.Equal(0, link?.IsPublic);
        Assert.Equal("https://example.com/original", link?.UrlOriginal);
        Assert.Equal("Nuevo título", link?.Title);
        Assert.Equal("Nueva descripción", link?.Description);
        Assert.Equal("https://example.com/new.png", link?.Image);
        Assert.Equal("Colección destino", projection?.CollectionName);
    }

    [Fact]
    public async Task PostEditLink_WithInvalidImageAndChangedCollectionDoesNotMoveLink()
    {
        long sourceCollectionId = await AddCollection(_userId, "Colección origen", true);
        long destinationCollectionId = await AddCollection(_userId, "Colección destino", true);
        long linkId = await AddLink(sourceCollectionId, "Título", "Descripción", "https://example.com/old.png", false);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", destinationCollectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Cambio no aplicado"),
            new("Input.Image", "file:///invalid.png"));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-image\"", html, StringComparison.Ordinal);
        Assert.Equal(original, await GetLink(linkId));
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Fact]
    public async Task PostEditLink_WithForeignCollectionDoesNotChangeLinkOrProjection()
    {
        long sourceCollectionId = await AddCollection(_userId, "Colección propia", true);
        long foreignUserId = await AddOtherUser();
        long foreignCollectionId = await AddCollection(foreignUserId, "Colección ajena", true);
        long linkId = await AddLink(sourceCollectionId, "Título", "Descripción", null, true);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", foreignCollectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Cambio no aplicado"),
            new("Input.IsPublic", "true"));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-collection\"", html, StringComparison.Ordinal);
        Assert.Contains("La colección seleccionada no es válida.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Colección ajena", html, StringComparison.Ordinal);
        Assert.Equal(original, await GetLink(linkId));
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("invalid")]
    public async Task PostEditLink_WithInvalidCollectionIdDoesNotChangeLink(string collectionId)
    {
        long sourceCollectionId = await AddCollection(_userId, "Colección propia", true);
        long linkId = await AddLink(sourceCollectionId, "Título", "Descripción", null, false);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", collectionId),
            new("Input.Title", "Cambio no aplicado"));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-collection\"", html, StringComparison.Ordinal);
        Assert.Equal(original, await GetLink(linkId));
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Fact]
    public async Task PostEditLink_WithMalformedCollectionIdKeepsExistingMessage()
    {
        long collectionId = await AddCollection(_userId, "Colección propia", true);
        long linkId = await AddLink(collectionId, "Título", "Descripción", null, false);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", "invalid"),
            new("Input.Title", "Cambio no aplicado"));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-collection\"", html, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Matches(html, Regex.Escape(CollectionSelectionInvalidMessage)).Count);
        Assert.DoesNotContain(UnsupportedUrlSchemeCode, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostEditLink_WithInvalidImageKeepsAllStoredFields()
    {
        long collectionId = await AddCollection(_userId, "Colección pública", true);
        long linkId = await AddLink(collectionId, "Anterior", "Descripción anterior", "https://example.com/old.png", false);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", collectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Cambio no aplicado"),
            new("Input.Description", "Cambio no aplicado"),
            new("Input.Image", "file:///invalid.png"),
            new("Input.IsPublic", "true"));
        string html = await response.Content.ReadAsStringAsync();
        string decodedHtml = WebUtility.HtmlDecode(html);
        LinkRow? current = await GetLink(linkId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-image\"", html, StringComparison.Ordinal);
        Assert.Contains("data-valmsg-for=\"Input.Image\"", html, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Matches(decodedHtml, Regex.Escape(UnsupportedUrlSchemeMessage)).Count);
        Assert.DoesNotContain(UnsupportedUrlSchemeCode, decodedHtml, StringComparison.Ordinal);
        Assert.Equal(original, current);
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Fact]
    public async Task PostEditLink_CannotMakeLinkPublicInPrivateCollection()
    {
        long collectionId = await AddCollection(_userId, "Colección privada", false);
        long linkId = await AddLink(collectionId, "Anterior", "Descripción anterior", null, false);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(linkId),
            EditRoute(linkId),
            new("Input.CollectionId", collectionId.ToString(CultureInfo.InvariantCulture)),
            new("Input.Title", "Cambio no aplicado"),
            new("Input.Description", "Descripción nueva"),
            new("Input.IsPublic", "true"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app/enlaces", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            header => header.StartsWith(TempDataCookieName + "=", StringComparison.Ordinal));
        Assert.Equal(original, await GetLink(linkId));
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Fact]
    public async Task PostEditLink_WithForeignLinkDoesNotDiscloseOrChangeIt()
    {
        long ownCollectionId = await AddCollection(_userId, "Colección propia", true);
        long ownLinkId = await AddLink(ownCollectionId, "Propio", null, null, false);
        long foreignUserId = await AddOtherUser();
        long foreignCollectionId = await AddCollection(foreignUserId, "Colección privada ajena", false);
        long foreignLinkId = await AddLink(foreignCollectionId, "Título privado", null, null, false, foreignUserId);
        LinkRow original = (await GetLink(foreignLinkId))!;

        using HttpResponseMessage response = await PostWithToken(
            EditRoute(ownLinkId),
            EditRoute(foreignLinkId),
            new KeyValuePair<string, string>("Input.Title", "Manipulado"));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No se ha podido acceder al elemento solicitado.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Título privado", html, StringComparison.Ordinal);
        Assert.Equal(original, await GetLink(foreignLinkId));
    }

    [Fact]
    public async Task PostMoveLink_ToOwnedCollectionMakesItPrivateAndPreservesItsData()
    {
        long sourceCollectionId = await AddCollection(_userId, "Colección origen", true);
        long destinationCollectionId = await AddCollection(_userId, "Colección destino", false);
        long linkId = await AddLink(sourceCollectionId, "Título", "Descripción", "https://example.com/image.png", true);

        using HttpResponseMessage response = await PostWithToken(
            MoveRoute(linkId),
            MoveRoute(linkId),
            new KeyValuePair<string, string>("Input.DestinationCollectionId", destinationCollectionId.ToString(CultureInfo.InvariantCulture)));
        LinkRow? link = await GetLink(linkId);
        LinkSearchRow? projection = await GetLinkSearch(linkId);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app/enlaces", response.Headers.Location?.OriginalString);
        Assert.Equal(destinationCollectionId, link?.CollectionId);
        Assert.Equal(0, link?.IsPublic);
        Assert.Equal("https://example.com/original", link?.UrlOriginal);
        Assert.Equal("Título", link?.Title);
        Assert.Equal("Descripción", link?.Description);
        Assert.Equal("https://example.com/image.png", link?.Image);
        Assert.Equal(SearchTextNormalizer.Normalize("Colección destino"), projection?.CollectionName);
    }

    [Fact]
    public async Task PostMoveLink_ToForeignCollectionDoesNotChangeLink()
    {
        long sourceCollectionId = await AddCollection(_userId, "Colección propia", true);
        long foreignUserId = await AddOtherUser();
        long foreignCollectionId = await AddCollection(foreignUserId, "Colección ajena", true);
        long linkId = await AddLink(sourceCollectionId, "Título", "Descripción", null, true);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            MoveRoute(linkId),
            MoveRoute(linkId),
            new KeyValuePair<string, string>("Input.DestinationCollectionId", foreignCollectionId.ToString(CultureInfo.InvariantCulture)));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No se ha podido acceder al elemento solicitado.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Colección ajena", html, StringComparison.Ordinal);
        Assert.Equal(original, await GetLink(linkId));
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("not-a-collection")]
    public async Task PostMoveLink_WithInvalidDestinationDoesNotChangeLink(string destinationId)
    {
        long collectionId = await AddCollection(_userId, "Colección", true);
        long linkId = await AddLink(collectionId, "Título", "Descripción", null, true);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            MoveRoute(linkId),
            MoveRoute(linkId),
            new KeyValuePair<string, string>("Input.DestinationCollectionId", destinationId));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("href=\"#link-collection\"", html, StringComparison.Ordinal);
        Assert.Contains("Título", html, StringComparison.Ordinal);
        Assert.Equal(original, await GetLink(linkId));
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Fact]
    public async Task PostMoveLink_ToNonexistentCollectionDoesNotChangeLink()
    {
        long collectionId = await AddCollection(_userId, "Colección", true);
        long linkId = await AddLink(collectionId, "Título", "Descripción", null, true);
        LinkRow original = (await GetLink(linkId))!;
        LinkSearchRow? originalProjection = await GetLinkSearch(linkId);

        using HttpResponseMessage response = await PostWithToken(
            MoveRoute(linkId),
            MoveRoute(linkId),
            new KeyValuePair<string, string>("Input.DestinationCollectionId", "999999"));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No se ha podido acceder al elemento solicitado.", html, StringComparison.Ordinal);
        Assert.Equal(original, await GetLink(linkId));
        Assert.Equal(originalProjection, await GetLinkSearch(linkId));
    }

    [Fact]
    public async Task GetDeleteLinkDoesNotDeleteAndConfirmedPostDeletesLinkAndSearchProjection()
    {
        long collectionId = await AddCollection(_userId, "Colección", false);
        long linkId = await AddLink(collectionId, "Eliminar", "Descripción", null, false);

        using HttpResponseMessage getResponse = await _client.GetAsync(DeleteRoute(linkId));
        string html = await getResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Contains("¿Seguro que quieres eliminar el enlace", html, StringComparison.Ordinal);
        Assert.Equal(1, await LinkCount(linkId));
        Assert.Equal(1, await LinkSearchCount(linkId));

        using HttpResponseMessage postResponse = await PostWithToken(
            DeleteRoute(linkId),
            DeleteRoute(linkId));

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal("/app/enlaces", postResponse.Headers.Location?.OriginalString);
        Assert.Equal(0, await LinkCount(linkId));
        Assert.Equal(0, await LinkSearchCount(linkId));
    }

    [Fact]
    public async Task PostDeleteLink_WithForeignLinkDoesNotDeleteIt()
    {
        long ownCollectionId = await AddCollection(_userId, "Colección propia", false);
        long ownLinkId = await AddLink(ownCollectionId, "Propio", null, null, false);
        long foreignUserId = await AddOtherUser();
        long foreignCollectionId = await AddCollection(foreignUserId, "Colección ajena", false);
        long foreignLinkId = await AddLink(foreignCollectionId, "Título privado", null, null, false, foreignUserId);

        using HttpResponseMessage response = await PostWithToken(
            DeleteRoute(ownLinkId),
            DeleteRoute(foreignLinkId));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/app/enlaces", response.Headers.Location?.OriginalString);
        Assert.Equal(1, await LinkCount(foreignLinkId));
        Assert.Equal(1, await LinkSearchCount(foreignLinkId));
    }

    [Fact]
    public async Task PostDeleteLink_WithInvalidIdDoesNotDeleteLink()
    {
        long collectionId = await AddCollection(_userId, "Colección", false);
        long linkId = await AddLink(collectionId, "Título", null, null, false);

        using HttpResponseMessage response = await PostWithToken(
            DeleteRoute(linkId),
            DeleteRoute(0));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("El identificador del enlace no es válido.", html, StringComparison.Ordinal);
        Assert.Equal(1, await LinkCount(linkId));
        Assert.Equal(1, await LinkSearchCount(linkId));
    }

    [Fact]
    public async Task PutLinkManagementPages_UsesGenericNotFoundForUnsupportedMethod()
    {
        long collectionId = await AddCollection(_userId, "Colección", false);
        long linkId = await AddLink(collectionId, "Título privado", null, null, false);

        foreach (string route in new[] { EditRoute(linkId), MoveRoute(linkId), DeleteRoute(linkId) })
        {
            using HttpRequestMessage request = new(HttpMethod.Put, route);
            using HttpResponseMessage response = await _client.SendAsync(request);
            string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("No se ha podido acceder al recurso solicitado dentro del área privada.", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Título privado", html, StringComparison.Ordinal);
        }

        Assert.Equal(0, (await GetLink(linkId))?.IsPublic);
    }

    [Fact]
    public async Task PostWithoutAntiforgeryTokenDoesNotChangeLink()
    {
        long collectionId = await AddCollection(_userId, "Colección", true);
        long linkId = await AddLink(collectionId, "Título", null, null, false);

        using HttpResponseMessage response = await _client.PostAsync(
            EditRoute(linkId),
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.CollectionId"] = collectionId.ToString(CultureInfo.InvariantCulture),
                ["Input.Title"] = "Cambio no autorizado",
                ["Input.IsPublic"] = "true",
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Título", (await GetLink(linkId))?.Title);
        Assert.Equal(0, (await GetLink(linkId))?.IsPublic);
    }

    private async Task<HttpResponseMessage> PostWithToken(
        string tokenRoute,
        string postRoute,
        params KeyValuePair<string, string>[] fields)
    {
        using HttpResponseMessage formResponse = await _client.GetAsync(tokenRoute);
        string html = await formResponse.Content.ReadAsStringAsync();
        string input = Regex.Match(
            html,
            "<input\\b(?=[^>]*name=\"__RequestVerificationToken\")[^>]*>").Value;
        string token = WebUtility.HtmlDecode(Regex.Match(input, "value=\"([^\"]+)\"").Groups[1].Value);
        List<KeyValuePair<string, string>> values = [.. fields, new("__RequestVerificationToken", token)];
        return await _client.PostAsync(postRoute, new FormUrlEncodedContent(values));
    }

    private async Task<long> AddCollection(long userId, string name, bool isPublic)
    {
        return await _keeper.ExecuteScalarAsync<long>(
            @"INSERT INTO Collection (UserId, Name, Slug, IsPublic)
              VALUES (@UserId, @Name, @Slug, @IsPublic);
              SELECT last_insert_rowid();",
            new { UserId = userId, Name = name, Slug = Guid.NewGuid().ToString("N"), IsPublic = isPublic });
    }

    private async Task<long> AddOtherUser()
    {
        return await _keeper.ExecuteScalarAsync<long>(
            @"INSERT INTO User (Email, Alias, SecurityStamp)
              VALUES (@Email, @Alias, @SecurityStamp);
              SELECT last_insert_rowid();",
            new
            {
                Email = $"other-{Guid.NewGuid():N}@linkubator.test",
                Alias = "other-user",
                SecurityStamp = Guid.NewGuid().ToString("N"),
            });
    }

    private async Task<long> AddLink(
        long collectionId,
        string? title,
        string? description,
        string? image,
        bool isPublic,
        long? userId = null,
        string url = "https://example.com/original")
    {
        long ownerId = userId ?? _userId;
        long linkId = await _keeper.ExecuteScalarAsync<long>(
            @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, CreatedAt)
              VALUES (@UserId, @CollectionId, @UrlOriginal, @UrlNormalized, @Title, @Description, @Image, @IsPublic, @CreatedAt);
              SELECT last_insert_rowid();",
            new
            {
                UserId = ownerId,
                CollectionId = collectionId,
                UrlOriginal = url,
                UrlNormalized = url,
                Title = title,
                Description = description,
                Image = image,
                IsPublic = isPublic,
                CreatedAt = DateTime.UtcNow,
            });
        string? collectionName = await _keeper.ExecuteScalarAsync<string?>(
            "SELECT Name FROM Collection WHERE Id = @CollectionId AND UserId = @UserId;",
            new { CollectionId = collectionId, UserId = ownerId });
        await _keeper.ExecuteAsync(
            @"INSERT INTO LinkSearch (rowid, UserId, UrlOriginal, Title, Description, CollectionName)
              VALUES (@Id, @UserId, @UrlOriginal, @Title, @Description, @CollectionName);",
            new
            {
                Id = linkId,
                UserId = ownerId,
                UrlOriginal = SearchTextNormalizer.Normalize(url),
                Title = SearchTextNormalizer.Normalize(title),
                Description = SearchTextNormalizer.Normalize(description),
                CollectionName = SearchTextNormalizer.Normalize(collectionName),
            });
        return linkId;
    }

    private Task<LinkRow?> GetLink(long linkId)
        => _keeper.QuerySingleOrDefaultAsync<LinkRow>(
            @"SELECT CollectionId, UrlOriginal, Title, Description, Image, IsPublic
              FROM Link WHERE Id = @LinkId;",
            new { LinkId = linkId });

    private Task<LinkSearchRow?> GetLinkSearch(long linkId)
        => _keeper.QuerySingleOrDefaultAsync<LinkSearchRow>(
            @"SELECT UrlOriginal, Title, Description, CollectionName
              FROM LinkSearch WHERE rowid = @LinkId;",
            new { LinkId = linkId });

    private Task<int> LinkCount(long linkId)
        => _keeper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link WHERE Id = @LinkId;", new { LinkId = linkId });

    private Task<int> LinkSearchCount(long linkId)
        => _keeper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM LinkSearch WHERE rowid = @LinkId;", new { LinkId = linkId });

    private static string EditRoute(long linkId) => $"/app/enlaces/{linkId.ToString(CultureInfo.InvariantCulture)}/editar";

    private static string MoveRoute(long linkId) => $"/app/enlaces/{linkId.ToString(CultureInfo.InvariantCulture)}/mover";

    private static string DeleteRoute(long linkId) => $"/app/enlaces/{linkId.ToString(CultureInfo.InvariantCulture)}/eliminar";

    private sealed record LinkRow(long CollectionId, string UrlOriginal, string? Title, string? Description, string? Image, long IsPublic);

    private sealed record LinkSearchRow(string UrlOriginal, string? Title, string? Description, string? CollectionName);

    private sealed class ConfiguredWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(TestHost.IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", connectionString);
            builder.UseSetting("DevelopmentIdentity:Email", DemoEmail);
            builder.UseSetting("DevelopmentIdentity:Name", "Demo User");
            builder.UseSetting("DevelopmentIdentity:Alias", "my-demo-user");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
        }
    }
}
