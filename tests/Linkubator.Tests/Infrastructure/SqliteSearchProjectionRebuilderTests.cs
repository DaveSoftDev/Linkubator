using Dapper;
using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteSearchProjectionRebuilderTests
{
    private const long OwnerUserId = 42;
    private const long OtherUserId = 99;

    [Fact]
    public async Task Rebuild_WhenProjectionsAreAltered_RestoresThemFromEntities()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Original collection", "original-collection", false);
        long linkId = await database.SeedLink(
            OwnerUserId,
            collectionId,
            "https://example.test/original",
            "https://example.test/original",
            "Original title",
            false,
            new SqliteTestLinkOptions(Description: "Original description"));

        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync("UPDATE Collection SET Name = 'Original e\u0301 collection' WHERE Id = @Id;", new { Id = collectionId });
        await connection.ExecuteAsync(
            "UPDATE CollectionSearch SET Name = 'corrupted collection' WHERE rowid = @Id; DELETE FROM LinkSearch WHERE rowid = @LinkId;",
            new { Id = collectionId, LinkId = linkId });
        ISqliteSearchProjectionRebuilder rebuilder = new SqliteSearchProjectionRebuilder(new SqliteConnectionFactory(database.ConnectionString));

        await rebuilder.Rebuild();

        Assert.Equal("Original é collection", await connection.ExecuteScalarAsync<string>(
            "SELECT Name FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = collectionId, UserId = OwnerUserId }));
        Assert.Equal("Original e\u0301 collection", await connection.ExecuteScalarAsync<string>(
            "SELECT Name FROM Collection WHERE Id = @Id;",
            new { Id = collectionId }));
        Assert.Equal("https://example.test/original|Original title|Original description|Original é collection", await connection.ExecuteScalarAsync<string>(
            "SELECT UrlOriginal || '|' || Title || '|' || Description || '|' || CollectionName FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = linkId, UserId = OwnerUserId }));
    }

    [Fact]
    public async Task Rebuild_WhenCalledRepeatedly_ReplacesStaleRowsForAllUsersAndEntities()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long ownerCollectionId = await database.SeedCollection(OwnerUserId, "Owner collection", "owner-collection", false);
        long otherCollectionId = await database.SeedCollection(OtherUserId, "Other collection", "other-collection", false);
        long ownerLinkId = await database.SeedLink(OwnerUserId, ownerCollectionId, "https://example.test/owner", "https://example.test/owner", "Owner link", false);
        long otherLinkId = await database.SeedLink(OtherUserId, otherCollectionId, "https://example.test/other", "https://example.test/other", "Other link", false);
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync(
            "DELETE FROM CollectionSearch WHERE rowid = @Id; DELETE FROM LinkSearch WHERE rowid = @LinkId; INSERT INTO CollectionSearch (rowid, UserId, Name) VALUES (999999, @UserId, 'Stale collection'); INSERT INTO LinkSearch (rowid, UserId, UrlOriginal, Title, Description, CollectionName) VALUES (999999, @UserId, '', 'Stale link', '', '');",
            new { Id = otherCollectionId, LinkId = otherLinkId, UserId = OwnerUserId });
        ISqliteSearchProjectionRebuilder rebuilder = new SqliteSearchProjectionRebuilder(new SqliteConnectionFactory(database.ConnectionString));

        await rebuilder.Rebuild();
        string firstSnapshot = await GetProjectionSnapshot(connection);
        await rebuilder.Rebuild();
        string secondSnapshot = await GetProjectionSnapshot(connection);

        Assert.Equal(firstSnapshot, secondSnapshot);
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CollectionSearch;"));
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM LinkSearch;"));
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;", new { Id = otherCollectionId, UserId = OtherUserId }));
                Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;", new { Id = otherLinkId, UserId = OtherUserId }));
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM LinkSearch WHERE rowid = @Id AND UserId = 42;", new { Id = ownerLinkId }));
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM LinkSearch WHERE rowid = 999999;"));
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CollectionSearch WHERE rowid = 999999;"));
    }

    [Fact]
    public async Task Rebuild_WhenOptionalLinkFieldsAreNull_PreservesNullProjectionValues()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Nullable collection", "nullable-collection", false);
        long linkId = await database.SeedLink(OwnerUserId, collectionId, "https://example.test/nullable", "https://example.test/nullable", null, false);
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync("UPDATE Link SET Description = NULL WHERE Id = @Id;", new { Id = linkId });
        ISqliteSearchProjectionRebuilder rebuilder = new SqliteSearchProjectionRebuilder(new SqliteConnectionFactory(database.ConnectionString));

        await rebuilder.Rebuild();

        Assert.Equal(1, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId AND Title IS NULL AND Description IS NULL;",
            new { Id = linkId, UserId = OwnerUserId }));
    }

    [Fact]
    public async Task Rebuild_WhenProjectedTextIsUnicodeDecomposed_NormalizesEveryFieldButPreservesEntityValues()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Initial collection", "initial-collection", false);
        long linkId = await database.SeedLink(OwnerUserId, collectionId, "https://example.test/initial", "https://example.test/initial", "Initial title", false);
        const string decomposedAccent = "e\u0301";
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync(
            "UPDATE Collection SET Name = @Value WHERE Id = @Id; UPDATE Link SET UrlOriginal = @Url, Title = @Title, Description = @Description WHERE Id = @Id;",
            new
            {
                Id = collectionId,
                Value = $"Collection {decomposedAccent}",
                Url = $"https://example.test/{decomposedAccent}",
                Title = $"Title {decomposedAccent}",
                Description = $"Description {decomposedAccent}",
            });
        ISqliteSearchProjectionRebuilder rebuilder = new SqliteSearchProjectionRebuilder(new SqliteConnectionFactory(database.ConnectionString));

        await rebuilder.Rebuild();

        Assert.Equal("Collection é", await connection.ExecuteScalarAsync<string>("SELECT Name FROM CollectionSearch WHERE rowid = @Id;", new { Id = collectionId }));
        Assert.Equal($"https://example.test/é|Title é|Description é|Collection é", await connection.ExecuteScalarAsync<string>(
            "SELECT UrlOriginal || '|' || Title || '|' || Description || '|' || CollectionName FROM LinkSearch WHERE rowid = @Id;",
            new { Id = linkId }));
        Assert.Equal($"Collection {decomposedAccent}", await connection.ExecuteScalarAsync<string>("SELECT Name FROM Collection WHERE Id = @Id;", new { Id = collectionId }));
        Assert.Equal($"https://example.test/{decomposedAccent}|Title {decomposedAccent}|Description {decomposedAccent}", await connection.ExecuteScalarAsync<string>(
            "SELECT UrlOriginal || '|' || Title || '|' || Description FROM Link WHERE Id = @Id;",
            new { Id = linkId }));
    }

    private static async Task<string> GetProjectionSnapshot(SqliteConnection connection)
    {
        IEnumerable<string> collectionRows = await connection.QueryAsync<string>(
            "SELECT rowid || ':' || UserId || ':' || Name FROM CollectionSearch ORDER BY rowid;");
        IEnumerable<string> linkRows = await connection.QueryAsync<string>(
            "SELECT rowid || ':' || UserId || ':' || UrlOriginal || ':' || COALESCE(Title, '') || ':' || COALESCE(Description, '') || ':' || CollectionName FROM LinkSearch ORDER BY rowid;");
        return string.Join(Environment.NewLine, collectionRows.Concat(linkRows));
    }

    [Fact]
    public async Task Rebuild_WhenLinkProjectionInsertFails_RollsBackBothProjections()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Original collection", "original-collection", false);
        long linkId = await database.SeedLink(
            OwnerUserId,
            collectionId,
            "https://example.test/original",
            "https://example.test/original",
            "Original title",
            false);
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync(
            "UPDATE CollectionSearch SET Name = 'sentinel collection' WHERE rowid = @Id; UPDATE LinkSearch SET Title = 'sentinel title' WHERE rowid = @LinkId; ALTER TABLE Link RENAME TO LinkUnavailable;",
                        new { Id = collectionId, LinkId = linkId });
        ISqliteSearchProjectionRebuilder rebuilder = new SqliteSearchProjectionRebuilder(new SqliteConnectionFactory(database.ConnectionString));

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(() => rebuilder.Rebuild());

        Assert.Contains("no such table: Link", exception.Message, StringComparison.Ordinal);
        Assert.Equal("sentinel collection", await connection.ExecuteScalarAsync<string>(
            "SELECT Name FROM CollectionSearch WHERE rowid = @Id;",
            new { Id = collectionId }));
        Assert.Equal("sentinel title", await connection.ExecuteScalarAsync<string>(
            "SELECT Title FROM LinkSearch WHERE rowid = @Id;",
            new { Id = linkId }));
    }
}
