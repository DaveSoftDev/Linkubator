using Dapper;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteMigrationServiceTests
{
    [Fact]
    public async Task ApplyPendingMigrations_WhenScriptsArePending_ExecutesInVersionOrder()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteMigrationService service = new(factory, new[]
        {
            new SqliteMigrationScript("V10", "UPDATE sample_data SET value = 'after-v10' WHERE value = 'after-v2';"),
            new SqliteMigrationScript("V2", "INSERT INTO sample_data (value) VALUES ('after-v2');"),
            new SqliteMigrationScript("V1", "CREATE TABLE sample_data (id INTEGER PRIMARY KEY, value TEXT);")
        });

        await service.ApplyPendingMigrations();

        using SqliteConnection connection = factory.CreateOpenConnection();
        Assert.Equal(new[] { "V1", "V2", "V10" }, connection.Query<string>("SELECT version FROM schema_migrations ORDER BY CAST(SUBSTR(version, 2) AS INTEGER)").ToArray());
        Assert.Equal(1, connection.QuerySingle<int>("SELECT COUNT(*) FROM sample_data;"));
        Assert.Equal("after-v10", connection.QuerySingle<string>("SELECT value FROM sample_data WHERE id = 1;"));

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task ApplyPendingMigrations_WhenExecutedTwice_DoesNotApplyScriptsAgain()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteMigrationService service = new(factory, new[]
        {
            new SqliteMigrationScript("V001", "CREATE TABLE sample_data (id INTEGER PRIMARY KEY, value TEXT);"),
            new SqliteMigrationScript("V002", "INSERT INTO sample_data (value) VALUES ('after-v002');"),
            new SqliteMigrationScript("V003", "UPDATE sample_data SET value = 'after-v003' WHERE value = 'after-v002';")
        });

        await service.ApplyPendingMigrations();
        await service.ApplyPendingMigrations();

        using SqliteConnection connection = factory.CreateOpenConnection();
        Assert.Equal(3, connection.QuerySingle<int>("SELECT COUNT(*) FROM schema_migrations;"));
        Assert.Equal(1, connection.QuerySingle<int>("SELECT COUNT(*) FROM sample_data;"));
        Assert.Equal("after-v003", connection.QuerySingle<string>("SELECT value FROM sample_data WHERE id = 1;"));

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task ApplyPendingMigrations_WhenAScriptFails_DoesNotRecordFailedVersionOrContinue()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteMigrationService service = new(factory, new[]
        {
            new SqliteMigrationScript("V001", "CREATE TABLE sample_data (id INTEGER PRIMARY KEY, value TEXT); INSERT INTO sample_data (value) VALUES ('keep');"),
            new SqliteMigrationScript("V002", "INSERT INTO sample_data (value) VALUES ('rollback'); INSERT INTO missing_table (value) VALUES ('should fail');"),
            new SqliteMigrationScript("V003", "CREATE TABLE should_not_be_created (id INTEGER PRIMARY KEY);")
        });

        await Assert.ThrowsAsync<SqliteException>(async () => await service.ApplyPendingMigrations());

        using SqliteConnection connection = factory.CreateOpenConnection();
        Assert.Equal(new[] { "V001" }, connection.Query<string>("SELECT version FROM schema_migrations ORDER BY version").ToArray());
        Assert.Equal(new[] { "keep" }, connection.Query<string>("SELECT value FROM sample_data ORDER BY id;").ToArray());
        Assert.Equal(0, connection.QuerySingle<int>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'should_not_be_created';"));

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task ApplyPendingMigrations_WhenUserCollectionLinkSchemaIsPresent_ExposesExpectedStructure()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteMigrationService service = new(factory, SqliteMigrationCatalog.Load());

        await service.ApplyPendingMigrations();

        using SqliteConnection connection = factory.CreateOpenConnection();
        string[] tableNames = connection.Query<string>("SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'LinkSearch_%' AND name NOT LIKE 'CollectionSearch_%' AND name <> 'schema_migrations' ORDER BY name").ToArray();
        Assert.Equal(new[] { "Collection", "CollectionSearch", "Link", "LinkSearch", "User" }, tableNames);

        AssertSchemaColumns(connection);
        AssertSchemaIndexes(connection);
        AssertSchemaForeignKeys(connection);
        AssertSchemaDefinitionsAndDefaults(connection);

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task ApplyPendingMigrations_WhenLegacyVersionsAreRecorded_PreservesHistoryAndSampleData()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteMigrationService service = new(factory, SqliteMigrationCatalog.Load());

        await service.ApplyPendingMigrations();

        using (SqliteConnection connection = factory.CreateOpenConnection())
        {
            await connection.ExecuteAsync("CREATE TABLE sample_data (id INTEGER PRIMARY KEY, value TEXT NOT NULL);");
            await connection.ExecuteAsync("INSERT INTO sample_data (value) VALUES ('legacy-seed');");
            await connection.ExecuteAsync(@"
                INSERT INTO schema_migrations (version, applied_at_utc)
                VALUES ('V001', '2026-10-01T00:00:00Z'),
                       ('V002', '2026-10-01T00:00:00Z'),
                       ('V003', '2026-10-01T00:00:00Z');
            ");
        }

        await service.ApplyPendingMigrations();

        using (SqliteConnection connection = factory.CreateOpenConnection())
        {
            Assert.Equal(new[] { "V001", "V002", "V003", "V004", "V005", "V006", "V007" }, (await connection.QueryAsync<string>("SELECT version FROM schema_migrations ORDER BY version")).ToArray());
            Assert.Equal("legacy-seed", await connection.QuerySingleAsync<string>("SELECT value FROM sample_data WHERE id = 1;"));
        }

        DeleteDatabaseFiles(databasePath);
    }

    private static void AssertSchemaColumns(SqliteConnection connection)
    {
        AssertColumns(connection, "User", new[]
        {
            ("Id", "INTEGER", 0L, 1L), ("Email", "TEXT", 1L, 0L), ("Name", "TEXT", 0L, 0L),
            ("Alias", "TEXT", 0L, 0L), ("Password", "TEXT", 0L, 0L), ("EmailConfirmedAt", "TEXT", 0L, 0L),
            ("SecurityStamp", "TEXT", 1L, 0L), ("FailedLoginAttempts", "INTEGER", 1L, 0L), ("LockoutEnd", "TEXT", 0L, 0L),
            ("LastEmailSentAt", "TEXT", 0L, 0L), ("LastExistingAccountNoticeAt", "TEXT", 0L, 0L), ("CreatedAt", "TEXT", 1L, 0L)
        });
        AssertColumns(connection, "Collection", new[]
        {
            ("Id", "INTEGER", 0L, 1L), ("UserId", "INTEGER", 1L, 0L), ("Name", "TEXT", 1L, 0L),
            ("Slug", "TEXT", 1L, 0L), ("Description", "TEXT", 0L, 0L), ("IsPublic", "INTEGER", 1L, 0L),
            ("CreatedAt", "TEXT", 1L, 0L)
        });
        AssertColumns(connection, "Link", new[]
        {
            ("Id", "INTEGER", 0L, 1L), ("UserId", "INTEGER", 1L, 0L), ("CollectionId", "INTEGER", 1L, 0L),
            ("UrlOriginal", "TEXT", 1L, 0L), ("UrlNormalized", "TEXT", 1L, 0L), ("Title", "TEXT", 0L, 0L),
            ("Description", "TEXT", 0L, 0L), ("Image", "TEXT", 0L, 0L), ("IsPublic", "INTEGER", 1L, 0L),
            ("ScrapingStatus", "INTEGER", 1L, 0L), ("CreatedAt", "TEXT", 1L, 0L)
        });
    }

    private static void AssertSchemaIndexes(SqliteConnection connection)
    {
        AssertIndexes(connection, "User", new[] { new[] { ("Email", 0L) }, new[] { ("Alias", 0L) } }, Array.Empty<(string Name, (string Column, long Descending)[] Columns)>());
        AssertIndexes(connection, "Collection", new[]
        {
            new[] { ("UserId", 0L), ("Slug", 0L) }, new[] { ("UserId", 0L), ("Id", 0L) }
        }, new[] { ("idx_collection_is_public", new[] { ("IsPublic", 0L), ("UserId", 0L) }) });
        AssertIndexes(connection, "Link", new[] { new[] { ("UserId", 0L), ("UrlNormalized", 0L) } }, new[]
        {
            ("idx_link_user_id", new[] { ("UserId", 0L), ("CreatedAt", 1L) }),
            ("idx_link_collection_id", new[] { ("CollectionId", 0L), ("UserId", 0L) }),
            ("idx_link_is_public", new[] { ("IsPublic", 0L), ("UserId", 0L) }),
            ("idx_link_scraping_status", new[] { ("ScrapingStatus", 0L), ("UserId", 0L) })
        });
    }

    private static void AssertSchemaForeignKeys(SqliteConnection connection)
    {
        Assert.Equal(1, connection.QuerySingle<int>("PRAGMA foreign_keys;"));
        AssertForeignKeys(connection, "Collection", new[] { ("User", "UserId", "Id", "NO ACTION", "RESTRICT") });
        AssertForeignKeys(connection, "Link", new[]
        {
            ("User", "UserId", "Id", "NO ACTION", "RESTRICT"),
            ("Collection", "UserId", "UserId", "NO ACTION", "RESTRICT"),
            ("Collection", "CollectionId", "Id", "NO ACTION", "RESTRICT")
        });
        AssertCompositeCollectionForeignKey(connection);
    }

    private static void AssertSchemaDefinitionsAndDefaults(SqliteConnection connection)
    {
        string userDefinition = connection.QuerySingle<string>("SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'User';");
        string collectionDefinition = connection.QuerySingle<string>("SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'Collection';");
        string linkDefinition = connection.QuerySingle<string>("SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'Link';");

        Assert.Contains("Email TEXT NOT NULL UNIQUE", userDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Alias TEXT NULL UNIQUE", userDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SecurityStamp TEXT NOT NULL", userDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UNIQUE (UserId, Slug)", collectionDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UNIQUE (UserId, Id)", collectionDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FOREIGN KEY (UserId) REFERENCES User (Id)", collectionDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UNIQUE (UserId, UrlNormalized)", linkDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FOREIGN KEY (UserId, CollectionId) REFERENCES Collection (UserId, Id)", linkDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CHECK (IsPublic IN (0, 1))", collectionDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CHECK (IsPublic IN (0, 1))", linkDefinition, StringComparison.OrdinalIgnoreCase);

        connection.Execute("INSERT INTO User (Email, SecurityStamp) VALUES ('first@example.test', 'stamp-1'), ('second@example.test', 'stamp-2');");
        Assert.Equal(2, connection.QuerySingle<int>("SELECT COUNT(*) FROM User WHERE Alias IS NULL;"));
        AssertUtcIso8601CreatedAt(connection);
        AssertDefault(connection, "User", "FailedLoginAttempts", "0");
        AssertDefaultContains(connection, "User", "CreatedAt", "strftime");
        AssertDefault(connection, "Collection", "IsPublic", "0");
        AssertDefaultContains(connection, "Collection", "CreatedAt", "strftime");
        AssertDefault(connection, "Link", "IsPublic", "0");
        AssertDefault(connection, "Link", "ScrapingStatus", "0");
        AssertDefaultContains(connection, "Link", "CreatedAt", "strftime");

        Assert.Equal(0, connection.QuerySingle<int>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'trigger';"));
        string tableDefinitions = string.Join(";", connection.Query<string>("SELECT sql FROM sqlite_master WHERE type = 'table' AND name IN ('User', 'Collection', 'Link') ORDER BY name"));
        Assert.DoesNotContain("CASCADE", tableDefinitions, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, connection.QuerySingle<int>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'idx_collection_user_id';"));
    }

    private static void AssertDefault(SqliteConnection connection, string tableName, string columnName, string expectedValue)
    {
        dynamic column = connection.Query($"PRAGMA table_info('{tableName}');").Single(column => (string)column.name == columnName);
        Assert.Equal(expectedValue, Convert.ToString(column.dflt_value));
    }

    private static void AssertDefaultContains(SqliteConnection connection, string tableName, string columnName, string expectedValue)
    {
        dynamic column = connection.Query($"PRAGMA table_info('{tableName}');").Single(column => (string)column.name == columnName);
        Assert.Contains(expectedValue, Convert.ToString(column.dflt_value), StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertColumns(SqliteConnection connection, string tableName, (string Name, string Type, long NotNull, long PrimaryKey)[] expected)
    {
        (string, string, long, long)[] actual = connection.Query($"PRAGMA table_info('{tableName}');")
            .Select(column => ((string)column.name, (string)column.type, (long)column.notnull, (long)column.pk))
            .ToArray();

        Assert.Equal(expected, actual);
    }

    private static void AssertIndexes(
        SqliteConnection connection,
        string tableName,
        (string Column, long Descending)[][] expectedUniqueIndexes,
        (string Name, (string Column, long Descending)[] Columns)[] expectedSecondaryIndexes)
    {
        (string Name, long Unique, string Origin)[] indexes = connection.Query($"PRAGMA index_list('{tableName}');")
            .Select(index => (Name: (string)index.name, Unique: (long)index.unique, Origin: (string)index.origin))
            .ToArray();
        string[] uniqueIndexes = indexes.Where(index => index.Unique == 1)
            .Select(index => ReadIndexColumns(connection, index.Name))
            .Select(FormatIndexColumns)
            .OrderBy(columns => columns)
            .ToArray();
        string[] expectedUnique = expectedUniqueIndexes
            .Select(FormatIndexColumns)
            .OrderBy(columns => columns)
            .ToArray();

        Assert.Equal(expectedUnique, uniqueIndexes);

        (string Name, long Unique, string Origin)[] secondaryIndexes = indexes.Where(index => index.Origin == "c")
            .OrderBy(index => index.Name)
            .ToArray();
        Assert.Equal(expectedSecondaryIndexes.Select(index => index.Name).OrderBy(name => name), secondaryIndexes.Select(index => index.Name).OrderBy(name => name));
        foreach ((string Name, (string Column, long Descending)[] Columns) expectedIndex in expectedSecondaryIndexes)
        {
            Assert.Equal(FormatIndexColumns(expectedIndex.Columns), FormatIndexColumns(ReadIndexColumns(connection, expectedIndex.Name)));
        }
    }

    private static string FormatIndexColumns((string Column, long Descending)[] columns)
    {
        return string.Join(",", columns.Select(column => $"{column.Column}:{column.Descending}"));
    }

    private static (string Column, long Descending)[] ReadIndexColumns(SqliteConnection connection, string indexName)
    {
        return connection.Query($"PRAGMA index_xinfo('{indexName}');")
            .Where(column => (long)column.key == 1)
            .OrderBy(column => (long)column.seqno)
            .Select(column => ((string)column.name, (long)column.desc))
            .ToArray();
    }

    private static void AssertForeignKeys(SqliteConnection connection, string tableName, (string Table, string From, string To, string OnUpdate, string OnDelete)[] expected)
    {
        (string Table, string From, string To, string OnUpdate, string OnDelete)[] actual = connection.Query($"PRAGMA foreign_key_list('{tableName}');")
            .Select(foreignKey => (Table: (string)foreignKey.table, From: (string)foreignKey.from, To: (string)foreignKey.to, OnUpdate: (string)foreignKey.on_update, OnDelete: (string)foreignKey.on_delete))
            .OrderBy(foreignKey => foreignKey.Table)
            .ThenBy(foreignKey => foreignKey.From)
            .ToArray();
        (string Table, string From, string To, string OnUpdate, string OnDelete)[] expectedSorted = expected.OrderBy(foreignKey => foreignKey.Table).ThenBy(foreignKey => foreignKey.From).ToArray();

        Assert.Equal(expectedSorted, actual);
    }

    private static void AssertCompositeCollectionForeignKey(SqliteConnection connection)
    {
        (long Id, long Sequence, string From, string To)[] foreignKeys = connection.Query("PRAGMA foreign_key_list('Link');")
            .Where(foreignKey => (string)foreignKey.table == "Collection")
            .Select(foreignKey => (Id: (long)foreignKey.id, Sequence: (long)foreignKey.seq, From: (string)foreignKey.from, To: (string)foreignKey.to))
            .OrderBy(foreignKey => foreignKey.Sequence)
            .ToArray();

        Assert.Equal(new[] { ("UserId", "UserId"), ("CollectionId", "Id") }, foreignKeys.Select(foreignKey => (foreignKey.From, foreignKey.To)).ToArray());
        Assert.Single(foreignKeys.Select(foreignKey => foreignKey.Id).Distinct());
        Assert.Equal(new[] { 0L, 1L }, foreignKeys.Select(foreignKey => foreignKey.Sequence).ToArray());
    }

    private static void AssertUtcIso8601CreatedAt(SqliteConnection connection)
    {
        string createdAt = connection.QuerySingle<string>("SELECT CreatedAt FROM User WHERE Email = 'first@example.test';");

        Assert.EndsWith("Z", createdAt, StringComparison.Ordinal);
        Assert.True(DateTimeOffset.TryParse(createdAt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out DateTimeOffset parsed));
        Assert.Equal(TimeSpan.Zero, parsed.Offset);
    }

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (string filePath in new[]
        {
            databasePath,
            $"{databasePath}-wal",
            $"{databasePath}-shm"
        })
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException)
                {
                    // SQLite can keep a WAL-backed temp file locked briefly on Windows after the connection closes.
                }
            }
        }
    }
}
