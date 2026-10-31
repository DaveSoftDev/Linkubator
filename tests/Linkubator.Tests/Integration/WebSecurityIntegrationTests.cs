using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Dapper;
using Linkubator.Web.Models.PrivateListing;
using Microsoft.Data.Sqlite;
using SqliteConnectionFactory = Linkubator.Infrastructure.Sqlite.SqliteConnectionFactory;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public sealed class WebSecurityIntegrationTests
{
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";
    private const string CreateCollectionRoute = "/app/colecciones/nueva";
    private const string TokenField = "__RequestVerificationToken";

    [Fact]
    public async Task HttpRequest_RedirectsToHttps()
    {
        string databasePath = CreateDatabasePath();
        try
        {
            await using SecurityIntegrationWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
            using HttpResponseMessage response = await client.GetAsync("http://localhost/");

            Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.Equal("https://localhost/", response.Headers.Location?.ToString());
            AssertSecurityHeaders(response);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task HostFiltering_RejectsUnapprovedHostAndAllowsConfiguredHost()
    {
        string databasePath = CreateDatabasePath();
        try
        {
            await using SecurityIntegrationWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
            using HttpRequestMessage allowedRequest = new(HttpMethod.Get, "https://localhost/");
            using HttpResponseMessage allowedResponse = await client.SendAsync(allowedRequest);
            using HttpRequestMessage rejectedRequest = new(HttpMethod.Get, "https://localhost/");
            rejectedRequest.Headers.Host = "attacker.invalid";
            using HttpResponseMessage rejectedResponse = await client.SendAsync(rejectedRequest);

            Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, rejectedResponse.StatusCode);
            AssertSecurityHeaders(rejectedResponse);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task ExplicitAntiforgeryValidationMiddleware_RejectsPostWithoutTokenAndAllowsIssuedToken()
    {
        string databasePath = CreateDatabasePath();
        try
        {
            await using SecurityIntegrationWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            factory.ResetPostExecutionCount();

            using HttpResponseMessage rejectedResponse = await client.PostAsync("/__tests/antiforgery", new FormUrlEncodedContent([]));

            Assert.Equal(0, factory.PostExecutionCount);
            Assert.Equal(HttpStatusCode.BadRequest, rejectedResponse.StatusCode);

            using HttpResponseMessage tokenResponse = await client.GetAsync("/__tests/antiforgery-token");
            AntiforgeryTokenResponse? issuedToken = await tokenResponse.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>();
            using HttpRequestMessage validRequest = new(HttpMethod.Post, "/__tests/antiforgery")
            {
                Content = new FormUrlEncodedContent([]),
            };
            validRequest.Headers.Add("RequestVerificationToken", issuedToken?.RequestToken);
            using HttpResponseMessage validResponse = await client.SendAsync(validRequest);

            Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(issuedToken?.RequestToken));
            Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);
            Assert.Equal(1, factory.PostExecutionCount);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task ExplicitAntiforgeryValidationMiddleware_WithAlteredTokenOrAnotherClientsToken_RejectsWithoutExecutingPost()
    {
        string databasePath = CreateDatabasePath();
        try
        {
            await using SecurityIntegrationWebApplicationFactory factory = new(databasePath);
            using HttpClient issuingClient = factory.CreateClient();
            using HttpClient otherClient = factory.CreateClient();
            AntiforgeryTokenResponse? issued = await issuingClient.GetFromJsonAsync<AntiforgeryTokenResponse>("/__tests/antiforgery-token");
            AntiforgeryTokenResponse? other = await otherClient.GetFromJsonAsync<AntiforgeryTokenResponse>("/__tests/antiforgery-token");
            Assert.False(string.IsNullOrWhiteSpace(issued?.RequestToken));
            Assert.False(string.IsNullOrWhiteSpace(other?.RequestToken));
            factory.ResetPostExecutionCount();
            using HttpRequestMessage alteredRequest = new(HttpMethod.Post, "/__tests/antiforgery")
            {
                Content = new FormUrlEncodedContent([]),
            };
            alteredRequest.Headers.Add("RequestVerificationToken", "invalid-" + issued.RequestToken);
            using HttpRequestMessage mismatchedRequest = new(HttpMethod.Post, "/__tests/antiforgery")
            {
                Content = new FormUrlEncodedContent([]),
            };
            mismatchedRequest.Headers.Add("RequestVerificationToken", issued.RequestToken);

            using HttpResponseMessage alteredResponse = await issuingClient.SendAsync(alteredRequest);
            using HttpResponseMessage mismatchedResponse = await otherClient.SendAsync(mismatchedRequest);

            Assert.Equal(HttpStatusCode.BadRequest, alteredResponse.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, mismatchedResponse.StatusCode);
            Assert.Equal(0, factory.PostExecutionCount);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task RazorCollectionForm_WithAlteredOrCrossedToken_RejectsWithoutPersistingAndAcceptsValidToken()
    {
        string databasePath = CreateDatabasePath();
        try
        {
            await using SecurityIntegrationWebApplicationFactory factory = new(databasePath);
            WebApplicationFactoryClientOptions options = new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false };
            using HttpClient issuingClient = factory.CreateClient(options);
            using HttpClient otherClient = factory.CreateClient(options);
            string issuedToken = await GetFormToken(issuingClient);
            string otherToken = await GetFormToken(otherClient);
            Assert.NotEqual(issuedToken, otherToken);
            string before = await CollectionSnapshot(databasePath);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            int initialCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection;");

            using HttpResponseMessage alteredResponse = await PostCollection(issuingClient, "invalid-" + issuedToken);
            Assert.Equal(HttpStatusCode.BadRequest, alteredResponse.StatusCode);
            Assert.Equal(before, await CollectionSnapshot(databasePath));

            using HttpResponseMessage crossedResponse = await PostCollection(otherClient, issuedToken);
            Assert.Equal(HttpStatusCode.BadRequest, crossedResponse.StatusCode);
            Assert.Equal(before, await CollectionSnapshot(databasePath));

            using HttpResponseMessage validResponse = await PostCollection(otherClient, otherToken);
            Assert.Equal(HttpStatusCode.Redirect, validResponse.StatusCode);
            Assert.Equal("/app/colecciones", validResponse.Headers.Location?.OriginalString);
            Assert.Equal(initialCount + 1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection;"));
            Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection c JOIN User u ON u.Id = c.UserId JOIN CollectionSearch s ON s.rowid = c.Id WHERE u.Email = @Email AND c.Name = @Name AND c.Description = @Description AND c.IsPublic = 0 AND s.Name = c.Name AND s.Description = c.Description;", new { Email = "user@linkubator.test", Name = "Antiforgery control", Description = "Valid form" }));
            Assert.NotEqual(before, await CollectionSnapshot(databasePath));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task TestEndpoint_PrivateListingQuery_InvokesProductionAnalyzer()
    {
        string databasePath = CreateDatabasePath();
        try
        {
            await using SecurityIntegrationWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            PrivateListingQueryAnalysisResult? firstResult = await client.GetFromJsonAsync<PrivateListingQueryAnalysisResult>(
                "/__tests/private-listing?p=2&p=3&c=7&c=8&q=hello&q=ignored&e=4");
            PrivateListingQueryAnalysisResult? invalidResult = await client.GetFromJsonAsync<PrivateListingQueryAnalysisResult>(
                "/__tests/private-listing?p=oops&e=not-a-filter");
            PrivateListingQueryAnalysisResult? emptyResult = await client.GetFromJsonAsync<PrivateListingQueryAnalysisResult>(
                "/__tests/private-listing?p=&q=%20%20&e=9");

            Assert.NotNull(firstResult);
            Assert.Equal(2, firstResult.PageNumber);
            Assert.Equal(7L, firstResult.CollectionId);
            Assert.Equal("hello", firstResult.SearchText);
            Assert.False(firstResult.HasAnyError);
            Assert.NotNull(invalidResult);
            Assert.Equal(1, invalidResult.PageNumber);
            Assert.False(invalidResult.HasAnyError);
            Assert.Null(invalidResult.SearchText);
            Assert.NotNull(emptyResult);
            Assert.Equal(1, emptyResult.PageNumber);
            Assert.Null(emptyResult.SearchText);
            Assert.False(emptyResult.HasAnyError);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static void AssertSecurityHeaders(HttpResponseMessage response)
    {
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("frame-ancestors 'none'", Assert.Single(response.Headers.GetValues("Content-Security-Policy")));
        Assert.Equal("strict-origin-when-cross-origin", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
    }

    private static async Task<string> GetFormToken(HttpClient client)
    {
        using HttpResponseMessage response = await client.GetAsync(CreateCollectionRoute);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = await response.Content.ReadAsStringAsync();
        string input = Regex.Match(html, "<input\\b(?=[^>]*name=\"__RequestVerificationToken\")[^>]*>").Value;
        string token = WebUtility.HtmlDecode(Regex.Match(input, "value=\"([^\"]+)\"").Groups[1].Value);
        Assert.False(string.IsNullOrWhiteSpace(token));
        return token;
    }

    private static Task<HttpResponseMessage> PostCollection(HttpClient client, string token)
        => client.PostAsync(CreateCollectionRoute, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Name"] = "Antiforgery control",
            ["Input.Description"] = "Valid form",
            ["Input.IsPublic"] = "false",
            [TokenField] = token,
        }));

    private static async Task<string> CollectionSnapshot(string databasePath)
    {
        await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
        string[] collections = (await connection.QueryAsync<string>(
            "SELECT json_object('Id', Id, 'UserId', UserId, 'Name', Name, 'Slug', Slug, 'Description', Description, 'IsPublic', IsPublic, 'CreatedAt', CreatedAt) FROM Collection ORDER BY Id;")).ToArray();
        string[] projections = (await connection.QueryAsync<string>(
            "SELECT json_object('Id', rowid, 'UserId', UserId, 'Name', Name, 'Description', Description) FROM CollectionSearch ORDER BY rowid;")).ToArray();
        return string.Join("\n", collections.Concat(projections));
    }

    private static string CreateDatabasePath()
        => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (string filePath in new[] { databasePath, $"{databasePath}-wal", $"{databasePath}-shm" })
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException)
                {
                    // SQLite can release temporary database files after the host shuts down.
                }
            }
        }
    }

    private sealed record AntiforgeryTokenResponse(string? RequestToken);

    private sealed class PostExecutionCounter
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public void Increment() => Interlocked.Increment(ref _count);

        public void Reset() => Interlocked.Exchange(ref _count, 0);
    }

    private sealed class SecurityIntegrationWebApplicationFactory(string databasePath) : WebApplicationFactory<Program>
    {
        private readonly PostExecutionCounter _postExecutionCounter = new();

        public int PostExecutionCount => _postExecutionCounter.Count;

        public void ResetPostExecutionCount() => _postExecutionCounter.Reset();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
            builder.UseSetting("https_port", "443");
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_postExecutionCounter);
                services.AddSingleton<IStartupFilter, TestEndpointsStartupFilter>();
            });
        }

        private sealed class TestEndpointsStartupFilter(PostExecutionCounter postExecutionCounter) : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
                => app =>
                {
                    app.Use(async (context, nextRequest) =>
                    {
                        if (context.Request.Path == "/__tests/antiforgery-token" && HttpMethods.IsGet(context.Request.Method))
                        {
                            IAntiforgery antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                            AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(context);
                            await context.Response.WriteAsJsonAsync(new AntiforgeryTokenResponse(tokens.RequestToken));
                            return;
                        }

                        if (context.Request.Path == "/__tests/antiforgery" && HttpMethods.IsPost(context.Request.Method))
                        {
                            IAntiforgery antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                            try
                            {
                                await antiforgery.ValidateRequestAsync(context);
                                postExecutionCounter.Increment();
                                context.Response.StatusCode = StatusCodes.Status200OK;
                            }
                            catch (AntiforgeryValidationException)
                            {
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            }

                            return;
                        }

                        await nextRequest();
                    });

                    next(app);
                    app.UseEndpoints(endpoints => endpoints.MapGet("/__tests/private-listing", async context =>
                    {
                        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(
                            context.Request.Query,
                            PrivateListingKind.Links,
                            static _ => true);
                        await context.Response.WriteAsJsonAsync(result);
                    }));
                };
        }
    }
}
