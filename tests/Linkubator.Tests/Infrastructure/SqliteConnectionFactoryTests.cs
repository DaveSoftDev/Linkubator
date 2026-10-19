using Dapper;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteConnectionFactoryTests
{
    [Fact]
    public void CreateOpenConnection_SetsWALBusyTimeoutAndForeignKeys()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteConnection connection = factory.CreateOpenConnection();

        try
        {
            Assert.Equal(1, connection.QuerySingle<int>("SELECT 1"));
            Assert.Equal("wal", connection.QuerySingle<string>("PRAGMA journal_mode;"), ignoreCase: true);
            Assert.Equal(5000, connection.QuerySingle<int>("PRAGMA busy_timeout;"));
            Assert.Equal(1, connection.QuerySingle<int>("PRAGMA foreign_keys;"));
        }
        finally
        {
            connection.Dispose();
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public void CreateOpenConnection_IndependentConnectionsKeepSameSettings()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteConnection firstConnection = factory.CreateOpenConnection();
        SqliteConnection secondConnection = factory.CreateOpenConnection();

        try
        {
            Assert.Equal("wal", firstConnection.QuerySingle<string>("PRAGMA journal_mode;"), ignoreCase: true);
            Assert.Equal("wal", secondConnection.QuerySingle<string>("PRAGMA journal_mode;"), ignoreCase: true);
            Assert.Equal(5000, firstConnection.QuerySingle<int>("PRAGMA busy_timeout;"));
            Assert.Equal(5000, secondConnection.QuerySingle<int>("PRAGMA busy_timeout;"));
            Assert.Equal(1, firstConnection.QuerySingle<int>("PRAGMA foreign_keys;"));
            Assert.Equal(1, secondConnection.QuerySingle<int>("PRAGMA foreign_keys;"));
        }
        finally
        {
            firstConnection.Dispose();
            secondConnection.Dispose();
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public void CreateOpenConnection_RegistersSpanishCollationForSqliteSorting()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteConnection connection = factory.CreateOpenConnection();

        try
        {
            int accentInsensitiveResult = connection.QuerySingle<int>("SELECT CASE WHEN 'Árbol' COLLATE LINKUBATOR_ES_ES = 'arbol' THEN 1 ELSE 0 END;");
            Assert.Equal(1, accentInsensitiveResult);

            int alphabeticalResult = connection.QuerySingle<int>("SELECT CASE WHEN 'Árbol' COLLATE LINKUBATOR_ES_ES < 'barranco' THEN 1 ELSE 0 END;");
            Assert.Equal(1, alphabeticalResult);
        }
        finally
        {
            connection.Dispose();
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public void CreateOpenConnection_RegistersSpanishCollationOnIndependentConnections()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteConnection firstConnection = factory.CreateOpenConnection();
        SqliteConnection secondConnection = factory.CreateOpenConnection();

        try
        {
            int firstResult = firstConnection.QuerySingle<int>("SELECT CASE WHEN 'Árbol' COLLATE LINKUBATOR_ES_ES = 'arbol' THEN 1 ELSE 0 END;");
            int secondResult = secondConnection.QuerySingle<int>("SELECT CASE WHEN 'Árbol' COLLATE LINKUBATOR_ES_ES = 'arbol' THEN 1 ELSE 0 END;");

            Assert.Equal(1, firstResult);
            Assert.Equal(1, secondResult);
        }
        finally
        {
            firstConnection.Dispose();
            secondConnection.Dispose();
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public void CreateOpenConnection_CollationOnlyAffectsExpressionsThatDeclareIt()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        SqliteConnection connection = factory.CreateOpenConnection();

        try
        {
            int explicitCollationResult = connection.QuerySingle<int>("SELECT CASE WHEN 'Árbol' COLLATE LINKUBATOR_ES_ES = 'arbol' THEN 1 ELSE 0 END;");
            int defaultComparisonResult = connection.QuerySingle<int>("SELECT CASE WHEN 'Árbol' = 'arbol' THEN 1 ELSE 0 END;");

            Assert.Equal(1, explicitCollationResult);
            Assert.Equal(0, defaultComparisonResult);
        }
        finally
        {
            connection.Dispose();
            DeleteDatabaseFiles(databasePath);
        }
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
