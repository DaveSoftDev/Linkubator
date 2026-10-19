using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Repositories;

public sealed class SqlitePublicReadRepository : IPublicReadRepository
{
    private const int PageSize = 15;
    private readonly SqliteConnection _connection;

    public SqlitePublicReadRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<PublicUserDto?> GetUserPage(string alias, CancellationToken cancellationToken = default)
    {
        var user = await _connection.QuerySingleOrDefaultAsync<UserRow>(
            "SELECT Id, Alias FROM User WHERE Alias = @Alias;",
            new { Alias = alias });

        if (user is null)
        {
            return null;
        }

        IReadOnlyList<PublicCollectionSummaryDto> collections = (await _connection.QueryAsync<PublicCollectionRow>(
            @"SELECT c.Name, c.Slug, c.Description
              FROM Collection c
              WHERE c.UserId = @UserId
                AND c.IsPublic = 1
                AND EXISTS (
                    SELECT 1
                    FROM Link l
                    WHERE l.UserId = c.UserId
                      AND l.CollectionId = c.Id
                      AND l.IsPublic = 1)
              ORDER BY c.Name COLLATE LINKUBATOR_ES_ES ASC, c.Id ASC;",
            new { UserId = user.Id }))
            .Select(row => new PublicCollectionSummaryDto(row.Name, row.Slug, row.Description))
            .ToList();

        IReadOnlyList<PublicLinkDto> recentLinks = (await _connection.QueryAsync<PublicLinkRow>(
            @"SELECT l.UrlOriginal, l.Title, l.Description, l.Image
              FROM Link l
              INNER JOIN Collection c ON c.Id = l.CollectionId AND c.UserId = l.UserId
              WHERE l.UserId = @UserId
                AND l.IsPublic = 1
                AND c.IsPublic = 1
              ORDER BY l.CreatedAt DESC, l.Id DESC
              LIMIT 5;",
            new { UserId = user.Id }))
            .Select(row => new PublicLinkDto(row.UrlOriginal, row.Title, row.Description, row.Image))
            .ToList();

        return new PublicUserDto(alias, collections, recentLinks);
    }

    public async Task<IReadOnlyCollection<PublicCollectionSummaryDto>?> GetCollectionsPage(string alias, CancellationToken cancellationToken = default)
    {
        var user = await _connection.QuerySingleOrDefaultAsync<UserRow>(
            "SELECT Id, Alias FROM User WHERE Alias = @Alias;",
            new { Alias = alias });

        if (user is null)
        {
            return null;
        }

        return (await _connection.QueryAsync<PublicCollectionRow>(
            @"SELECT c.Name, c.Slug, c.Description
              FROM Collection c
              WHERE c.UserId = @UserId
                AND c.IsPublic = 1
                AND EXISTS (
                    SELECT 1
                    FROM Link l
                    WHERE l.UserId = c.UserId
                      AND l.CollectionId = c.Id
                      AND l.IsPublic = 1)
              ORDER BY c.Name COLLATE LINKUBATOR_ES_ES ASC, c.Id ASC;",
            new { UserId = user.Id }))
            .Select(row => new PublicCollectionSummaryDto(row.Name, row.Slug, row.Description))
            .ToList();
    }

    public async Task<PublicCollectionDto?> GetCollectionPage(string alias, string slug, int page, CancellationToken cancellationToken = default)
    {
        var user = await _connection.QuerySingleOrDefaultAsync<UserRow>(
            "SELECT Id, Alias FROM User WHERE Alias = @Alias;",
            new { Alias = alias });

        if (user is null)
        {
            return null;
        }

        var collection = await _connection.QuerySingleOrDefaultAsync<CollectionPageRow>(
            @"SELECT c.Id, c.Name, c.Slug
              FROM Collection c
              WHERE c.UserId = @UserId
                AND c.Slug = @Slug
                AND c.IsPublic = 1;",
            new { UserId = user.Id, Slug = slug });

        if (collection is null)
        {
            return null;
        }

        var links = (await _connection.QueryAsync<PublicLinkRow>(
            @"SELECT l.UrlOriginal, l.Title, l.Description, l.Image
              FROM Link l
              WHERE l.UserId = @UserId
                AND l.CollectionId = @CollectionId
                AND l.IsPublic = 1
              ORDER BY CASE WHEN l.Title IS NOT NULL AND TRIM(l.Title) <> '' THEN l.Title ELSE l.UrlOriginal END COLLATE LINKUBATOR_ES_ES ASC,
                       l.Id ASC
              LIMIT @Limit OFFSET @Offset;",
            new { UserId = user.Id, CollectionId = collection.Id, Limit = PageSize, Offset = (page - 1) * PageSize }))
            .Select(row => new PublicLinkDto(row.UrlOriginal, row.Title, row.Description, row.Image))
            .ToList();

        return new PublicCollectionDto(alias, collection.Name, collection.Slug, links);
    }

    public async Task<int> GetCollectionPageCount(string alias, string slug, CancellationToken cancellationToken = default)
    {
        var user = await _connection.QuerySingleOrDefaultAsync<UserRow>(
            "SELECT Id, Alias FROM User WHERE Alias = @Alias;",
            new { Alias = alias });

        if (user is null)
        {
            return 0;
        }

        var collection = await _connection.QuerySingleOrDefaultAsync<CollectionPageRow>(
            @"SELECT c.Id, c.Name, c.Slug
              FROM Collection c
              WHERE c.UserId = @UserId AND c.Slug = @Slug AND c.IsPublic = 1;",
            new { UserId = user.Id, Slug = slug });

        if (collection is null)
        {
            return 0;
        }

        long total = await _connection.ExecuteScalarAsync<long>(
            @"SELECT COUNT(*)
              FROM Link
              WHERE UserId = @UserId AND CollectionId = @CollectionId AND IsPublic = 1;",
            new { UserId = user.Id, CollectionId = collection.Id });

        return total == 0 ? 0 : (int)Math.Ceiling(total / (double)PageSize);
    }

    private sealed class UserRow
    {
        public long Id { get; set; }
        public string Alias { get; set; } = string.Empty;
    }

    private sealed class PublicCollectionRow
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    private sealed class PublicLinkRow
    {
        public string UrlOriginal { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    private sealed class CollectionPageRow
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}
