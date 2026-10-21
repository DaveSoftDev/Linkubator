using Dapper;
using Linkubator.Infrastructure.DevelopmentIdentity;
using Linkubator.Infrastructure.Options;
using Linkubator.Infrastructure.Security.PasswordHashing;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class DevelopmentIdentityProvisionerTests
{
    private const string DemoEmail = "user@linkubator.test";
    private const string DemoName = "Demo User";
    private const string DemoAlias = "my-demo-user";
    private const string ValidArgon2idHash = "$argon2id$v=19$m=65536,t=3,p=1$42xS9y5nNCo8mABa9Y0KQw$w2hDMTrLZgBZ0Iqk7Q1m5J0C2U0xHgpiA62b0mXkVQ4";
    private const string AnotherValidArgon2idHash = "$argon2id$v=19$m=19456,t=2,p=1$7VuS6kaxXydjX+UlI4f1TQ$YWJjZGVmZ2hpamtsbW5vcHFyc3Q=";
    private const string TestVectorPassword = "Argon2id verifier test vector only";
    private const string TestVectorPhcHash = "$argon2id$v=19$m=19456,t=2,p=1$AAECAwQFBgcICQoLDA0ODw$yA5x3tCrHbeWOAFDR8GQq4W9o7J+yo/NcCn/b/vtcyI";

    [Fact]
    public void Argon2idPhcVerifier_WhenPasswordMatchesHash_ReturnsTrue()
    {
        Assert.True(Argon2idPhcVerifier.Verify(TestVectorPassword, TestVectorPhcHash));
    }

    [Fact]
    public void Argon2idPhcVerifier_WhenPasswordDoesNotMatchHash_ReturnsFalse()
    {
        Assert.False(Argon2idPhcVerifier.Verify("wrong password", TestVectorPhcHash));
    }

    [Fact]
    public void Argon2idPhcVerifier_WhenDigestIsChanged_ReturnsFalse()
    {
        string tamperedHash = TestVectorPhcHash[..^1] + (TestVectorPhcHash[^1] == 'A' ? 'B' : 'A');

        Assert.False(Argon2idPhcVerifier.Verify(TestVectorPassword, tamperedHash));
    }

    [Theory]
    [InlineData("not-a-hash")]
    [InlineData("$argon2id$v=19$m=19456,t=2,p=1$ZGVtbw$ZGVtbw")]
    [InlineData("$argon2id$v=19$m=1024,t=1,p=1$AAECAwQFBgcICQoLDA0ODw$yA5x3tCrHbeWOAFDR8GQq4W9o7J+yo/NcCn/b/vtcyI")]
    [InlineData("$argon2id$v=19$m=999999999,t=2,p=1$AAECAwQFBgcICQoLDA0ODw$yA5x3tCrHbeWOAFDR8GQq4W9o7J+yo/NcCn/b/vtcyI")]
    public void Argon2idPhcVerifier_WhenPhcIsMalformedOrBelowMinimum_ReturnsFalse(string phcHash)
    {
        Assert.False(Argon2idPhcVerifier.Verify(TestVectorPassword, phcHash));
    }

    [Fact]
    public async Task Provision_WhenUserDoesNotExist_CreatesCompletedProfileAndInboxCollection()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        DevelopmentIdentityProvisioner provisioner = database.CreateProvisioner();

        await provisioner.Provision();

        await using SqliteConnection connection = database.OpenConnection();
        (string Email, string Name, string Alias, string Password, DateTime EmailConfirmedAt) user = await connection.QuerySingleAsync<(string Email, string Name, string Alias, string Password, DateTime EmailConfirmedAt)>(
            "SELECT Email, Name, Alias, Password, EmailConfirmedAt FROM User WHERE Email = @Email;",
            new { Email = DemoEmail });
        (string Name, string Slug, long IsPublic) collection = await connection.QuerySingleAsync<(string Name, string Slug, long IsPublic)>(
            "SELECT Name, Slug, IsPublic FROM Collection WHERE UserId = (SELECT Id FROM User WHERE Email = @Email);",
            new { Email = DemoEmail });

        Assert.Equal((DemoEmail, DemoName, DemoAlias, ValidArgon2idHash), (user.Email, user.Name, user.Alias, user.Password));
        Assert.NotEqual(default, user.EmailConfirmedAt);
        Assert.Equal(("Bandeja de entrada", "bandeja-de-entrada", 0L), collection);
    }

    [Fact]
    public async Task Provision_WhenUserIsIncomplete_CompletesOnlyMissingProfileFields()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        await database.SeedUser(DemoEmail, null, null, null, null);

        await database.CreateProvisioner().Provision();

        await using SqliteConnection connection = database.OpenConnection();
        (string? Name, string? Alias, string? Password, DateTime? EmailConfirmedAt) user = await connection.QuerySingleAsync<(string? Name, string? Alias, string? Password, DateTime? EmailConfirmedAt)>(
            "SELECT Name, Alias, Password, EmailConfirmedAt FROM User WHERE Email = @Email;",
            new { Email = DemoEmail });

        Assert.Equal((DemoName, DemoAlias, ValidArgon2idHash), (user.Name, user.Alias, user.Password));
        Assert.NotNull(user.EmailConfirmedAt);
    }

    [Fact]
    public async Task Provision_WhenUserIsComplete_PreservesExistingProfileAndPassword()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        string existingPasswordHash = AnotherValidArgon2idHash;
        DateTime existingConfirmation = new(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        await database.SeedUser(DemoEmail, "Existing Name", "existing-alias", existingPasswordHash, existingConfirmation);

        await database.CreateProvisioner().Provision();

        await using SqliteConnection connection = database.OpenConnection();
        (string Name, string Alias, string Password, DateTime EmailConfirmedAt) user = await connection.QuerySingleAsync<(string Name, string Alias, string Password, DateTime EmailConfirmedAt)>(
            "SELECT Name, Alias, Password, EmailConfirmedAt FROM User WHERE Email = @Email;",
            new { Email = DemoEmail });

        Assert.Equal(("Existing Name", "existing-alias", existingPasswordHash, existingConfirmation), user);
    }

    [Fact]
    public async Task Provision_WhenUserIsComplete_DoesNotExecuteUpdate()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        await database.SeedUser(DemoEmail, "Existing Name", "existing-alias", AnotherValidArgon2idHash, DateTime.UtcNow);
        await database.CreateUpdateCounterTrigger();

        await database.CreateProvisioner().Provision();

        Assert.Equal(0L, await database.ReadUpdateCount());
    }

    [Fact]
    public async Task Provision_WhenUserAlreadyHasCollection_DoesNotChangeExistingCollections()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        await database.SeedUser(DemoEmail, DemoName, DemoAlias, ValidArgon2idHash, DateTime.UtcNow);
        await database.SeedCollection(DemoEmail, "Existing public", "existing-public", true);
        await database.SeedCollection(DemoEmail, "Existing private", "existing-private", false);

        (string Name, string Slug, long IsPublic)[] before = await database.ReadCollections(DemoEmail);
        await database.CreateProvisioner().Provision();
        (string Name, string Slug, long IsPublic)[] after = await database.ReadCollections(DemoEmail);

        Assert.Equal(before, after);
    }

    [Fact]
    public async Task Provision_WhenUserAlreadyHasCollection_DoesNotExecuteCollectionInsert()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        await database.SeedUser(DemoEmail, DemoName, DemoAlias, ValidArgon2idHash, DateTime.UtcNow);
        await database.SeedCollection(DemoEmail, "Existing public", "existing-public", true);
        await database.CreateCollectionInsertCounterTrigger();

        await database.CreateProvisioner().Provision();

        Assert.Equal(0L, await database.ReadCollectionInsertCount());
    }

    [Fact]
    public async Task Provision_WhenRunRepeatedly_DoesNotDuplicateUserOrCollection()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        DevelopmentIdentityProvisioner provisioner = database.CreateProvisioner();

        await provisioner.Provision();
        await provisioner.Provision();

        await using SqliteConnection connection = database.OpenConnection();
        long users = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail });
        long collections = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Collection WHERE UserId = (SELECT Id FROM User WHERE Email = @Email);", new { Email = DemoEmail });

        Assert.Equal((1L, 1L), (users, collections));
    }

    [Fact]
    public async Task Provision_WhenConcurrentCallsTargetSameDatabase_CreatesSingleUserAndCollection()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        DevelopmentIdentityProvisioner first = database.CreateProvisioner();
        DevelopmentIdentityProvisioner second = database.CreateProvisioner();

        await Task.WhenAll(first.Provision(), second.Provision());

        await using SqliteConnection connection = database.OpenConnection();
        long users = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail });
        long collections = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Collection WHERE UserId = (SELECT Id FROM User WHERE Email = @Email);", new { Email = DemoEmail });

        Assert.Equal((1L, 1L), (users, collections));
    }

    [Fact]
    public async Task Provision_WhenCollectionInsertFails_RollsBackNewUser()
    {
        await using DevelopmentIdentityTestDatabase database = await DevelopmentIdentityTestDatabase.Create();
        await using (SqliteConnection connection = database.OpenConnection())
        {
            await connection.ExecuteAsync(@"
                CREATE TRIGGER fail_demo_collection_insert
                BEFORE INSERT ON Collection
                BEGIN
                    SELECT RAISE(ABORT, 'injected collection insert failure');
                END;
            ");
        }

        await Assert.ThrowsAsync<SqliteException>(() => database.CreateProvisioner().Provision());

        await using SqliteConnection verificationConnection = database.OpenConnection();
        long users = await verificationConnection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM User WHERE Email = @Email;", new { Email = DemoEmail });
        Assert.Equal(0L, users);
    }

    [Fact]
    public void Constructor_WhenPasswordHashIsNotArgon2idPhc_Throws()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            new DevelopmentIdentityProvisioner(new SqliteConnectionFactory("Data Source=:memory:"), new DevelopmentIdentityOptions
            {
                Email = DemoEmail,
                Name = DemoName,
                Alias = DemoAlias,
                PasswordHash = "not-a-hash",
            }));

        Assert.Contains("Argon2id", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WhenPasswordHashIsNotCryptographicallyVerifiableAsGenericHash_StillAcceptsStructuralPhc()
    {
        DevelopmentIdentityProvisioner provisioner = new(new SqliteConnectionFactory("Data Source=:memory:"), new DevelopmentIdentityOptions
        {
            Email = DemoEmail,
            Name = DemoName,
            Alias = DemoAlias,
            PasswordHash = ValidArgon2idHash,
        });

        Assert.NotNull(provisioner);
    }

    [Fact]
    public void ProvisioningSqlResource_IsEmbeddedOutsideVersionedMigrationCatalog()
    {
        string[] resources = typeof(DevelopmentIdentityProvisioner).Assembly.GetManifestResourceNames();
        string[] migrationVersions = SqliteMigrationCatalog.Load().Select(script => script.Version).ToArray();

        Assert.Contains("Linkubator.Infrastructure.DevelopmentIdentity.Sql.ProvisionDevelopmentIdentity.sql", resources);
        Assert.Equal(new[] { "V004", "V005" }, migrationVersions);
    }

    private sealed class DevelopmentIdentityTestDatabase : IAsyncDisposable
    {
        private readonly string _databasePath;
        private readonly SqliteConnectionFactory _connectionFactory;

        private DevelopmentIdentityTestDatabase(string databasePath)
        {
            _databasePath = databasePath;
            _connectionFactory = new SqliteConnectionFactory($"Data Source={databasePath}");
        }

        public static async Task<DevelopmentIdentityTestDatabase> Create()
        {
            string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            DevelopmentIdentityTestDatabase database = new(databasePath);
            await new SqliteMigrationService(database._connectionFactory).ApplyPendingMigrations();
            return database;
        }

        public SqliteConnection OpenConnection()
            => _connectionFactory.CreateOpenConnection();

        public DevelopmentIdentityProvisioner CreateProvisioner()
            => new(_connectionFactory, new DevelopmentIdentityOptions
            {
                Email = DemoEmail,
                Name = DemoName,
                Alias = DemoAlias,
                PasswordHash = ValidArgon2idHash,
            });

        public async Task SeedUser(string email, string? name, string? alias, string? passwordHash, DateTime? emailConfirmedAt)
        {
            await using SqliteConnection connection = OpenConnection();
            await connection.ExecuteAsync(@"
                INSERT INTO User (Email, Name, Alias, Password, EmailConfirmedAt, SecurityStamp)
                VALUES (@Email, @Name, @Alias, @PasswordHash, @EmailConfirmedAt, @SecurityStamp);
            ", new
            {
                Email = email,
                Name = name,
                Alias = alias,
                PasswordHash = passwordHash,
                EmailConfirmedAt = emailConfirmedAt,
                SecurityStamp = Guid.NewGuid().ToString("N"),
            });
        }

        public async Task SeedCollection(string email, string name, string slug, bool isPublic)
        {
            await using SqliteConnection connection = OpenConnection();
            await connection.ExecuteAsync(@"
                INSERT INTO Collection (UserId, Name, Slug, IsPublic)
                VALUES ((SELECT Id FROM User WHERE Email = @Email), @Name, @Slug, @IsPublic);
            ", new { Email = email, Name = name, Slug = slug, IsPublic = isPublic ? 1 : 0 });
        }

        public async Task CreateUpdateCounterTrigger()
        {
            await using SqliteConnection connection = OpenConnection();
            await connection.ExecuteAsync(@"
                CREATE TABLE test_user_update_counter (count INTEGER NOT NULL);
                INSERT INTO test_user_update_counter VALUES (0);
                CREATE TRIGGER count_user_updates
                AFTER UPDATE ON User
                BEGIN
                    UPDATE test_user_update_counter SET count = count + 1;
                END;
            ");
        }

        public async Task<long> ReadUpdateCount()
        {
            await using SqliteConnection connection = OpenConnection();
            return await connection.ExecuteScalarAsync<long>("SELECT count FROM test_user_update_counter;");
        }

        public async Task CreateCollectionInsertCounterTrigger()
        {
            await using SqliteConnection connection = OpenConnection();
            await connection.ExecuteAsync(@"
                CREATE TABLE test_collection_insert_counter (count INTEGER NOT NULL);
                INSERT INTO test_collection_insert_counter VALUES (0);
                CREATE TRIGGER count_collection_inserts
                AFTER INSERT ON Collection
                BEGIN
                    UPDATE test_collection_insert_counter SET count = count + 1;
                END;
            ");
        }

        public async Task<long> ReadCollectionInsertCount()
        {
            await using SqliteConnection connection = OpenConnection();
            return await connection.ExecuteScalarAsync<long>("SELECT count FROM test_collection_insert_counter;");
        }

        public async Task<(string Name, string Slug, long IsPublic)[]> ReadCollections(string email)
        {
            await using SqliteConnection connection = OpenConnection();
            IEnumerable<(string Name, string Slug, long IsPublic)> collections = await connection.QueryAsync<(string Name, string Slug, long IsPublic)>(@"
                SELECT Name, Slug, IsPublic
                FROM Collection
                WHERE UserId = (SELECT Id FROM User WHERE Email = @Email)
                ORDER BY Id;
            ", new { Email = email });
            return collections.ToArray();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (string filePath in new[] { _databasePath, $"{_databasePath}-wal", $"{_databasePath}-shm" })
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (IOException)
                    {
                        // SQLite may release temporary database files after the connection closes.
                    }
                }
            }

            await ValueTask.CompletedTask;
        }
    }
}
