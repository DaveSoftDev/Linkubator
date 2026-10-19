using System.Globalization;
using Dapper;
using Linkubator.Infrastructure.Contracts;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private const string PragmaJournalModeWal = "PRAGMA journal_mode = WAL;";
    private const string PragmaBusyTimeout5000 = "PRAGMA busy_timeout = 5000;";
    private const string PragmaForeignKeysOn = "PRAGMA foreign_keys = ON;";
    private const string SpanishCollationName = "LINKUBATOR_ES_ES";

    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public SqliteConnection CreateOpenConnection()
    {
        EnsureParentDirectoryExists();

        SqliteConnection connection = new(_connectionString);
        connection.Open();

        connection.CreateCollation(SpanishCollationName, CompareSpanish);
        connection.Execute(PragmaJournalModeWal);
        connection.Execute(PragmaBusyTimeout5000);
        connection.Execute(PragmaForeignKeysOn);

        return connection;
    }

    private void EnsureParentDirectoryExists()
    {
        SqliteConnectionStringBuilder builder = new();
        builder.ConnectionString = _connectionString;

        string? dataSource = builder.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:" || dataSource == "file::memory:?cache=shared")
        {
            return;
        }

        string? directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
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
