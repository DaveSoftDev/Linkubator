using Dapper;
using Linkubator.Infrastructure.Contracts;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private const int BusyTimeoutMilliseconds = 5000;

    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public SqliteConnection CreateOpenConnection()
    {
        SqliteConnection connection = new(_connectionString);
        connection.Open();

        connection.Execute("PRAGMA journal_mode = WAL;");
        connection.Execute($"PRAGMA busy_timeout = {BusyTimeoutMilliseconds};");
        connection.Execute("PRAGMA foreign_keys = ON;");

        return connection;
    }
}
