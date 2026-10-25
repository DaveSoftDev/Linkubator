using Dapper;
using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.Search;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteSearchProjectionRebuilder : ISqliteSearchProjectionRebuilder
{
    private const string ClearProjectionsSql = "DELETE FROM LinkSearch; DELETE FROM CollectionSearch;";
    private const string RebuildCollectionsSql = """
        INSERT INTO CollectionSearch (rowid, UserId, Name, Description)
        SELECT Id, UserId, NormalizeSearchText(Name), NormalizeSearchText(Description)
        FROM Collection;
        """;
    private const string RebuildLinksSql = """
        INSERT INTO LinkSearch (rowid, UserId, UrlOriginal, Title, Description, CollectionName)
        SELECT Link.Id,
               Link.UserId,
               NormalizeSearchText(Link.UrlOriginal),
               NormalizeSearchText(Link.Title),
               NormalizeSearchText(Link.Description),
               NormalizeSearchText(Collection.Name)
        FROM Link
        INNER JOIN Collection
            ON Collection.Id = Link.CollectionId
           AND Collection.UserId = Link.UserId;
        """;

    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteSearchProjectionRebuilder(ISqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public async Task Rebuild(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
        connection.CreateFunction<string?, string?>("NormalizeSearchText", SearchTextNormalizer.Normalize, isDeterministic: true);
        await using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(ClearProjectionsSql, transaction: transaction, cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(RebuildCollectionsSql, transaction: transaction, cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(RebuildLinksSql, transaction: transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
