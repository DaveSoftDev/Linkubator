using System.Collections.Concurrent;
using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationUseCases = Linkubator.Application.UseCases.Dashboard;

namespace Linkubator.Tests.Integration;

[Trait("Category", "Web")]
public class WebStartupTests
{
    [Fact]
    public async Task HomePageReturnsSuccess()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/");

            Assert.True(response.IsSuccessStatusCode);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PrivateResourceUnavailable_RendersGenericAccessibleNoticeWithoutInternalDetails()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using PrivateResourceUnavailableWebApplicationFactory factory = new(databasePath);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/app/test-resource-unavailable");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("No se ha podido acceder al elemento solicitado.", html, StringComparison.Ordinal);
            Assert.Contains("role=\"status\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/app/enlaces\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_RESOURCE_CODE", html, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE_RESOURCE_MESSAGE", html, StringComparison.Ordinal);
            Assert.DoesNotContain("999999", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppDashboard_RendersRecentLinksFromDashboardUseCase()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            await using (SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection())
            {
                long userId = await connection.ExecuteScalarAsync<long>(
                    "SELECT Id FROM User WHERE Email = @Email;",
                    new { Email = DemoEmail });
                long collectionId = await connection.ExecuteScalarAsync<long>(
                    "INSERT INTO Collection (UserId, Name, Slug, IsPublic) VALUES (@UserId, @Name, @Slug, 0); SELECT last_insert_rowid();",
                    new { UserId = userId, Name = "Dashboard Collection", Slug = "dashboard-collection" });
                await connection.ExecuteAsync(
                    "INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, IsPublic, CreatedAt) VALUES (@UserId, @CollectionId, @Url, @Url, @Title, @Description, 0, @CreatedAt);",
                    new
                    {
                        UserId = userId,
                        CollectionId = collectionId,
                        Url = "https://dashboard-link.example",
                        Title = "Example Dashboard Link",
                        Description = "Seeded from dashboard integration test",
                        CreatedAt = DateTime.UtcNow,
                    });
            }

            using HttpResponseMessage response = await client.GetAsync("/app");
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Example Dashboard Link", html, StringComparison.Ordinal);
            Assert.Contains("dashboard-link.example", html, StringComparison.Ordinal);
            Assert.Contains("Seeded from dashboard integration test", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppDashboard_RendersSearchAndDatabaseBackedEmptyState()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/app");
            string html = await response.Content.ReadAsStringAsync();
            string decodedHtml = System.Net.WebUtility.HtmlDecode(html);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("action=\"/app/enlaces\"", html, StringComparison.Ordinal);
            Assert.Contains("name=\"q\"", html, StringComparison.Ordinal);
            Assert.Contains("Todavía no tienes enlaces guardados.", decodedHtml, StringComparison.Ordinal);
            Assert.DoesNotContain("React Documentation", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Crear etiqueta", html, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task PresentationPage_RendersAccessiblePresentationComponents()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/app/presentacion");
            string html = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("role=\"alert\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-live=\"assertive\"", html, StringComparison.Ordinal);
            Assert.Contains("href=\"#email\"", html, StringComparison.Ordinal);
            Assert.Contains("role=\"status\"", html, StringComparison.Ordinal);
            Assert.Contains("aria-current=\"page\"", html, StringComparison.Ordinal);
            Assert.Contains("method=\"get\"", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("action=\"/app/presentacion\"", html, StringComparison.Ordinal);
            Assert.Contains("for=\"search-links\"", html, StringComparison.Ordinal);
            Assert.Contains("id=\"search-links\"", html, StringComparison.Ordinal);
            Assert.Contains("name=\"q\"", html, StringComparison.Ordinal);
            Assert.Contains("name=\"status\" value=\"all\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("name=\"page\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppDashboard_WhenRequestedWithUnsupportedMethod_ReturnsNotFound()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.PutAsync("/app", new StringContent(string.Empty));
            string html = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("No se ha podido acceder al recurso solicitado dentro del área privada.", html, StringComparison.Ordinal);
            Assert.Contains("href=\"/app\"", html, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task HomePage_IncludesSecurityHeaders()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/");

            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
            Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task AppDashboard_UsesResolvedIdentityEvenWhenRequestContainsUserId()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using IdentityTestWebApplicationFactory factory = new(databasePath, expectedUserId: 42);
            using HttpClient client = factory.CreateClient();
            factory.ResetProviderCallCount();
            using HttpResponseMessage response = await client.GetAsync("/app?userId=99");
            string html = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("Dashboard", html, StringComparison.Ordinal);
            Assert.Equal(42L, factory.CapturedUserIds.Single());
            Assert.Equal(1, factory.ProviderCallCount);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task HomePage_DoesNotResolvePrivateIdentity()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using IdentityTestWebApplicationFactory factory = new(databasePath, expectedUserId: 42);
            using HttpClient client = factory.CreateClient();
            factory.ResetProviderCallCount();
            using HttpResponseMessage response = await client.GetAsync("/");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(0, factory.ProviderCallCount);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_WhenDatabaseIsNew_AppliesMigrationsBeforeProvisioningAndServing()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash);
            using HttpClient client = factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("/");

            Assert.True(response.IsSuccessStatusCode);
            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            Assert.Equal(1L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail }));
            Assert.Equal(1L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Collection WHERE UserId = (SELECT Id FROM User WHERE Email = @Email);", new { Email = DemoEmail }));
            Assert.Equal(2L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM schema_migrations;"));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_WhenProvisioningConfigurationIsInvalid_DoesNotServeRequests()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using ConfiguredWebApplicationFactory factory = new(databasePath, "invalid-hash");

            Assert.ThrowsAny<Exception>(() => factory.CreateClient());

            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            Assert.Equal(2L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM schema_migrations;"));
            Assert.Equal(0L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail }));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_WhenDevelopmentIdentityCannotBeResolved_DoesNotServeRequests()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using ConfiguredWebApplicationFactory factory = new(databasePath, ValidArgon2idHash, failIdentityResolution: true);

            Assert.ThrowsAny<Exception>(() => factory.CreateClient());

            await using SqliteConnection connection = new SqliteConnectionFactory($"Data Source={databasePath}").CreateOpenConnection();
            Assert.Equal(2L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM schema_migrations;"));
            Assert.Equal(1L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail }));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private const string DemoEmail = "user@linkubator.test";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";

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

    private sealed class IdentityTestWebApplicationFactory(string databasePath, long expectedUserId) : WebApplicationFactory<Program>
    {
        private readonly CapturingDashboardRepository _dashboardRepository = new();
        private readonly CountingDevelopmentIdentityProvider _identityProvider = new(expectedUserId);

        public IReadOnlyCollection<long> CapturedUserIds => _dashboardRepository.UserIds;
        public int ProviderCallCount => _identityProvider.CallCount;

        public void ResetProviderCallCount() => _identityProvider.ResetCallCount();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:Email", DemoEmail);
            builder.UseSetting("DevelopmentIdentity:Name", "Demo User");
            builder.UseSetting("DevelopmentIdentity:Alias", "my-demo-user");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
            builder.ConfigureTestServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IDevelopmentIdentityProvider>(_identityProvider));
                services.RemoveAll<ApplicationContracts.IDashboardRepository>();
                services.AddScoped<ApplicationContracts.IDashboardRepository>(_ => _dashboardRepository);
                services.AddScoped<ApplicationContracts.IDashboardService, ApplicationUseCases.DashboardService>();
            });
        }
    }

    private sealed class CountingDevelopmentIdentityProvider(long userId) : IDevelopmentIdentityProvider
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public void ResetCallCount() => Interlocked.Exchange(ref _callCount, 0);

        public Task<long> GetUserId(CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromResult(userId);
        }
    }

    private sealed class CapturingDashboardRepository : ApplicationContracts.IDashboardRepository
    {
        private readonly ConcurrentQueue<long> _userIds = new();

        public IReadOnlyCollection<long> UserIds => _userIds.ToArray();

        public Task<ApplicationDtos.DashboardDto> GetByUser(long userId, CancellationToken cancellationToken = default)
        {
            _userIds.Enqueue(userId);
            return Task.FromResult(new ApplicationDtos.DashboardDto(0, 0, [], []));
        }
    }

    private sealed class PrivateResourceUnavailableWebApplicationFactory(string databasePath) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:Email", DemoEmail);
            builder.UseSetting("DevelopmentIdentity:Name", "Demo User");
            builder.UseSetting("DevelopmentIdentity:Alias", "my-demo-user");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", ValidArgon2idHash);
            builder.ConfigureTestServices(services =>
                services.AddSingleton<IStartupFilter, PrivateResourceUnavailableTestEndpointFilter>());
        }
    }

    private sealed class PrivateResourceUnavailableTestEndpointFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => app =>
            {
                next(app);
                app.UseEndpoints(endpoints => endpoints.MapGet("/app/test-resource-unavailable", async context =>
                {
                    Linkubator.Application.Results.ApplicationError error = Linkubator.Application.Results.ApplicationError.ResourceNotAccessible(
                        "PRIVATE_RESOURCE_CODE",
                        "999999",
                        "PRIVATE_RESOURCE_MESSAGE");
                    Linkubator.Web.Models.Presentation.PrivateResourceUnavailableViewModel? viewModel =
                        Linkubator.Web.Models.Presentation.PrivateResourceUnavailablePresenter.Create(error, "/app/enlaces");
                    Linkubator.Web.Models.Presentation.PrivateResourceUnavailableViewModel? validationErrorViewModel =
                        Linkubator.Web.Models.Presentation.PrivateResourceUnavailablePresenter.Create(
                            Linkubator.Application.Results.ApplicationError.Validation("PRIVATE_VALIDATION_CODE", "PRIVATE_FIELD", "PRIVATE_VALIDATION_MESSAGE"),
                            "/app/enlaces");
                    if (validationErrorViewModel is not null)
                    {
                        throw new InvalidOperationException("Validation errors must not use the resource-unavailable presenter.");
                    }
                    Microsoft.AspNetCore.Mvc.Razor.IRazorViewEngine viewEngine = context.RequestServices
                        .GetRequiredService<Microsoft.AspNetCore.Mvc.Razor.IRazorViewEngine>();
                    Microsoft.AspNetCore.Mvc.ViewEngines.ViewEngineResult viewResult = viewEngine.GetView(
                        executingFilePath: null,
                        viewPath: "~/Pages/Shared/_PrivateResourceUnavailable.cshtml",
                        isMainPage: false);
                    if (!viewResult.Success)
                    {
                        throw new InvalidOperationException("The private resource notice partial was not found.");
                    }

                    Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState = new();
                    Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary viewData = new(
                        context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider>(),
                        modelState)
                    {
                        Model = viewModel,
                    };
                    Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider tempDataProvider = context.RequestServices
                        .GetRequiredService<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>();
                    Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary tempData = new(context, tempDataProvider);
                    Microsoft.AspNetCore.Mvc.ActionContext actionContext = new(
                        context,
                        new Microsoft.AspNetCore.Routing.RouteData(),
                        new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
                        modelState);
                    using StringWriter writer = new();
                    Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext = new(
                        actionContext,
                        viewResult.View,
                        viewData,
                        tempData,
                        writer,
                        new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions());
                    await viewResult.View.RenderAsync(viewContext);
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(writer.ToString());
                }));
            };
    }

    private sealed class ConfiguredWebApplicationFactory(string databasePath, string passwordHash, bool failIdentityResolution = false) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Linkubator", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:Email", DemoEmail);
            builder.UseSetting("DevelopmentIdentity:Name", "Demo User");
            builder.UseSetting("DevelopmentIdentity:Alias", "my-demo-user");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", passwordHash);

            if (failIdentityResolution)
            {
                builder.ConfigureTestServices(services =>
                    services.AddSingleton<IDevelopmentIdentityProvider, UnresolvableDevelopmentIdentityProvider>());
            }
        }
    }

    private sealed class UnresolvableDevelopmentIdentityProvider : IDevelopmentIdentityProvider
    {
        public Task<long> GetUserId(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("The configured development identity cannot be resolved.");
    }
}