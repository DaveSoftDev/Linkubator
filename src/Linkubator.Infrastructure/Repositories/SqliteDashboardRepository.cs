using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Repositories;

public sealed class SqliteDashboardRepository : IDashboardRepository
{
    private readonly SqliteConnection _connection;

    public SqliteDashboardRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<DashboardDto> GetByUser(long userId, CancellationToken cancellationToken = default)
    {
        long collectionCount = await _connection.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) FROM Collection WHERE UserId = @UserId;",
            new { UserId = userId },
            cancellationToken: cancellationToken));

        long linkCount = await _connection.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(*) FROM Link WHERE UserId = @UserId;",
            new { UserId = userId },
            cancellationToken: cancellationToken));

        IReadOnlyList<CollectionDto> collections = (await _connection.QueryAsync<CollectionDto>(new CommandDefinition(
            @"SELECT c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt,
                     COUNT(l.Id) AS LinkCount
              FROM Collection c
              LEFT JOIN Link l ON l.UserId = c.UserId AND l.CollectionId = c.Id
              WHERE c.UserId = @UserId
              GROUP BY c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt
              ORDER BY c.Name COLLATE LINKUBATOR_ES_ES ASC, c.Id ASC;",
            new { UserId = userId },
            cancellationToken: cancellationToken)))
            .ToList();

        IReadOnlyList<LinkDto> recentLinks = (await _connection.QueryAsync<LinkDto>(new CommandDefinition(
            @"SELECT Id, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, CreatedAt
              FROM Link
              WHERE UserId = @UserId
              ORDER BY CreatedAt DESC, Id DESC
              LIMIT 5;",
            new { UserId = userId },
            cancellationToken: cancellationToken)))
            .ToList();

        return new DashboardDto(
            (int)collectionCount,
            (int)linkCount,
            collections,
            recentLinks);
    }

}
