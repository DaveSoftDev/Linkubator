using System.Data.Common;
using System.Text.RegularExpressions;
using Dapper;
using Linkubator.Infrastructure.Contracts;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteMigrationService : ISqliteMigrationService
{
    private const string MigrationHistoryTableName = "schema_migrations";
    private static readonly Regex _versionRegex = new("^V\\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly IReadOnlyList<SqliteMigrationScript> _scripts;

    /// <summary>
    /// Crea el servicio de migraciones con la fábrica de conexiones y los scripts indicados.
    /// </summary>
    public SqliteMigrationService(ISqliteConnectionFactory connectionFactory)
        : this(connectionFactory, SqliteMigrationCatalog.Load())
    {
    }

    /// <summary>
    /// Crea el servicio de migraciones con la fábrica de conexiones y los scripts indicados.
    /// </summary>
    public SqliteMigrationService(ISqliteConnectionFactory connectionFactory, IEnumerable<SqliteMigrationScript> scripts)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(scripts);

        _connectionFactory = connectionFactory;
        _scripts = scripts.ToArray();

        if (_scripts.Count == 0)
        {
            throw new InvalidOperationException("At least one migration script is required.");
        }

        foreach (SqliteMigrationScript script in _scripts)
        {
            if (string.IsNullOrWhiteSpace(script.Version) || !_versionRegex.IsMatch(script.Version))
            {
                throw new InvalidOperationException($"Migration version '{script.Version}' is invalid. Expected format VNNN.");
            }

            if (string.IsNullOrWhiteSpace(script.Sql))
            {
                throw new InvalidOperationException($"Migration '{script.Version}' has no SQL content.");
            }
        }

        _scripts = _scripts.OrderBy(script => ParseVersion(script.Version)).ToArray();
    }

    /// <summary>
    /// Aplica las migraciones pendientes y registra cada versión aplicada.
    /// </summary>
    public async Task ApplyPendingMigrations()
    {
        using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
        connection.CreateFunction<string?, string?>("NormalizeSearchText", Linkubator.Infrastructure.Search.SearchTextNormalizer.Normalize, isDeterministic: true);

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

            await using DbTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(script.Sql, transaction: transaction));
                await connection.ExecuteAsync(new CommandDefinition(
                    $"INSERT INTO {MigrationHistoryTableName} (version, applied_at_utc) VALUES (@Version, @AppliedAtUtc);",
                    new { Version = script.Version, AppliedAtUtc = DateTime.UtcNow },
                    transaction: transaction));
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    /// <summary>
    /// Crea la tabla de historial de migraciones si todavía no existe.
    /// </summary>
    private static async Task EnsureMigrationHistoryTableAsync(SqliteConnection connection)
    {
        await connection.ExecuteAsync($@"
            CREATE TABLE IF NOT EXISTS {MigrationHistoryTableName} (
                version TEXT PRIMARY KEY,
                applied_at_utc TEXT NOT NULL
            );
        ");
    }

    /// <summary>
    /// Convierte la versión de migración en su valor numérico para ordenar.
    /// </summary>
    private static long ParseVersion(string version)
    {
        string digits = version.Substring(1);
        return long.TryParse(digits, out long parsed) ? parsed : long.MaxValue;
    }
}
