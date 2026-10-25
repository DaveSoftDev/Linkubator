using Dapper;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

internal sealed class SqliteRepositoryTestDatabase : IAsyncDisposable
{
    private readonly string _databasePath;
    private readonly SqliteConnectionFactory _connectionFactory;

    private SqliteRepositoryTestDatabase(string databasePath)
    {
        _databasePath = databasePath;
        _connectionFactory = new SqliteConnectionFactory($"Data Source={databasePath}");
    }

    public string ConnectionString => $"Data Source={_databasePath}";

    public static async Task<SqliteRepositoryTestDatabase> Create()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteRepositoryTestDatabase database = new(databasePath);
        await new SqliteMigrationService(database._connectionFactory).ApplyPendingMigrations();
        await database.SeedUser(42, "owner@example.test", "owner");
        await database.SeedUser(99, "other@example.test", "other");
        return database;
    }

    public SqliteConnection OpenConnection()
        => _connectionFactory.CreateOpenConnection();

    public async Task<long> SeedCollection(long userId, string name, string slug, bool isPublic, string? description = null)
    {
        await using SqliteConnection connection = OpenConnection();
        long collectionId = await connection.ExecuteScalarAsync<long>(@"
            INSERT INTO Collection (UserId, Name, Slug, Description, IsPublic)
            VALUES (@UserId, @Name, @Slug, @Description, @IsPublic);
            SELECT last_insert_rowid();
        ", new
        {
            UserId = userId,
            Name = name,
            Slug = slug,
            Description = description,
            IsPublic = isPublic ? 1 : 0,
        });
        await connection.ExecuteAsync(
            "INSERT INTO CollectionSearch (rowid, UserId, Name, Description) VALUES (@Id, @UserId, @Name, @Description);",
            new
            {
                Id = collectionId,
                UserId = userId,
                Name = Linkubator.Infrastructure.Search.SearchTextNormalizer.Normalize(name),
                Description = Linkubator.Infrastructure.Search.SearchTextNormalizer.Normalize(description),
            });
        return collectionId;
    }

    public async Task<long> SeedLink(
        long userId,
        long collectionId,
        string url,
        string normalizedUrl,
        string? title,
        bool isPublic,
        SqliteTestLinkOptions? options = null)
    {
        SqliteTestLinkOptions linkOptions = options ?? new SqliteTestLinkOptions();
        await using SqliteConnection connection = OpenConnection();
        long linkId = await connection.ExecuteScalarAsync<long>(@"
            INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, Title, Description, Image, IsPublic, CreatedAt)
            VALUES (@UserId, @CollectionId, @UrlOriginal, @UrlNormalized, @Title, @Description, @Image, @IsPublic, @CreatedAt);
            SELECT last_insert_rowid();
        ", new
        {
            UserId = userId,
            CollectionId = collectionId,
            UrlOriginal = url,
            UrlNormalized = normalizedUrl,
            Title = title,
            Description = linkOptions.Description,
            Image = linkOptions.Image,
            IsPublic = isPublic ? 1 : 0,
            CreatedAt = linkOptions.CreatedAt ?? DateTime.UtcNow,
        });
        string? collectionName = await connection.ExecuteScalarAsync<string?>(
            "SELECT Name FROM Collection WHERE Id = @CollectionId AND UserId = @UserId;",
            new { CollectionId = collectionId, UserId = userId });
        await connection.ExecuteAsync(
            @"INSERT INTO LinkSearch (rowid, UserId, UrlOriginal, Title, Description, CollectionName)
              VALUES (@Id, @UserId, @UrlOriginal, @Title, @Description, @CollectionName);",
            new
            {
                Id = linkId,
                UserId = userId,
                UrlOriginal = Linkubator.Infrastructure.Search.SearchTextNormalizer.Normalize(url),
                Title = Linkubator.Infrastructure.Search.SearchTextNormalizer.Normalize(title),
                Description = Linkubator.Infrastructure.Search.SearchTextNormalizer.Normalize(linkOptions.Description),
                CollectionName = Linkubator.Infrastructure.Search.SearchTextNormalizer.Normalize(collectionName),
            });
        return linkId;
    }

    public async Task SeedCollectionRange(long userId, int count)
    {
        for (int index = 0; index < count; index++)
        {
            await SeedCollection(userId, $"Collection {index:D2}", $"collection-{index}", false);
        }
    }

    public async Task SeedLinkRange(long userId, long collectionId, int count, bool isPublic = false)
    {
        for (int index = 0; index < count; index++)
        {
            DateTime createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(index);
            await SeedLink(userId, collectionId, $"https://example.test/{index:D2}", $"https://example.test/{index:D2}", $"Link {index:D2}", isPublic, new SqliteTestLinkOptions(CreatedAt: createdAt));
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (string filePath in new[]
        {
            _databasePath,
            $"{_databasePath}-wal",
            $"{_databasePath}-shm"
        })
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException)
                {
                    // SQLite can release temporary database files after the connection is closed.
                }
            }
        }

        await ValueTask.CompletedTask;
    }

    private async Task SeedUser(long userId, string email, string alias)
    {
        await using SqliteConnection connection = OpenConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO User (Id, Email, Alias, Name, SecurityStamp)
            VALUES (@Id, @Email, @Alias, @Name, @SecurityStamp);
        ", new
        {
            Id = userId,
            Email = email,
            Alias = alias,
            Name = alias,
            SecurityStamp = Guid.NewGuid().ToString("N"),
        });
    }
}

internal sealed record SqliteTestLinkOptions(string? Description = null, string? Image = null, DateTime? CreatedAt = null);
