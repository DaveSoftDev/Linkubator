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
using ApplicationResults = Linkubator.Application.Results;
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
    public async Task AppDashboardTestEndpoint_UsesResolvedIdentityEvenWhenRequestContainsUserId()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            await using IdentityTestWebApplicationFactory factory = new(databasePath, expectedUserId: 42);
            using HttpClient client = factory.CreateClient();
            factory.ResetProviderCallCount();
            using HttpResponseMessage response = await client.GetAsync("/app/test-dashboard?userId=99");

            Assert.True(response.IsSuccessStatusCode);
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
                services.AddSingleton<IStartupFilter, TestDashboardEndpointStartupFilter>();
            });
        }
    }

    private sealed class TestDashboardEndpointStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => app =>
            {
                next(app);
                app.UseEndpoints(endpoints => endpoints.MapGet("/app/test-dashboard", async context =>
                {
                    ApplicationContracts.IDashboardService dashboardService = context.RequestServices
                        .GetRequiredService<ApplicationContracts.IDashboardService>();
                    ApplicationResults.Result<ApplicationDtos.DashboardDto> result =
                        await dashboardService.GetDashboard(context.RequestAborted);
                    await context.Response.WriteAsync(result.IsSuccess ? "Dashboard loaded" : "Dashboard failed");
                }));
            };
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