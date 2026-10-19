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
        long collectionCount = await _connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM Collection WHERE UserId = @UserId;",
            new { UserId = userId });

        long linkCount = await _connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM Link WHERE UserId = @UserId;",
            new { UserId = userId });

        IReadOnlyList<CollectionDto> collections = (await _connection.QueryAsync<CollectionSummaryRow>(
            @"SELECT c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt,
                     COUNT(l.Id) AS LinkCount
              FROM Collection c
              LEFT JOIN Link l ON l.UserId = c.UserId AND l.CollectionId = c.Id
              WHERE c.UserId = @UserId
              GROUP BY c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt
              ORDER BY c.Name COLLATE LINKUBATOR_ES_ES ASC, c.Id ASC;",
            new { UserId = userId }))
            .Select(row => new CollectionDto(
                row.Id,
                row.Name,
                row.Slug,
                row.Description,
                row.IsPublic == 1,
                row.LinkCount,
                row.CreatedAt))
            .ToList();

        IReadOnlyList<LinkDto> recentLinks = (await _connection.QueryAsync<LinkSummaryRow>(
            @"SELECT Id, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, CreatedAt
              FROM Link
              WHERE UserId = @UserId
              ORDER BY CreatedAt DESC, Id DESC
              LIMIT 5;",
            new { UserId = userId }))
            .Select(row => new LinkDto(
                row.Id,
                row.CollectionId,
                row.UrlOriginal,
                row.UrlNormalized,
                row.Title,
                row.Description,
                row.Image,
                row.IsPublic == 1,
                row.CreatedAt))
            .ToList();

        return new DashboardDto(
            (int)collectionCount,
            (int)linkCount,
            collections,
            recentLinks);
    }

    private sealed class CollectionSummaryRow
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long IsPublic { get; set; }
        public int LinkCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed class LinkSummaryRow
    {
        public long Id { get; set; }
        public long CollectionId { get; set; }
        public string UrlOriginal { get; set; } = string.Empty;
        public string UrlNormalized { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public long IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
