using System.Reflection;
using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Microsoft.Data.Sqlite;
using Entities = Linkubator.Domain.Entities;

namespace Linkubator.Infrastructure.Repositories;

public sealed class SqliteCollectionRepository : ICollectionRepository
{
    private const int PageSize = 15;
    private readonly SqliteConnection _connection;

    public SqliteCollectionRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<Entities.Collection?> GetById(long id, long userId, CancellationToken cancellationToken = default)
    {
        CollectionRow? row = await _connection.QuerySingleOrDefaultAsync<CollectionRow>(new CommandDefinition(
            @"SELECT Id, UserId, Name, Slug, Description, IsPublic, CreatedAt
              FROM Collection
              WHERE Id = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId },
            commandTimeout: null,
            commandType: null,
            cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<Entities.Collection?> GetBySlug(long userId, string slug, CancellationToken cancellationToken = default)
    {
        CollectionRow? row = await _connection.QuerySingleOrDefaultAsync<CollectionRow>(new CommandDefinition(
            @"SELECT Id, UserId, Name, Slug, Description, IsPublic, CreatedAt
              FROM Collection
              WHERE UserId = @UserId AND Slug = @Slug;",
            new { UserId = userId, Slug = slug },
            commandTimeout: null,
            commandType: null,
            cancellationToken: cancellationToken));

        return row is null ? null : Map(row);
    }

    public async Task<IReadOnlyCollection<Entities.Collection>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
    {
        IEnumerable<CollectionRow> rows = await _connection.QueryAsync<CollectionRow>(new CommandDefinition(
            @"SELECT Id, UserId, Name, Slug, Description, IsPublic, CreatedAt
              FROM Collection
              WHERE UserId = @UserId
              ORDER BY Name COLLATE LINKUBATOR_ES_ES ASC, Id ASC;",
            new { UserId = userId },
            cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyCollection<CollectionDto>> GetPageByUser(long userId, string? query, int page, CancellationToken cancellationToken = default)
    {
        string? searchQuery = Search.SearchQueryBuilder.Build(Search.SearchTextNormalizer.Normalize(query));
        int offset = (page - 1) * PageSize;
        string sql = searchQuery is null
            ? @"SELECT c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt,
                       COUNT(l.Id) AS LinkCount
                FROM Collection c
                LEFT JOIN Link l ON l.UserId = c.UserId AND l.CollectionId = c.Id
                WHERE c.UserId = @UserId
                GROUP BY c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt
                ORDER BY c.Name COLLATE LINKUBATOR_ES_ES ASC, c.Id ASC
                LIMIT @Limit OFFSET @Offset;"
            : @"SELECT c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt,
                       COUNT(l.Id) AS LinkCount
                FROM CollectionSearch
                INNER JOIN Collection c ON c.Id = CollectionSearch.rowid AND c.UserId = CollectionSearch.UserId
                LEFT JOIN Link l ON l.UserId = c.UserId AND l.CollectionId = c.Id
                WHERE CollectionSearch MATCH @SearchQuery
                  AND CollectionSearch.UserId = @UserId
                  AND c.UserId = @UserId
                GROUP BY c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt
                ORDER BY c.Name COLLATE LINKUBATOR_ES_ES ASC, c.Id ASC
                LIMIT @Limit OFFSET @Offset;";
        IEnumerable<CollectionListRow> rows = await _connection.QueryAsync<CollectionListRow>(new CommandDefinition(
            sql,
            new { UserId = userId, SearchQuery = searchQuery, Limit = PageSize, Offset = offset },
            cancellationToken: cancellationToken));

        return rows.Select(row => new CollectionDto(
            row.Id,
            row.Name,
            row.Slug,
            row.Description,
            row.IsPublic,
            row.LinkCount,
            row.CreatedAt)).ToList();
    }

    public async Task<int> GetPageCountByUser(long userId, string? query, CancellationToken cancellationToken = default)
    {
        string? searchQuery = Search.SearchQueryBuilder.Build(Search.SearchTextNormalizer.Normalize(query));
        string sql = searchQuery is null
            ? "SELECT COUNT(*) FROM Collection WHERE UserId = @UserId;"
            : @"SELECT COUNT(*)
                FROM CollectionSearch
                INNER JOIN Collection c ON c.Id = CollectionSearch.rowid AND c.UserId = CollectionSearch.UserId
                WHERE CollectionSearch MATCH @SearchQuery
                  AND CollectionSearch.UserId = @UserId
                  AND c.UserId = @UserId;";
        long total = await _connection.ExecuteScalarAsync<long>(new CommandDefinition(
            sql,
            new { UserId = userId, SearchQuery = searchQuery },
            cancellationToken: cancellationToken));

        if (total == 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(total / (double)PageSize);
    }

    public async Task Insert(Entities.Collection collection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);

        DateTime createdAt = collection.CreatedAt;
        long id = await _connection.ExecuteScalarAsync<long>(new CommandDefinition(
            @"INSERT INTO Collection (UserId, Name, Slug, Description, IsPublic, CreatedAt)
              VALUES (@UserId, @Name, @Slug, @Description, @IsPublic, @CreatedAt);
              SELECT last_insert_rowid();",
            new
            {
                UserId = collection.UserId,
                Name = collection.Name,
                Slug = collection.Slug,
                Description = collection.Description,
                IsPublic = collection.IsPublic ? 1 : 0,
                CreatedAt = createdAt,
            },
            cancellationToken: cancellationToken));

        SetProperty(collection, nameof(Entities.Collection.Id), id);

        await _connection.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO CollectionSearch (rowid, UserId, Name)
              VALUES (@Id, @UserId, @Name);",
            new
            {
                Id = id,
                UserId = collection.UserId,
                Name = Search.SearchTextNormalizer.Normalize(collection.Name),
            },
            cancellationToken: cancellationToken));
    }

    public async Task Update(Entities.Collection collection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);

        int affectedRows = await _connection.ExecuteAsync(new CommandDefinition(
            @"UPDATE Collection
              SET Name = @Name,
                  Slug = @Slug,
                  Description = @Description,
                  IsPublic = @IsPublic
              WHERE Id = @Id AND UserId = @UserId;",
            new
            {
                Id = collection.Id,
                UserId = collection.UserId,
                Name = collection.Name,
                Slug = collection.Slug,
                Description = collection.Description,
                IsPublic = collection.IsPublic ? 1 : 0,
            },
            cancellationToken: cancellationToken));

        if (affectedRows > 0)
        {
            await _connection.ExecuteAsync(new CommandDefinition(
                "DELETE FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
                new { Id = collection.Id, UserId = collection.UserId },
                cancellationToken: cancellationToken));

            await _connection.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO CollectionSearch (rowid, UserId, Name)
                  VALUES (@Id, @UserId, @Name);",
                new
                {
                    Id = collection.Id,
                    UserId = collection.UserId,
                    Name = Search.SearchTextNormalizer.Normalize(collection.Name),
                },
                cancellationToken: cancellationToken));

            await _connection.ExecuteAsync(new CommandDefinition(
                @"UPDATE LinkSearch
                  SET CollectionName = @CollectionName
                  WHERE UserId = @UserId
                    AND rowid IN (SELECT Id FROM Link WHERE UserId = @UserId AND CollectionId = @CollectionId);",
                new
                {
                    Id = collection.Id,
                    UserId = collection.UserId,
                    CollectionId = collection.Id,
                    CollectionName = Search.SearchTextNormalizer.Normalize(collection.Name),
                },
                cancellationToken: cancellationToken));
        }
    }

    public async Task Delete(long id, long userId, CancellationToken cancellationToken = default)
    {
        await _connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId },
            cancellationToken: cancellationToken));

        await _connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM Collection WHERE Id = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId },
            cancellationToken: cancellationToken));
    }

    private static Entities.Collection Map(CollectionRow row)
    {
        Entities.Collection collection = CreateEntity();
        SetProperty(collection, nameof(Entities.Collection.Id), row.Id);
        SetProperty(collection, nameof(Entities.Collection.UserId), row.UserId);
        SetProperty(collection, nameof(Entities.Collection.Name), row.Name);
        SetProperty(collection, nameof(Entities.Collection.Slug), row.Slug);
        SetProperty(collection, nameof(Entities.Collection.Description), row.Description);
        SetProperty(collection, nameof(Entities.Collection.IsPublic), row.IsPublic == 1);
        SetProperty(collection, nameof(Entities.Collection.CreatedAt), ParseDate(row.CreatedAt));
        return collection;
    }

    private static Entities.Collection CreateEntity()
        => (Entities.Collection)Activator.CreateInstance(typeof(Entities.Collection), nonPublic: true)!;

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

    private sealed class CollectionRow
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long IsPublic { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    private sealed class CollectionListRow
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public int LinkCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
