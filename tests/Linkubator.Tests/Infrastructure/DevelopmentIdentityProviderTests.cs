using Dapper;
using Linkubator.Infrastructure.DevelopmentIdentity;
using Linkubator.Infrastructure.Options;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class DevelopmentIdentityProviderTests
{
    private const string DemoEmail = "user@linkubator.test";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";

    [Fact]
    public async Task GetProfile_WhenOtherUserExistsFirst_SelectsAccountWithInitialEmail()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        await database.SeedOtherUser();
        await database.Provision();

        Linkubator.Application.Models.DevelopmentIdentityProfile profile = await database.CreateProvider().GetProfile();

        Assert.Equal(DemoEmail, profile.Email);
        Assert.Equal(await database.GetUserId(), profile.UserId);
    }

    [Fact]
    public async Task GetProfile_WhenAccountIsComplete_ReturnsItsPersistedProfile()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        await database.Provision();
        DevelopmentIdentityProvider provider = database.CreateProvider();
        UserProfile before = await database.GetUserProfile();

        Linkubator.Application.Models.DevelopmentIdentityProfile profile = await provider.GetProfile();
        UserProfile after = await database.GetUserProfile();

        Assert.Equal(await database.GetUserId(), profile.UserId);
        Assert.Equal((before.Name, before.Alias, before.Email), (profile.Name, profile.Alias, profile.Email));
        Assert.Equal(before, after);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetProfile_WhenAccountIsMissingOrIncomplete_Throws(bool seedIncompleteAccount)
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        if (seedIncompleteAccount)
        {
            await database.SeedIncompleteUser();
        }
        DevelopmentIdentityProvider provider = database.CreateProvider();

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetProfile());
    }

    private sealed class DevelopmentIdentityTestDatabase : IAsyncDisposable
    {
        private readonly string _databasePath;
        private readonly SqliteConnectionFactory _connectionFactory;

        private DevelopmentIdentityTestDatabase(string databasePath)
        {
            _databasePath = databasePath;
            _connectionFactory = new SqliteConnectionFactory($"Data Source={databasePath};Pooling=False");
        }

        public static async Task<DevelopmentIdentityTestDatabase> Create()
        {
            string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            DevelopmentIdentityTestDatabase database = new(databasePath);
            await new SqliteMigrationService(database._connectionFactory).ApplyPendingMigrations();
            return database;
        }

        public DevelopmentIdentityProvider CreateProvider()
            => new(_connectionFactory);

        public async Task Provision()
        {
            DevelopmentIdentityOptions options = new()
            {
                PasswordHash = ValidArgon2idHash,
            };
            await new DevelopmentIdentityProvisioner(_connectionFactory, options).Provision();
        }

        public async Task SeedIncompleteUser()
        {
            using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
            await connection.ExecuteAsync(
                "INSERT INTO User (Email, SecurityStamp) VALUES (@Email, @SecurityStamp);",
                new { Email = DemoEmail, SecurityStamp = Guid.NewGuid().ToString("N") });
        }

        public async Task SeedOtherUser()
        {
            using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
            await connection.ExecuteAsync(
                "INSERT INTO User (Email, Name, Alias, Password, EmailConfirmedAt, SecurityStamp) VALUES (@Email, @Name, @Alias, @Password, @Confirmed, @Stamp);",
                new { Email = "other@example.test", Name = "Other User", Alias = "other-user", Password = ValidArgon2idHash, Confirmed = DateTime.UtcNow, Stamp = Guid.NewGuid().ToString("N") });
        }

        public async Task<long> GetUserId()
        {
            using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
            return await connection.ExecuteScalarAsync<long>("SELECT Id FROM User WHERE Email = @Email;", new { Email = DemoEmail });
        }

        public async Task<UserProfile> GetUserProfile()
        {
            using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
            return await connection.QuerySingleAsync<UserProfile>(
                "SELECT Email, Name, Alias, Password, EmailConfirmedAt FROM User WHERE Email = @Email;",
                new { Email = DemoEmail });
        }

        public ValueTask DisposeAsync()
        {
            foreach (string filePath in new[] { _databasePath, $"{_databasePath}-wal", $"{_databasePath}-shm" })
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed record UserProfile(string Email, string Name, string Alias, string Password, string EmailConfirmedAt);
}