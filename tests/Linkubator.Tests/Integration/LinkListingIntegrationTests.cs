using System.Net;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
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
    public async Task GetLinks_WithPageAfterExactBoundary_ShowsErrorWithoutCardsOrSwitch(int count, int page)
    {
        await AddLinks(_userId, _collectionId, count, "Lectura");

        using HttpResponseMessage response = await _client.GetAsync($"/app/enlaces?c={_collectionId}&p={page}");
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("Los valores no son correctos.", html, StringComparison.Ordinal);
        Assert.Empty(CardTitles(html));
        Assert.DoesNotContain("data-link-view-switch", html, StringComparison.Ordinal);
        Assert.DoesNotContain("toolbar-create-link", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDashboardSearch_SubmitsQueryToOwnedLinkListing()
    {
        await AddLinks(_userId, _collectionId, 1, "DashboardSearchMatch");
        await AddLinks(_userId, _otherCollectionId, 1, "UnrelatedDashboardLink");
        await AddLinks(_foreignUserId, _foreignCollectionId, 1, "DashboardSearchMatchPrivate");

        using HttpResponseMessage dashboardResponse = await _client.GetAsync("/app");
        string dashboardHtml = await dashboardResponse.Content.ReadAsStringAsync();
        string searchForm = Regex.Match(dashboardHtml, "<form\\b[^>]*class=\\\"search-form\\\"[^>]*>.*?</form>", RegexOptions.Singleline).Value;
        string action = Regex.Match(searchForm, "action=\\\"([^\\\"]+)\\\"").Groups[1].Value;

        using HttpResponseMessage listingResponse = await _client.GetAsync($"{action}?q=DashboardSearchMatch");
        string listingHtml = await listingResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
        Assert.DoesNotContain("method=\"post\"", searchForm, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("role=\"search\"", searchForm, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Buscar enlaces\"", searchForm, StringComparison.Ordinal);
        Assert.Contains("name=\"q\"", searchForm, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"p\"", searchForm, StringComparison.Ordinal);
        Assert.Equal("/app/enlaces", action);
        Assert.Equal(HttpStatusCode.OK, listingResponse.StatusCode);
        Assert.Equal(["DashboardSearchMatch 01"], CardTitles(listingHtml));
        Assert.DoesNotContain("UnrelatedDashboardLink", listingHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("DashboardSearchMatchPrivate", listingHtml, StringComparison.Ordinal);
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

    [Fact]
    public async Task GetLinks_WithSearch_RanksResultsBeforeDateAndIdTieBreakers()
    {
        DateTime createdAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await AddLink(_userId, _collectionId, "needle", createdAt);
        await AddLink(_userId, _collectionId, "needle needle", createdAt);

        using HttpResponseMessage response = await _client.GetAsync("/app/enlaces?q=needle");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["needle needle", "needle"], CardTitles(html));
    }

    [Fact]
    public async Task GetListings_WithSameQuery_ReturnsOnlyItsEntityAndOwnedData()
    {
        const string collectionName = "CrossEntityNeedle";
        const string ownLinkTitle = "CrossEntityNeedleLink";
        const string foreignLinkTitle = "CrossEntityNeedleForeign";
        long matchingCollectionId = await AddCollection(_userId, collectionName);
        await _keeper.ExecuteAsync(
            "INSERT INTO CollectionSearch (rowid, UserId, Name, Description) VALUES (@Id, @UserId, @Name, '');",
            new { Id = matchingCollectionId, UserId = _userId, Name = SearchTextNormalizer.Normalize(collectionName) });
        await AddLink(_userId, _collectionId, ownLinkTitle, DateTime.UtcNow);
        await AddLink(_foreignUserId, _foreignCollectionId, foreignLinkTitle, DateTime.UtcNow);

        using HttpResponseMessage linksResponse = await _client.GetAsync("/app/enlaces?q=CrossEntityNeedle&userId=" + _foreignUserId);
        string linksHtml = await linksResponse.Content.ReadAsStringAsync();
        using HttpResponseMessage collectionsResponse = await _client.GetAsync("/app/colecciones?q=CrossEntityNeedle");
        string collectionsHtml = WebUtility.HtmlDecode(await collectionsResponse.Content.ReadAsStringAsync());
        string collectionsTable = Regex.Match(collectionsHtml, "<table\\b.*?</table>", RegexOptions.Singleline).Value;

        Assert.Equal(HttpStatusCode.OK, linksResponse.StatusCode);
        Assert.Equal([ownLinkTitle], CardTitles(linksHtml));
        Assert.DoesNotContain(foreignLinkTitle, linksHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, collectionsResponse.StatusCode);
        Assert.Contains(collectionName, collectionsTable, StringComparison.Ordinal);
        Assert.DoesNotContain(ownLinkTitle, collectionsTable, StringComparison.Ordinal);
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

        Assert.Equal(16, CardTitles(firstHtml).Length);
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
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{origin}\"", formHtml, StringComparison.Ordinal);
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
    [InlineData("p=0")]
    [InlineData("p=-1")]
    [InlineData("p=999999")]
    [InlineData("p=2147483648")]
    [InlineData("c=not-an-id")]
    [InlineData("c=9223372036854775808")]
    public async Task GetLinks_WithInvalidParameters_ShowsAccessibleErrorInsteadOfUnfilteredCards(string query)
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");

        using HttpResponseMessage response = await _client.GetAsync("/app/enlaces?" + query);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Los valores no son correctos.", html, StringComparison.Ordinal);
        Assert.Contains("role=\"alert\"", html, StringComparison.Ordinal);
        Assert.Empty(CardTitles(html));
        Assert.DoesNotContain("data-link-view-switch", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetLinks_WithOversizedSearch_ShowsAccessibleErrorWithoutResults()
    {
        using HttpResponseMessage response = await _client.GetAsync("/app/enlaces?q=" + new string('a', 101));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("El texto de búsqueda supera la longitud máxima permitida.", html, StringComparison.Ordinal);
        Assert.Contains("role=\"alert\"", html, StringComparison.Ordinal);
        Assert.Empty(CardTitles(html));
    }

    [Fact]
    public async Task GetLinks_WithRepeatedAndUnsupportedParameters_UsesOnlyFirstApplicableValues()
    {
        await AddLinks(_userId, _collectionId, 1, "Lectura");
        await AddLinks(_userId, _otherCollectionId, 1, "Otra");

        using HttpResponseMessage response = await _client.GetAsync(
            $"/app/enlaces?c={_collectionId}&c={_otherCollectionId}&p=1&p=invalid&q=Lectura&q=Otra&e=invalid&e=999&unknown=invalid");
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
        Assert.Contains($"class=\"collection-form__cancel\" href=\"{origin}\"", formHtml, StringComparison.Ordinal);
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

    private Task<long> AddCollection(long userId, string name)
        => _keeper.ExecuteScalarAsync<long>(
            "INSERT INTO Collection (UserId, Name, Slug, IsPublic) VALUES (@UserId, @Name, @Slug, 0); SELECT last_insert_rowid();",
            new { UserId = userId, Name = name, Slug = Guid.NewGuid().ToString("N") });

    private async Task<long> AddLink(long userId, long collectionId, string title, DateTime createdAt)
    {
        string url = $"https://example.com/{collectionId}/{Guid.NewGuid():N}";
        long id = await _keeper.ExecuteScalarAsync<long>(
            @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, IsPublic, CreatedAt)
              VALUES (@UserId, @CollectionId, @Url, @Url, @Title, 0, @CreatedAt); SELECT last_insert_rowid();",
            new { UserId = userId, CollectionId = collectionId, Url = url, Title = title, CreatedAt = createdAt });
        await _keeper.ExecuteAsync(
            @"INSERT INTO LinkSearch (rowid, UserId, UrlOriginal, Title, CollectionName)
              VALUES (@Id, @UserId, @Url, @Title, @CollectionName);",
            new { Id = id, UserId = userId, Url = url, Title = SearchTextNormalizer.Normalize(title), CollectionName = string.Empty });
        return id;
    }

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
