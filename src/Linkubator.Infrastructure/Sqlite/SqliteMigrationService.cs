using System.Text.RegularExpressions;
using Dapper;
using Linkubator.Infrastructure.Contracts;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteMigrationService : ISqliteMigrationService
{
    private const string MigrationHistoryTableName = "schema_migrations";
    private static readonly Regex VersionRegex = new("^V\\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly IReadOnlyList<SqliteMigrationScript> _scripts;

    public SqliteMigrationService(ISqliteConnectionFactory connectionFactory)
        : this(connectionFactory, SqliteMigrationCatalog.Load())
    {
    }

    public SqliteMigrationService(ISqliteConnectionFactory connectionFactory, IEnumerable<SqliteMigrationScript> scripts)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(scripts);

        _connectionFactory = connectionFactory;
        _scripts = scripts
            .OrderBy(script => ParseVersion(script.Version))
            .ToArray();

        if (_scripts.Count == 0)
        {
            throw new InvalidOperationException("At least one migration script is required.");
        }

        foreach (SqliteMigrationScript script in _scripts)
        {
            if (string.IsNullOrWhiteSpace(script.Version) || !VersionRegex.IsMatch(script.Version))
            {
                throw new InvalidOperationException($"Migration version '{script.Version}' is invalid. Expected format VNNN.");
            }

            if (string.IsNullOrWhiteSpace(script.Sql))
            {
                throw new InvalidOperationException($"Migration '{script.Version}' has no SQL content.");
            }
        }
    }

    public async Task ApplyPendingMigrations()
    {
        using SqliteConnection connection = _connectionFactory.CreateOpenConnection();

        await EnsureMigrationHistoryTableAsync(connection);

        string[] appliedVersions = (await connection.QueryAsync<string>(
            $"SELECT version FROM {MigrationHistoryTableName} ORDER BY version;"))
            .Select(version => version.Trim())
            .ToArray();

        foreach (SqliteMigrationScript script in _scripts)
        {
            if (appliedVersions.Contains(script.Version, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(script.Sql, transaction: transaction));
                await connection.ExecuteAsync(new CommandDefinition(
                    $"INSERT INTO {MigrationHistoryTableName} (version, applied_at_utc) VALUES (@Version, @AppliedAtUtc);",
                    new { Version = script.Version, AppliedAtUtc = DateTime.UtcNow },
                    transaction: transaction));
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    private static async Task EnsureMigrationHistoryTableAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($@"
            CREATE TABLE IF NOT EXISTS {MigrationHistoryTableName} (
                version TEXT PRIMARY KEY,
                applied_at_utc TEXT NOT NULL
            );
        ");
    }

    private static long ParseVersion(string version)
    {
        string digits = version.Substring(1);
        return long.TryParse(digits, out long parsed) ? parsed : long.MaxValue;
    }
}
