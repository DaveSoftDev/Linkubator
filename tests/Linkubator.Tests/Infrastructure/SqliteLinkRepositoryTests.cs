using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Repositories;
using Linkubator.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteLinkRepositoryTests
{
    [Fact]
    public async Task GetById_WhenLinkBelongsToDifferentUser_ReturnsNull()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(99, "Other", "other", false);
        long linkId = await database.SeedLink(99, collectionId, "https://example.test/a", "https://example.test/a", "Other", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        Link? result = await repository.GetById(linkId, 42);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNormalizedUrl_WhenUrlBelongsToAnotherUser_ReturnsNull()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(99, "Other", "other", false);
        await database.SeedLink(99, collectionId, "https://example.test/a", "https://example.test/a", "Other", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        Link? result = await repository.GetByNormalizedUrl(42, "https://example.test/a");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNormalizedUrl_WhenUrlBelongsToUser_ReturnsLink()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        await database.SeedLink(42, collectionId, "https://example.test/a", "https://example.test/a", "Own", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        Link? result = await repository.GetByNormalizedUrl(42, "https://example.test/a");

        Assert.Equal("Own", result!.Title);
    }

    [Fact]
    public async Task Insert_ThenGetById_RoundTripsLinkProperties()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Private", "private", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        ILinkRepository linkRepository = new SqliteLinkRepository(connection);
        Collection collection = (await collectionRepository.GetById(collectionId, 42))!;
        Link link = Link.Create(42, collection, "https://example.test/path", "Title", "Description", null);

        await linkRepository.Insert(link);
        Link? result = await linkRepository.GetById(link.Id, 42);

        Assert.NotNull(result);
        Assert.Equal(link.Id, result.Id);
        Assert.Equal(42, result.UserId);
        Assert.Equal(collectionId, result.CollectionId);
        Assert.Equal("https://example.test/path", result.UrlOriginal);
        Assert.Equal(link.UrlNormalized, result.UrlNormalized);
        Assert.Equal("Title", result.Title);
        Assert.Equal("Description", result.Description);
        Assert.False(result.IsPublic);
        Assert.Equal(
            "https://example.test/path|Title|Description|Private",
            await connection.QuerySingleAsync<string>(
                "SELECT UrlOriginal || '|' || Title || '|' || Description || '|' || CollectionName FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
                new { Id = link.Id, UserId = 42 }));
    }

    [Fact]
    public async Task Update_WhenLinkBelongsToAnotherUser_DoesNotChangeIt()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(99, "Other", "other", false);
        long linkId = await database.SeedLink(99, collectionId, "https://example.test/a", "https://example.test/a", "Original", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);
        Link link = (await repository.GetById(linkId, 99))!;
        link.Edit("Changed", null, null);

        await repository.Update(link);
        Link? resultForOtherUser = await repository.GetById(linkId, 99);

        Assert.Equal("Changed", resultForOtherUser!.Title);
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotOwnLink_LeavesLinkUnchanged()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(99, "Other", "other", false);
        long linkId = await database.SeedLink(99, collectionId, "https://example.test/a", "https://example.test/a", "Other", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        await repository.Delete(linkId, 42);
        Link? result = await repository.GetById(linkId, 99);

        Assert.NotNull(result);
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = linkId, UserId = 42 }));
    }

    [Fact]
    public async Task Update_WhenProjectionWriteFails_RollsBackLinkAndProjection()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        long linkId = await database.SeedLink(42, collectionId, "https://example.test/a", "https://example.test/a", "Original", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);
        Link link = (await repository.GetById(linkId, 42))!;
        link.Edit("Changed", null, null);
        await connection.ExecuteAsync("BEGIN IMMEDIATE;");
        await connection.ExecuteAsync("DROP TABLE LinkSearch;");

        await Assert.ThrowsAsync<SqliteException>(() => repository.Update(link));
        await connection.ExecuteAsync("ROLLBACK;");

        Assert.Equal("Original", (await repository.GetById(linkId, 42))!.Title);
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'LinkSearch';"));
        Assert.Equal("Original", await connection.QuerySingleAsync<string>(
            "SELECT Title FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = linkId, UserId = 42 }));
    }

    [Fact]
    public async Task Insert_WhenTextIsDecomposed_NormalizesOnlyProjection()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Cafe\u0301", "cafe", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        ILinkRepository repository = new SqliteLinkRepository(connection);
        Collection collection = (await collectionRepository.GetById(collectionId, 42))!;
        Link link = Link.Create(42, collection, "https://example.test/path", "Cafe\u0301", null, null);

        await repository.Insert(link);

        Assert.Equal("Cafe\u0301", link.Title);
        Assert.Equal("Café", await connection.QuerySingleAsync<string>(
            "SELECT Title FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = link.Id, UserId = 42 }));
        Assert.Equal("Café", await connection.QuerySingleAsync<string>(
            "SELECT CollectionName FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = link.Id, UserId = 42 }));
    }

    [Fact]
    public async Task Delete_WhenUserOwnsLink_RemovesSearchProjection()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        ILinkRepository repository = new SqliteLinkRepository(connection);
        Collection collection = (await collectionRepository.GetById(collectionId, 42))!;
        Link link = Link.Create(42, collection, "https://example.test/path", "Title", null, null);
        await repository.Insert(link);

        await repository.Delete(link.Id, 42);

        Assert.Equal(0, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = link.Id, UserId = 42 }));
    }

    [Fact]
    public async Task Update_WhenLinkMovesToDifferentCollection_UpdatesCollectionIdAndPrivacy()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long sourceCollectionId = await database.SeedCollection(42, "Source", "source", true);
        long targetCollectionId = await database.SeedCollection(42, "Target", "target", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        Collection source = (await collectionRepository.GetById(sourceCollectionId, 42))!;
        Collection target = (await collectionRepository.GetById(targetCollectionId, 42))!;
        Link link = Link.Create(42, source, "https://example.test/a", "Link", null, null, true);
        await repository.Insert(link);
        link.MoveToCollection(target);

        await repository.Update(link);
        Link? result = await repository.GetById(link.Id, 42);

        Assert.Equal(targetCollectionId, result!.CollectionId);
        Assert.False(result.IsPublic);
        Assert.Equal(
            "Target",
            await connection.QuerySingleAsync<string>(
                "SELECT CollectionName FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
                new { Id = link.Id, UserId = 42 }));
    }

    [Fact]
    public async Task Update_WhenLinkBelongsToUser_PersistsEditableFields()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        long linkId = await database.SeedLink(42, collectionId, "https://example.test/a", "https://example.test/a", "Original", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);
        Link link = (await repository.GetById(linkId, 42))!;
        link.Edit("Updated", "New description", null);

        await repository.Update(link);
        Link? result = await repository.GetById(linkId, 42);

        Assert.Equal("Updated", result!.Title);
        Assert.Equal("New description", result.Description);
        Assert.Equal(
            "Updated|New description",
            await connection.QuerySingleAsync<string>(
                "SELECT Title || '|' || Description FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
                new { Id = linkId, UserId = 42 }));
    }

    [Fact]
    public async Task GetPageByUser_WhenQueryIsBlankAndCollectionSpecified_FiltersAndReturnsPage()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long firstCollectionId = await database.SeedCollection(42, "First", "first", false);
        long secondCollectionId = await database.SeedCollection(42, "Second", "second", false);
        await database.SeedLink(42, firstCollectionId, "https://example.test/one", "https://example.test/one", "First link", false);
        await database.SeedLink(42, secondCollectionId, "https://example.test/two", "https://example.test/two", "Second link", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, null, firstCollectionId, 1);

        Assert.Equal(new[] { "https://example.test/one" }, result.Select(item => item.UrlOriginal));
    }

    [Theory]
    [InlineData("term")]
    [InlineData(" term ")]
    public async Task GetPageByUser_WhenQueryHasTerms_ThrowsBeforeS6(string query)
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetPageByUser(42, query, null, 1));

        Assert.Equal("Search queries are not supported before S6.", exception.Message);
    }

    [Fact]
    public async Task GetPageCountByUser_WhenCollectionIsSpecified_CountsOnlyItsLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long firstCollectionId = await database.SeedCollection(42, "First", "first", false);
        long secondCollectionId = await database.SeedCollection(42, "Second", "second", false);
        await database.SeedLink(42, firstCollectionId, "https://example.test/one", "https://example.test/one", "One", false);
        await database.SeedLink(42, secondCollectionId, "https://example.test/two", "https://example.test/two", "Two", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        int result = await repository.GetPageCountByUser(42, string.Empty, firstCollectionId);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetPageByUser_WhenPageIsSecond_ReturnsRemainingLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        await database.SeedLinkRange(42, collectionId, 16);

        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, null, collectionId, 2);

        Assert.Single(result);
        Assert.Equal("https://example.test/00", result.Single().UrlOriginal);
    }

    [Fact]
    public async Task GetPageCountByUser_WhenQueryHasTerms_ThrowsBeforeS6()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetPageCountByUser(42, "term", null));

        Assert.Equal("Search queries are not supported before S6.", exception.Message);
    }
}
