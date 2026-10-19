using System.Globalization;
using Dapper;
using Linkubator.Infrastructure.Contracts;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private const int BusyTimeoutMilliseconds = 5000;
    private const string SpanishCollationName = "LINKUBATOR_ES_ES";

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

        connection.CreateCollation(SpanishCollationName, CompareSpanish);
        connection.Execute("PRAGMA journal_mode = WAL;");
        connection.Execute($"PRAGMA busy_timeout = {BusyTimeoutMilliseconds};");
        connection.Execute("PRAGMA foreign_keys = ON;");

        return connection;
    }

    private static int CompareSpanish(string? left, string? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left is null)
        {
            return right is null ? 0 : -1;
        }

        if (right is null)
        {
            return 1;
        }

        CompareInfo compareInfo = CultureInfo.GetCultureInfo("es-ES").CompareInfo;
        return compareInfo.Compare(left, right, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
    }
}
