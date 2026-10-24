using System.Net;
using System.Net.Http.Json;
using Linkubator.Web.Models.PrivateListing;
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
    private const string DemoEmail = "user@linkubator.test";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";

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
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Antiforgery_RejectsPostWithoutTokenAndAllowsIssuedToken()
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
    public async Task PrivateListingQueryEndpoint_UsesProductionAnalyzer()
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
            Assert.True(invalidResult.HasPageError);
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
            builder.UseSetting("DevelopmentIdentity:Email", DemoEmail);
            builder.UseSetting("DevelopmentIdentity:Name", "Demo User");
            builder.UseSetting("DevelopmentIdentity:Alias", "my-demo-user");
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
