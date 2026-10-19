using Dapper;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;

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
            Assert.Equal(4L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM schema_migrations;"));
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
            Assert.Equal(4L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM schema_migrations;"));
            Assert.Equal(0L, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail }));
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

    private sealed class ConfiguredWebApplicationFactory(string databasePath, string passwordHash) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={databasePath}");
            builder.UseSetting("DevelopmentIdentity:Email", DemoEmail);
            builder.UseSetting("DevelopmentIdentity:Name", "Demo User");
            builder.UseSetting("DevelopmentIdentity:Alias", "my-demo-user");
            builder.UseSetting("DevelopmentIdentity:PasswordHash", passwordHash);
        }
    }
}