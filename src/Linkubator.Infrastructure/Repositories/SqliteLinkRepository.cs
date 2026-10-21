using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Microsoft.Data.Sqlite;
using Entities = Linkubator.Domain.Entities;

namespace Linkubator.Infrastructure.Repositories;

public sealed class SqliteLinkRepository : ILinkRepository
{
    private const int PageSize = 15;
    private readonly SqliteConnection _connection;

    public SqliteLinkRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<Entities.Link?> GetById(long id, long userId, CancellationToken cancellationToken = default)
    {
        return await _connection.QuerySingleOrDefaultAsync<Entities.Link>(new CommandDefinition(
            @"SELECT Id, UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt
              FROM Link
              WHERE Id = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId },
            cancellationToken: cancellationToken));
    }

    public async Task<Entities.Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
    {
        return await _connection.QuerySingleOrDefaultAsync<Entities.Link>(new CommandDefinition(
            @"SELECT Id, UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt
              FROM Link
              WHERE UserId = @UserId AND UrlNormalized = @UrlNormalized;",
            new { UserId = userId, UrlNormalized = normalizedUrl },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<Entities.Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
    {
        IEnumerable<Entities.Link> links = await _connection.QueryAsync<Entities.Link>(new CommandDefinition(
            @"SELECT Id, UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt
              FROM Link
              WHERE UserId = @UserId
              ORDER BY CreatedAt DESC, Id DESC;",
            new { UserId = userId },
            cancellationToken: cancellationToken));

        return links.ToList();
    }

    public async Task<IReadOnlyCollection<Entities.Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
    {
        IEnumerable<Entities.Link> links = await _connection.QueryAsync<Entities.Link>(new CommandDefinition(
            @"SELECT Id, UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt
              FROM Link
              WHERE UserId = @UserId AND CollectionId = @CollectionId
              ORDER BY CreatedAt DESC, Id DESC;",
            new { UserId = userId, CollectionId = collectionId },
            cancellationToken: cancellationToken));

        return links.ToList();
    }

    public async Task<IReadOnlyCollection<LinkDto>> GetPageByUser(long userId, string? query, long? collectionId, int page, CancellationToken cancellationToken = default)
    {
        string? searchQuery = Search.SearchQueryBuilder.Build(Search.SearchTextNormalizer.Normalize(query));
        int offset = (page - 1) * PageSize;
        string sql = searchQuery is null
            ? @"SELECT l.Id, l.CollectionId, l.UrlOriginal, l.UrlNormalized, l.Title, l.Description, l.Image, l.IsPublic, l.CreatedAt
                FROM Link l
                WHERE l.UserId = @UserId
                  AND (@CollectionId IS NULL OR l.CollectionId = @CollectionId)
                ORDER BY l.CreatedAt DESC, l.Id DESC
                LIMIT @Limit OFFSET @Offset;"
            : @"SELECT l.Id, l.CollectionId, l.UrlOriginal, l.UrlNormalized, l.Title, l.Description, l.Image, l.IsPublic, l.CreatedAt
                FROM LinkSearch
                INNER JOIN Link l ON l.Id = LinkSearch.rowid AND l.UserId = LinkSearch.UserId
                WHERE LinkSearch MATCH @SearchQuery
                  AND LinkSearch.UserId = @UserId
                  AND l.UserId = @UserId
                  AND (@CollectionId IS NULL OR l.CollectionId = @CollectionId)
                ORDER BY rank ASC, l.CreatedAt DESC, l.Id DESC
                LIMIT @Limit OFFSET @Offset;";
        IEnumerable<LinkDto> links = await _connection.QueryAsync<LinkDto>(new CommandDefinition(
            sql,
            new { UserId = userId, CollectionId = collectionId, SearchQuery = searchQuery, Limit = PageSize, Offset = offset },
            cancellationToken: cancellationToken));

        return links.ToList();
    }

    public async Task<int> GetPageCountByUser(long userId, string? query, long? collectionId, CancellationToken cancellationToken = default)
    {
        string? searchQuery = Search.SearchQueryBuilder.Build(Search.SearchTextNormalizer.Normalize(query));
        string sql = searchQuery is null
            ? @"SELECT COUNT(*)
                FROM Link
                WHERE UserId = @UserId
                  AND (@CollectionId IS NULL OR CollectionId = @CollectionId);"
            : @"SELECT COUNT(*)
                FROM LinkSearch
                INNER JOIN Link l ON l.Id = LinkSearch.rowid AND l.UserId = LinkSearch.UserId
                WHERE LinkSearch MATCH @SearchQuery
                  AND LinkSearch.UserId = @UserId
                  AND l.UserId = @UserId
                  AND (@CollectionId IS NULL OR l.CollectionId = @CollectionId);";
        long total = await _connection.ExecuteScalarAsync<long>(new CommandDefinition(
            sql,
            new { UserId = userId, CollectionId = collectionId, SearchQuery = searchQuery },
            cancellationToken: cancellationToken));

        if (total == 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(total / (double)PageSize);
    }

    public async Task Insert(Entities.Link link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);

        long id = await _connection.ExecuteScalarAsync<long>(new CommandDefinition(
            @"INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt)
              VALUES (@UserId, @CollectionId, @UrlOriginal, @UrlNormalized, @Title, @Description, @Image, @IsPublic, @ScrapingStatus, @CreatedAt);
              SELECT last_insert_rowid();",
            new
            {
                UserId = link.UserId,
                CollectionId = link.CollectionId,
                UrlOriginal = link.UrlOriginal,
                UrlNormalized = link.UrlNormalized,
                Title = link.Title,
                Description = link.Description,
                Image = link.Image,
                IsPublic = link.IsPublic ? 1 : 0,
                ScrapingStatus = (int)link.ScrapingStatus,
                CreatedAt = link.CreatedAt,
            },
            cancellationToken: cancellationToken));

        await InsertSearchProjection(id, link.UserId, link.CollectionId, link.UrlOriginal, link.Title, link.Description, cancellationToken);

        link.SetId(id);
    }

    public async Task Update(Entities.Link link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);

        int affectedRows = await _connection.ExecuteAsync(new CommandDefinition(
            @"UPDATE Link
              SET CollectionId = @CollectionId,
                  UrlOriginal = @UrlOriginal,
                  UrlNormalized = @UrlNormalized,
                  Title = @Title,
                  Description = @Description,
                  Image = @Image,
                  IsPublic = @IsPublic,
                  ScrapingStatus = @ScrapingStatus
              WHERE Id = @Id AND UserId = @UserId;",
            new
            {
                Id = link.Id,
                UserId = link.UserId,
                CollectionId = link.CollectionId,
                UrlOriginal = link.UrlOriginal,
                UrlNormalized = link.UrlNormalized,
                Title = link.Title,
                Description = link.Description,
                Image = link.Image,
                IsPublic = link.IsPublic ? 1 : 0,
                ScrapingStatus = (int)link.ScrapingStatus,
            },
            cancellationToken: cancellationToken));

        if (affectedRows > 0)
        {
            await DeleteSearchProjection(link.Id, link.UserId, cancellationToken);
            await InsertSearchProjection(link.Id, link.UserId, link.CollectionId, link.UrlOriginal, link.Title, link.Description, cancellationToken);
        }
    }

    public async Task Delete(long id, long userId, CancellationToken cancellationToken = default)
    {
        await DeleteSearchProjection(id, userId, cancellationToken);
        await _connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM Link WHERE Id = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId },
            cancellationToken: cancellationToken));
    }

    private async Task InsertSearchProjection(
        long id,
        long userId,
        long collectionId,
        string urlOriginal,
        string? title,
        string? description,
        CancellationToken cancellationToken)
    {
        string? collectionName = await _connection.ExecuteScalarAsync<string?>(
            new CommandDefinition(
                "SELECT Name FROM Collection WHERE Id = @CollectionId AND UserId = @UserId;",
                new { CollectionId = collectionId, UserId = userId },
                cancellationToken: cancellationToken));

        await _connection.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO LinkSearch (rowid, UserId, UrlOriginal, Title, Description, CollectionName)
              VALUES (@Id, @UserId, @UrlOriginal, @Title, @Description, @CollectionName);",
            new
            {
                Id = id,
                UserId = userId,
                UrlOriginal = Search.SearchTextNormalizer.Normalize(urlOriginal),
                Title = Search.SearchTextNormalizer.Normalize(title),
                Description = Search.SearchTextNormalizer.Normalize(description),
                CollectionName = Search.SearchTextNormalizer.Normalize(collectionName),
            },
            cancellationToken: cancellationToken));
    }

    private Task DeleteSearchProjection(long id, long userId, CancellationToken cancellationToken)
        => _connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId },
            cancellationToken: cancellationToken));

}
