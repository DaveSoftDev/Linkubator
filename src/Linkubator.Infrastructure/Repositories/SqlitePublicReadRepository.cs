using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Repositories;

public sealed class SqlitePublicReadRepository : IPublicReadRepository
{
    private const int PageSize = 16;
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Crea el repositorio de lecturas públicas asociado a la conexión SQLite indicada.
    /// </summary>
    public SqlitePublicReadRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// Obtiene la página pública del usuario, incluidas sus colecciones y enlaces públicos recientes.
    /// </summary>
    public async Task<PublicUserDto?> GetUserPage(string alias, CancellationToken cancellationToken = default)
    {
        UserRow? user = await GetUserByAlias(alias, cancellationToken);

        if (user is null)
        {
            return null;
        }

        IReadOnlyList<PublicCollectionSummaryDto> collections = (await _connection.QueryAsync<PublicCollectionRow>(new CommandDefinition(
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
              ORDER BY c.CreatedAt DESC, c.Id DESC
              LIMIT 5;",
            new { UserId = user.Id },
            cancellationToken: cancellationToken)))
            .Select(row => new PublicCollectionSummaryDto(row.Name, row.Slug, row.Description))
            .ToList();

        IReadOnlyList<PublicLinkDto> recentLinks = (await _connection.QueryAsync<PublicLinkDto>(new CommandDefinition(
            @"SELECT l.UrlOriginal, l.Title, l.Description, l.Image
              FROM Link l
              INNER JOIN Collection c ON c.Id = l.CollectionId AND c.UserId = l.UserId
              WHERE l.UserId = @UserId
                AND l.IsPublic = 1
                AND c.IsPublic = 1
              ORDER BY l.CreatedAt DESC, l.Id DESC
              LIMIT 6;",
            new { UserId = user.Id },
            cancellationToken: cancellationToken))).ToList();

        return new PublicUserDto(alias, collections, recentLinks);
    }

    /// <summary>
    /// Obtiene las colecciones públicas con enlaces públicos del usuario indicado.
    /// </summary>
    public async Task<IReadOnlyCollection<PublicCollectionSummaryDto>?> GetCollectionsPage(string alias, CancellationToken cancellationToken = default)
    {
        UserRow? user = await GetUserByAlias(alias, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return (await _connection.QueryAsync<PublicCollectionRow>(new CommandDefinition(
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
            new { UserId = user.Id },
            cancellationToken: cancellationToken)))
            .Select(row => new PublicCollectionSummaryDto(row.Name, row.Slug, row.Description))
            .ToList();
    }

    /// <summary>
    /// Obtiene una página de enlaces públicos de la colección identificada por alias y slug.
    /// </summary>
    public async Task<PublicCollectionDto?> GetCollectionPage(string alias, string slug, int page, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);

        List<CollectionPageRow> rows = (await _connection.QueryAsync<CollectionPageRow>(new CommandDefinition(
            @"WITH selected_collection AS MATERIALIZED (
                  SELECT c.Id, c.UserId, c.Name, c.Slug,
                         (SELECT COUNT(*) FROM Link l
                          WHERE l.UserId = c.UserId AND l.CollectionId = c.Id AND l.IsPublic = 1) AS LinkCount
                  FROM Collection c
                  INNER JOIN User u ON u.Id = c.UserId
                  WHERE u.Alias = @Alias AND c.Slug = @Slug AND c.IsPublic = 1
              ), pagination AS MATERIALIZED (
                  SELECT Id, UserId, Name, Slug,
                         (LinkCount + @Limit - 1) / @Limit AS PageCount,
                         MIN(@Page, MAX(1, (LinkCount + @Limit - 1) / @Limit)) AS Page
                  FROM selected_collection
              ), page_links AS (
                  SELECT l.Id, l.UrlOriginal, l.Title, l.Description, l.Image,
                         CASE WHEN l.Title IS NOT NULL AND TRIM(l.Title) <> '' THEN l.Title ELSE l.UrlOriginal END AS SortKey
                  FROM pagination p
                  CROSS JOIN Link l
                  WHERE l.UserId = p.UserId AND l.CollectionId = p.Id AND l.IsPublic = 1
                  ORDER BY SortKey COLLATE LINKUBATOR_ES_ES ASC, l.Id ASC
                  LIMIT @Limit OFFSET COALESCE((SELECT (Page - 1) * @Limit FROM pagination), 0)
              )
              SELECT p.Name, p.Slug, p.Page, p.PageCount,
                     l.UrlOriginal, l.Title, l.Description, l.Image
              FROM pagination p
              LEFT JOIN page_links l ON 1 = 1
              ORDER BY l.SortKey COLLATE LINKUBATOR_ES_ES ASC, l.Id ASC;",
            new { Alias = alias, Slug = slug, Page = page, Limit = PageSize },
            cancellationToken: cancellationToken))).ToList();

        CollectionPageRow? collection = rows.FirstOrDefault();
        if (collection is null)
        {
            return null;
        }

        IReadOnlyList<PublicLinkDto> links = rows
            .Where(row => row.UrlOriginal is not null)
            .Select(row => new PublicLinkDto(row.UrlOriginal!, row.Title, row.Description, row.Image))
            .ToList();
        return new PublicCollectionDto(alias, collection.Name, collection.Slug, links, collection.Page, collection.PageCount);
    }

    /// <summary>
    /// Busca el identificador y el alias del usuario por su alias público.
    /// </summary>
    private Task<UserRow?> GetUserByAlias(string alias, CancellationToken cancellationToken)
        => _connection.QuerySingleOrDefaultAsync<UserRow>(new CommandDefinition(
            "SELECT Id, Alias FROM User WHERE Alias = @Alias;",
            new { Alias = alias },
            cancellationToken: cancellationToken));

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

    private sealed class CollectionPageRow
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Page { get; set; }
        public int PageCount { get; set; }
        public string? UrlOriginal { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }
}
