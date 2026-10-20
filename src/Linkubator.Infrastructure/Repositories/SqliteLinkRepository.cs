using System.Reflection;
using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Microsoft.Data.Sqlite;
using Entities = Linkubator.Domain.Entities;
using Search = Linkubator.Infrastructure.Search;

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
        LinkRow? row = await _connection.QuerySingleOrDefaultAsync<LinkRow>(
            @"SELECT Id, UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt
              FROM Link
              WHERE Id = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId });

        return row is null ? null : Map(row);
    }

    public async Task<Entities.Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
    {
        LinkRow? row = await _connection.QuerySingleOrDefaultAsync<LinkRow>(
            @"SELECT Id, UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt
              FROM Link
              WHERE UserId = @UserId AND UrlNormalized = @UrlNormalized;",
            new { UserId = userId, UrlNormalized = normalizedUrl });

        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyCollection<Entities.Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
    {
        IEnumerable<LinkRow> rows = await _connection.QueryAsync<LinkRow>(
            @"SELECT Id, UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt
              FROM Link
              WHERE UserId = @UserId
              ORDER BY CreatedAt DESC, Id DESC;",
            new { UserId = userId });

        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyCollection<Entities.Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
    {
        IEnumerable<LinkRow> rows = await _connection.QueryAsync<LinkRow>(
            @"SELECT Id, UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, ScrapingStatus, CreatedAt
              FROM Link
              WHERE UserId = @UserId AND CollectionId = @CollectionId
              ORDER BY CreatedAt DESC, Id DESC;",
            new { UserId = userId, CollectionId = collectionId });

        return rows.Select(Map).ToList();
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
        IEnumerable<LinkListRow> rows = await _connection.QueryAsync<LinkListRow>(
            sql,
            new { UserId = userId, CollectionId = collectionId, SearchQuery = searchQuery, Limit = PageSize, Offset = offset });

        return rows.Select(row => new LinkDto(
            row.Id,
            row.CollectionId,
            row.UrlOriginal,
            row.UrlNormalized,
            row.Title,
            row.Description,
            row.Image,
            row.IsPublic,
            row.CreatedAt)).ToList();
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
        long total = await _connection.ExecuteScalarAsync<long>(
            sql,
            new { UserId = userId, CollectionId = collectionId, SearchQuery = searchQuery });

        if (total == 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(total / (double)PageSize);
    }

    public async Task Insert(Entities.Link link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);

        long id = await _connection.ExecuteScalarAsync<long>(
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
            });

        SetProperty(link, nameof(Entities.Link.Id), id);

        await InsertSearchProjection(id, link.UserId, link.CollectionId, link.UrlOriginal, link.Title, link.Description, cancellationToken);
    }

    public async Task Update(Entities.Link link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);

        int affectedRows = await _connection.ExecuteAsync(
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
            });

        if (affectedRows > 0)
        {
            await DeleteSearchProjection(link.Id, link.UserId, cancellationToken);
            await InsertSearchProjection(link.Id, link.UserId, link.CollectionId, link.UrlOriginal, link.Title, link.Description, cancellationToken);
        }
    }

    public async Task Delete(long id, long userId, CancellationToken cancellationToken = default)
    {
        await DeleteSearchProjection(id, userId, cancellationToken);
        await _connection.ExecuteAsync(
            "DELETE FROM Link WHERE Id = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId });
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

    private static Entities.Link Map(LinkRow row)
    {
        Entities.Link link = CreateEntity();
        SetProperty(link, nameof(Entities.Link.Id), row.Id);
        SetProperty(link, nameof(Entities.Link.UserId), row.UserId);
        SetProperty(link, nameof(Entities.Link.CollectionId), row.CollectionId);
        SetProperty(link, nameof(Entities.Link.UrlOriginal), row.UrlOriginal);
        SetProperty(link, nameof(Entities.Link.UrlNormalized), row.UrlNormalized);
        SetProperty(link, nameof(Entities.Link.Title), row.Title);
        SetProperty(link, nameof(Entities.Link.Description), row.Description);
        SetProperty(link, nameof(Entities.Link.Image), row.Image);
        SetProperty(link, nameof(Entities.Link.IsPublic), row.IsPublic == 1);
        SetProperty(link, nameof(Entities.Link.ScrapingStatus), (LinkScrapingStatus)row.ScrapingStatus);
        SetProperty(link, nameof(Entities.Link.CreatedAt), ParseDate(row.CreatedAt));
        return link;
    }

    private static Entities.Link CreateEntity()
        => (Entities.Link)Activator.CreateInstance(typeof(Entities.Link), nonPublic: true)!;

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo? property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property is null)
        {
            return;
        }

        MethodInfo? setter = property.SetMethod ?? property.GetSetMethod(true);
        setter?.Invoke(target, new object[] { value! });
    }

    private static DateTime ParseDate(string value)
        => DateTime.TryParse(value, out DateTime parsed) ? parsed : DateTime.UtcNow;

    private enum LinkScrapingStatus
    {
        NotRequested = 0,
        InProgress = 1,
        Completed = 2,
        Failed = 3,
    }

    private sealed class LinkRow
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long CollectionId { get; set; }
        public string UrlOriginal { get; set; } = string.Empty;
        public string UrlNormalized { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public long IsPublic { get; set; }
        public int ScrapingStatus { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    private sealed class LinkListRow
    {
        public long Id { get; set; }
        public long CollectionId { get; set; }
        public string UrlOriginal { get; set; } = string.Empty;
        public string UrlNormalized { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
