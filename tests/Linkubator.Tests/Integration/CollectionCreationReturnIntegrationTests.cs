using System.Net;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using SqliteConnectionFactory = Linkubator.Infrastructure.Sqlite.SqliteConnectionFactory;
using TestHost = Microsoft.AspNetCore.Hosting;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public sealed class CollectionCreationReturnIntegrationTests : IAsyncLifetime
{
    private const string CreateRoute = "/app/colecciones/nueva";
    private const string DefaultRoute = "/app/colecciones";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";
    private readonly SqliteConnectionFactory _connections = new($"Data Source=collections-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
    private SqliteConnection _keeper = null!;
    private ConfiguredWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _keeper = _connections.CreateOpenConnection();
        _factory = new ConfiguredWebApplicationFactory(_keeper.ConnectionString);
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _keeper.DisposeAsync();
    }

    [Theory]
    [InlineData("/app", "/app")]
    [InlineData("/app/enlaces?q=uno&c=1&p=2", "/app/enlaces?q=uno&c=1&p=2")]
    [InlineData("/app/colecciones?q=uno&p=2", "/app/colecciones?q=uno&p=2")]
    [InlineData("", DefaultRoute)]
    [InlineData("https://example.com/app", DefaultRoute)]
    [InlineData("//example.com/app", DefaultRoute)]
    [InlineData("/app/../../outside", DefaultRoute)]
    [InlineData("/app\\outside", DefaultRoute)]
    [InlineData("/app?x=\r\nLocation: https://example.com", DefaultRoute)]
    [InlineData(CreateRoute, DefaultRoute)]
    public async Task GetNewCollection_WithReturnUrl_RendersValidatedCancelAndPostDestination(string origin, string expected)
    {
        using HttpResponseMessage response = await _client.GetAsync(CreateRoute + "?ru=" + Uri.EscapeDataString(origin));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{expected}\"", html, StringComparison.Ordinal);
        Assert.Contains($"class=\"secondary-button\" href=\"{expected}\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/app", "/app")]
    [InlineData("/app/enlaces?q=uno&c=1&p=2", "/app/enlaces?q=uno&c=1&p=2")]
    [InlineData("/app/colecciones?q=uno&p=2", "/app/colecciones?q=uno&p=2")]
    [InlineData("", DefaultRoute)]
    [InlineData("https://example.com/app", DefaultRoute)]
    [InlineData("//example.com/app", DefaultRoute)]
    [InlineData("/app/../../outside", DefaultRoute)]
    [InlineData("/app\\outside", DefaultRoute)]
    [InlineData("/app?x=\r\nLocation: https://example.com", DefaultRoute)]
    [InlineData(CreateRoute, DefaultRoute)]
    public async Task PostNewCollection_WithReturnUrl_CreatesCollectionAndUsesValidatedDestination(string origin, string expected)
    {
        string token = await GetToken();
        Dictionary<string, string> fields = NewFields(token, origin);

        using HttpResponseMessage response = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expected, response.Headers.Location?.OriginalString);
        Assert.Equal(1, await _keeper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE Name = 'Nueva colección';"));
    }

    [Fact]
    public async Task CreateCollection_WithoutReturnUrl_UsesCollectionListing()
    {
        string html = await _client.GetStringAsync(CreateRoute);
        Dictionary<string, string> fields = NewFields(GetInputValue(html, "__RequestVerificationToken"), string.Empty);
        fields.Remove("ru");

        using HttpResponseMessage response = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));

        Assert.Contains($"class=\"secondary-button\" href=\"{DefaultRoute}\"", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(DefaultRoute, response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("", "false")]
    [InlineData("Nueva colección", "invalid")]
    [InlineData("Bandeja de entrada", "false")]
    public async Task PostNewCollection_WithErrors_KeepsOriginAndValuesForCancelAndRetry(string name, string privacy)
    {
        const string origin = "/app/enlaces?q=guardado&c=1&p=2";
        string token = await GetToken();
        Dictionary<string, string> fields = NewFields(token, origin);
        fields["Input.Name"] = name;
        fields["Input.Description"] = "Descripción pendiente";
        fields["Input.IsPublic"] = privacy;

        using HttpResponseMessage response = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{origin}\"", html, StringComparison.Ordinal);
        Assert.Contains($"class=\"secondary-button\" href=\"{origin}\"", html, StringComparison.Ordinal);
        Assert.Equal(name, GetInputValue(html, "Input.Name"));
        Assert.Contains("Descripción pendiente", html, StringComparison.Ordinal);
        Assert.Equal(0, await _keeper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE Name = 'Nueva colección';"));

        fields["Input.Name"] = "Nueva colección";
        fields["Input.IsPublic"] = "false";
        fields["ru"] = GetInputValue(html, "ru");
        fields["__RequestVerificationToken"] = GetInputValue(html, "__RequestVerificationToken");
        using HttpResponseMessage retry = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));

        Assert.Equal(HttpStatusCode.Redirect, retry.StatusCode);
        Assert.Equal(origin, retry.Headers.Location?.OriginalString);
        Assert.Equal(1, await _keeper.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE Name = 'Nueva colección';"));
    }

    [Theory]
    [InlineData("/app", "/app")]
    [InlineData("/app/enlaces?q=uno&p=1", "/app/enlaces?q=uno&p=1")]
    [InlineData("/app/colecciones?q=uno&p=1", "/app/colecciones?q=uno&p=1")]
    [InlineData(CreateRoute, DefaultRoute)]
    public async Task CreateCollection_FromSidebar_ReturnsToValidatedOrigin(string source, string expected)
    {
        using HttpResponseMessage page = await _client.GetAsync(source);
        string html = await page.Content.ReadAsStringAsync();
        string anchor = Regex.Match(html, "<a\\b(?=[^>]*class=\"[^\"]*\\bsidebar-create-collection\\b)[^>]*>").Value;
        string destination = WebUtility.HtmlDecode(Regex.Match(anchor, "href=\"([^\"]+)\"").Groups[1].Value);

        using HttpResponseMessage form = await _client.GetAsync(destination);
        string formHtml = WebUtility.HtmlDecode(await form.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, form.StatusCode);
        Assert.Contains($"name=\"ru\" value=\"{expected}\"", formHtml, StringComparison.Ordinal);
        Assert.Contains($"class=\"secondary-button\" href=\"{expected}\"", formHtml, StringComparison.Ordinal);

        Dictionary<string, string> fields = NewFields(GetInputValue(formHtml, "__RequestVerificationToken"), GetInputValue(formHtml, "ru"));
        using HttpResponseMessage created = await _client.PostAsync(CreateRoute, new FormUrlEncodedContent(fields));

        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        Assert.Equal(expected, created.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task GetCollections_FromCreateButton_TransmitsSearchAndPage()
    {
        const string origin = "/app/colecciones?q=uno&p=1";
        using HttpResponseMessage page = await _client.GetAsync(origin);
        string html = await page.Content.ReadAsStringAsync();
        string anchor = Regex.Match(html, "<a\\b(?=[^>]*class=\"view-switch__button toolbar-tool\")[^>]*>").Value;
        string destination = WebUtility.HtmlDecode(Regex.Match(anchor, "href=\"([^\"]+)\"").Groups[1].Value);

        using HttpResponseMessage form = await _client.GetAsync(destination);
        string formHtml = WebUtility.HtmlDecode(await form.Content.ReadAsStringAsync());

        Assert.Contains($"name=\"ru\" value=\"{origin}\"", formHtml, StringComparison.Ordinal);
    }

    private async Task<string> GetToken()
    {
        string html = await _client.GetStringAsync(CreateRoute);
        return GetInputValue(html, "__RequestVerificationToken");
    }

    private static string GetInputValue(string html, string name)
    {
        string input = Regex.Match(html, $"<input\\b(?=[^>]*name=\"{Regex.Escape(name)}\")[^>]*>").Value;
        return WebUtility.HtmlDecode(Regex.Match(input, "value=\"([^\"]*)\"").Groups[1].Value);
    }

    private static Dictionary<string, string> NewFields(string token, string origin) => new()
    {
        ["__RequestVerificationToken"] = token,
        ["ru"] = origin,
        ["Input.Name"] = "Nueva colección",
        ["Input.IsPublic"] = "false",
    };

    private sealed class ConfiguredWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(TestHost.IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", connectionString);
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
        }
    }
}
