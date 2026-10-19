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
            new SqliteMigrationScript("V001", "CREATE TABLE sample_data (id INTEGER PRIMARY KEY, value TEXT);"),
            new SqliteMigrationScript("V002", "INSERT INTO sample_data (value) VALUES ('after-v002');"),
            new SqliteMigrationScript("V003", "UPDATE sample_data SET value = 'after-v003' WHERE value = 'after-v002';")
        });

        await service.ApplyPendingMigrations();

        using SqliteConnection connection = factory.CreateOpenConnection();
        Assert.Equal(new[] { "V001", "V002", "V003" }, connection.Query<string>("SELECT version FROM schema_migrations ORDER BY version").ToArray());
        Assert.Equal(1, connection.QuerySingle<int>("SELECT COUNT(*) FROM sample_data;"));
        Assert.Equal("after-v003", connection.QuerySingle<string>("SELECT value FROM sample_data WHERE id = 1;"));

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
            new SqliteMigrationScript("V001", "CREATE TABLE sample_data (id INTEGER PRIMARY KEY, value TEXT);"),
            new SqliteMigrationScript("V002", "INSERT INTO missing_table (value) VALUES ('should fail');"),
            new SqliteMigrationScript("V003", "CREATE TABLE should_not_be_created (id INTEGER PRIMARY KEY);")
        });

        await Assert.ThrowsAsync<SqliteException>(async () => await service.ApplyPendingMigrations());

        using SqliteConnection connection = factory.CreateOpenConnection();
        Assert.Equal(new[] { "V001" }, connection.Query<string>("SELECT version FROM schema_migrations ORDER BY version").ToArray());
        Assert.Equal(0, connection.QuerySingle<int>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'should_not_be_created';"));

        DeleteDatabaseFiles(databasePath);
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
