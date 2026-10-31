using Dapper;
using Linkubator.Infrastructure.Repositories;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteSearchProjectionMigrationTests
{
    private const string LinkProjectionName = "LinkSearch";
    private const string CollectionProjectionName = "CollectionSearch";

    [Fact]
    public async Task ApplyPendingMigrations_WhenSearchProjectionsAreCreated_ExposesExpectedColumnsAndTokenizer()
    {
            await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        using SqliteConnection connection = database.OpenConnection();

        Assert.Equal(new[] { "Collection", "CollectionSearch", "Link", "LinkSearch", "User" }, GetApplicationTableNames(connection));
        Assert.Equal(new[] { "UserId", "UrlOriginal", "Title", "Description", "CollectionName" }, GetProjectionColumns(connection, LinkProjectionName));
        Assert.Equal(new[] { "UserId", "Name", "Description" }, GetProjectionColumns(connection, CollectionProjectionName));
        Assert.Contains("unicode61 remove_diacritics 2", GetProjectionDefinition(connection, LinkProjectionName), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unicode61 remove_diacritics 2", GetProjectionDefinition(connection, CollectionProjectionName), StringComparison.OrdinalIgnoreCase);

        connection.Execute($"INSERT INTO {LinkProjectionName} (rowid, UserId, UrlOriginal, Title, Description, CollectionName) VALUES (1, 42, 'https://example.test/canción', 'Árbol 2026', 'Texto', 'Colección');");
        connection.Execute($"INSERT INTO {CollectionProjectionName} (rowid, UserId, Name) VALUES (1, 42, 'Colección Árbol');");

        Assert.Equal(1, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {LinkProjectionName} WHERE {LinkProjectionName} MATCH 'arbol';"));
        Assert.Equal(1, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {LinkProjectionName} WHERE {LinkProjectionName} MATCH '2026';"));
        Assert.Equal(1, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {CollectionProjectionName} WHERE {CollectionProjectionName} MATCH 'coleccion';"));
        Assert.Equal(0, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {LinkProjectionName} WHERE {LinkProjectionName} MATCH 'colec';"));
    }

    [Fact]
    public async Task ApplyPendingMigrations_WhenProjectionRowsHaveSameRowId_RemainsSeparatedByEntity()
    {
            await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        using SqliteConnection connection = database.OpenConnection();

        connection.Execute($"INSERT INTO {LinkProjectionName} (rowid, UserId, UrlOriginal, Title, Description, CollectionName) VALUES (7, 42, '', 'linkonlytoken', '', '');");
        connection.Execute($"INSERT INTO {CollectionProjectionName} (rowid, UserId, Name) VALUES (7, 42, 'collectiononlytoken');");

        Assert.Equal(1, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {LinkProjectionName} WHERE {LinkProjectionName} MATCH 'linkonlytoken' AND UserId = 42;"));
        Assert.Equal(0, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {LinkProjectionName} WHERE {LinkProjectionName} MATCH 'collectiononlytoken' AND UserId = 42;"));
        Assert.Equal(1, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {CollectionProjectionName} WHERE {CollectionProjectionName} MATCH 'collectiononlytoken' AND UserId = 42;"));
        Assert.Equal(0, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {CollectionProjectionName} WHERE {CollectionProjectionName} MATCH 'linkonlytoken' AND UserId = 42;"));
    }

    [Fact]
    public async Task ApplyPendingMigrations_WhenProjectionRowsHaveDifferentUsers_PreservesUserIdColumn()
    {
            await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        using SqliteConnection connection = database.OpenConnection();

        connection.Execute($"INSERT INTO {LinkProjectionName} (rowid, UserId, UrlOriginal, Title, Description, CollectionName) VALUES (8, 42, '', 'privatesearchtoken', '', '');");

        connection.Execute($"INSERT INTO {LinkProjectionName} (rowid, UserId, UrlOriginal, Title) VALUES (9, 99, '', 'privatesearchtoken');");
        Assert.Equal(new long[] { 42, 99 }, connection.Query<long>($"SELECT UserId FROM {LinkProjectionName} WHERE {LinkProjectionName} MATCH 'privatesearchtoken' ORDER BY rowid;"));

        Assert.Equal(1, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {LinkProjectionName} WHERE {LinkProjectionName} MATCH 'privatesearchtoken' AND UserId = 42;"));
        Assert.Equal(1, connection.QuerySingle<int>($"SELECT COUNT(*) FROM {LinkProjectionName} WHERE {LinkProjectionName} MATCH 'privatesearchtoken' AND UserId = 99;"));
    }

    [Fact]
    public async Task ApplyPendingMigrations_WhenAppliedToLegacySchema_PreservesExistingDataAndAddsMigration()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteMigrationService legacyMigrationService = new(factory, SqliteMigrationCatalog.Load().Where(script => script.Version == "V004"));
        SqliteMigrationService migrationService = new(factory, SqliteMigrationCatalog.Load());

        try
        {
            await legacyMigrationService.ApplyPendingMigrations();
            using (SqliteConnection connection = factory.CreateOpenConnection())
            {
                await connection.ExecuteAsync("INSERT INTO User (Id, Email, Name, Alias, SecurityStamp) VALUES (42, 'owner@example.test', 'Owner', 'owner', 'stamp');");
                await connection.ExecuteAsync("INSERT INTO Collection (Id, UserId, Name, Slug) VALUES (11, 42, 'Inbox', 'inbox');");
                await connection.ExecuteAsync("INSERT INTO Link (Id, UserId, CollectionId, UrlOriginal, UrlNormalized) VALUES (13, 42, 11, 'https://example.test', 'https://example.test');");
            }

            await migrationService.ApplyPendingMigrations();

            using (SqliteConnection migratedConnection = factory.CreateOpenConnection())
            {
                Assert.Equal(new[] { "V004", "V005", "V006", "V007" }, migratedConnection.Query<string>("SELECT version FROM schema_migrations ORDER BY version;").ToArray());
                Assert.Equal("Owner", migratedConnection.QuerySingle<string>("SELECT Name FROM User WHERE Id = 42;"));
                Assert.Equal("Inbox", migratedConnection.QuerySingle<string>("SELECT Name FROM Collection WHERE Id = 11;"));
                Assert.Equal("https://example.test", migratedConnection.QuerySingle<string>("SELECT UrlOriginal FROM Link WHERE Id = 13;"));
                Assert.Equal(new[] { "Collection", "CollectionSearch", "Link", "LinkSearch", "User" }, GetApplicationTableNames(migratedConnection));
            }
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task ApplyPendingMigrations_WhenLegacyDescriptionIsDecomposed_SearchFindsNormalizedText()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteMigrationService legacyMigrationService = new(factory, SqliteMigrationCatalog.Load().Where(script => script.Version == "V004"));
        SqliteMigrationService previousMigrationService = new(factory, SqliteMigrationCatalog.Load().Where(script => string.CompareOrdinal(script.Version, "V007") < 0));
        SqliteMigrationService migrationService = new(factory, SqliteMigrationCatalog.Load());

        try
        {
            await legacyMigrationService.ApplyPendingMigrations();
            using (SqliteConnection connection = factory.CreateOpenConnection())
            {
                await connection.ExecuteAsync("INSERT INTO User (Id, Email, Name, Alias, SecurityStamp) VALUES (42, 'owner@example.test', 'Owner', 'owner', 'stamp');");
                await connection.ExecuteAsync(
                    "INSERT INTO Collection (Id, UserId, Name, Slug, Description) VALUES (11, 42, 'Recipes', 'recipes', @Description);",
                    new { Description = "\u1100\u1161" });
            }

            await previousMigrationService.ApplyPendingMigrations();
            await migrationService.ApplyPendingMigrations();
            await migrationService.ApplyPendingMigrations();

            using SqliteConnection migratedConnection = factory.CreateOpenConnection();
            SqliteCollectionRepository repository = new(migratedConnection);
            IReadOnlyCollection<Linkubator.Application.Dtos.Models.CollectionDto> result = await repository.GetPageByUser(42, "\uAC00", 1);

            Assert.Equal(new long[] { 11 }, result.Select(item => item.Id));
            Assert.Equal("\u1100\u1161", await migratedConnection.QuerySingleAsync<string>("SELECT Description FROM Collection WHERE Id = 11;"));
            Assert.Equal("\uAC00", await migratedConnection.QuerySingleAsync<string>("SELECT Description FROM CollectionSearch WHERE rowid = 11;"));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static string[] GetApplicationTableNames(SqliteConnection connection)
        => connection.Query<string>("SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name <> 'schema_migrations' AND name NOT LIKE 'LinkSearch_%' AND name NOT LIKE 'CollectionSearch_%' ORDER BY name;").ToArray();

    private static string[] GetProjectionColumns(SqliteConnection connection, string projectionName)
        => connection.Query($"PRAGMA table_info('{projectionName}');").Select(column => (string)column.name).ToArray();

    private static string GetProjectionDefinition(SqliteConnection connection, string projectionName)
        => connection.QuerySingle<string>("SELECT sql FROM sqlite_master WHERE type = 'table' AND name = @ProjectionName;", new { ProjectionName = projectionName });

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
                }
            }
        }
    }
}
