using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Microsoft.Data.Sqlite;
using Entities = Linkubator.Domain.Entities;

namespace Linkubator.Infrastructure.Repositories;

public sealed class SqliteCollectionRepository : ICollectionRepository
{
    private const int PageSize = 16;
    private readonly SqliteConnection _connection;

    public SqliteCollectionRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<Entities.Collection?> GetById(long id, long userId, CancellationToken cancellationToken = default)
    {
        return await _connection.QuerySingleOrDefaultAsync<Entities.Collection>(new CommandDefinition(
            @"SELECT Id, UserId, Name, Slug, Description, IsPublic, CreatedAt
              FROM Collection
              WHERE Id = @Id AND UserId = @UserId;",
            new { Id = id, UserId = userId },
            cancellationToken: cancellationToken));
    }

    public async Task<Entities.Collection?> GetBySlug(long userId, string slug, CancellationToken cancellationToken = default)
    {
        return await _connection.QuerySingleOrDefaultAsync<Entities.Collection>(new CommandDefinition(
            @"SELECT Id, UserId, Name, Slug, Description, IsPublic, CreatedAt
              FROM Collection
              WHERE UserId = @UserId AND Slug = @Slug;",
            new { UserId = userId, Slug = slug },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<Entities.Collection>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
    {
        IEnumerable<Entities.Collection> collections = await _connection.QueryAsync<Entities.Collection>(new CommandDefinition(
            @"SELECT Id, Name, Slug, IsPublic
              FROM Collection
              WHERE UserId = @UserId
              ORDER BY Name COLLATE LINKUBATOR_ES_ES ASC, Id ASC;",
            new { UserId = userId },
            cancellationToken: cancellationToken));

        return collections.ToList();
    }

    public async Task<IReadOnlyCollection<CollectionDto>> GetAllByUserWithLinkCounts(long userId, CancellationToken cancellationToken = default)
    {
        IEnumerable<CollectionDto> collections = await _connection.QueryAsync<CollectionDto>(new CommandDefinition(
            @"SELECT c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt,
                       COUNT(l.Id) AS LinkCount
                FROM Collection c
                LEFT JOIN Link l ON l.UserId = c.UserId AND l.CollectionId = c.Id
                WHERE c.UserId = @UserId
                GROUP BY c.Id, c.Name, c.Slug, c.Description, c.IsPublic, c.CreatedAt
                ORDER BY c.Name COLLATE LINKUBATOR_ES_ES ASC, c.Id ASC;",
            new { UserId = userId },
            cancellationToken: cancellationToken));

        return collections.ToList();
    }

    public async Task<IReadOnlyCollection<CollectionDto>> GetPageByUser(long userId, string? query, int page, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);

        string? searchQuery = Search.SearchQueryBuilder.Build(Search.SearchTextNormalizer.Normalize(query));
        long offset = ((long)page - 1) * PageSize;
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
        IEnumerable<CollectionDto> collections = await _connection.QueryAsync<CollectionDto>(new CommandDefinition(
            sql,
            new { UserId = userId, SearchQuery = searchQuery, Limit = PageSize, Offset = offset },
            cancellationToken: cancellationToken));

        return collections.ToList();
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

        collection.SetId(id);

        await _connection.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO CollectionSearch (rowid, UserId, Name, Description)
              VALUES (@Id, @UserId, @Name, @Description);",
            new
            {
                Id = id,
                UserId = collection.UserId,
                Name = Search.SearchTextNormalizer.Normalize(collection.Name),
                Description = Search.SearchTextNormalizer.Normalize(collection.Description),
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
                @"INSERT INTO CollectionSearch (rowid, UserId, Name, Description)
                  VALUES (@Id, @UserId, @Name, @Description);",
                new
                {
                    Id = collection.Id,
                    UserId = collection.UserId,
                    Name = Search.SearchTextNormalizer.Normalize(collection.Name),
                    Description = Search.SearchTextNormalizer.Normalize(collection.Description),
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

}
