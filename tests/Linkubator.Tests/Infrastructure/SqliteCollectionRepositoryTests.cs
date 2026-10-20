using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Repositories;
using Linkubator.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteCollectionRepositoryTests
{
    [Fact]
    public async Task GetById_WhenCollectionBelongsToDifferentUser_ReturnsNull()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(99, "Other", "other", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        Collection? result = await repository.GetById(collectionId, 42);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySlug_WhenSlugBelongsToDifferentUser_ReturnsNull()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollection(99, "Other", "shared", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        Collection? result = await repository.GetBySlug(42, "shared");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByUser_ReturnsOnlyCollectionsOwnedByRequestedUser()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollection(42, "Mine", "mine", false);
        await database.SeedCollection(99, "Other", "other", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        IReadOnlyCollection<Collection> result = await repository.GetAllByUser(42);

        Assert.Equal(new[] { "Mine" }, result.Select(collection => collection.Name));
    }

    [Fact]
    public async Task Insert_ThenGetById_RoundTripsCollectionProperties()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);
        Collection collection = Collection.Create(42, "Árbol", "Description", true);

        await repository.Insert(collection);
        Collection? result = await repository.GetById(collection.Id, 42);

        Assert.NotNull(result);
        Assert.Equal(collection.Id, result.Id);
        Assert.Equal(42, result.UserId);
        Assert.Equal("Árbol", result.Name);
        Assert.Equal("arbol", result.Slug);
        Assert.Equal("Description", result.Description);
        Assert.True(result.IsPublic);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.Equal("Árbol", await connection.QuerySingleAsync<string>(
            "SELECT Name FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = collection.Id, UserId = 42 }));
    }

    [Fact]
    public async Task Update_WhenCollectionBelongsToAnotherUser_DoesNotChangeIt()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(99, "Original", "original", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);
        Collection foreignCollection = (await repository.GetById(collectionId, 99))!;
        foreignCollection.Rename("Changed");

        await repository.Update(foreignCollection);
        Collection? resultForOwner = await repository.GetById(collectionId, 42);
        Collection? resultForUser = await repository.GetById(collectionId, 99);

        Assert.Null(resultForOwner);
        Assert.Equal("Changed", resultForUser!.Name);
    }

    [Fact]
    public async Task Update_WhenCollectionBelongsToUser_PersistsChanges()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Original", "original", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);
        Collection collection = (await repository.GetById(collectionId, 42))!;
        collection.Rename("Renamed");
        collection.ChangeDescription("Updated description");

        await repository.Update(collection);
        Collection? result = await repository.GetById(collectionId, 42);

        Assert.Equal("Renamed", result!.Name);
        Assert.Equal("renamed", result.Slug);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("Renamed", await connection.QuerySingleAsync<string>(
            "SELECT Name FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = collectionId, UserId = 42 }));
    }

    [Fact]
    public async Task Update_WhenCollectionIsRenamed_UpdatesItsProjectionAndOnlyItsLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        ILinkRepository linkRepository = new SqliteLinkRepository(connection);
        Collection renamedCollection = Collection.Create(42, "Original", null, false);
        Collection otherCollection = Collection.Create(42, "Other", null, false);
        await collectionRepository.Insert(renamedCollection);
        await collectionRepository.Insert(otherCollection);
        Link renamedLink = Link.Create(42, renamedCollection, "https://example.test/one", "One", null, null);
        Link unchangedLink = Link.Create(42, otherCollection, "https://example.test/two", "Two", null, null);
        await linkRepository.Insert(renamedLink);
        await linkRepository.Insert(unchangedLink);
        renamedCollection.Rename("Renamed");

        await collectionRepository.Update(renamedCollection);

        Assert.Equal("Renamed", await connection.QuerySingleAsync<string>(
            "SELECT Name FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = renamedCollection.Id, UserId = 42 }));
        Assert.Equal("Renamed", await connection.QuerySingleAsync<string>(
            "SELECT CollectionName FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = renamedLink.Id, UserId = 42 }));
        Assert.Equal("Other", await connection.QuerySingleAsync<string>(
            "SELECT CollectionName FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = unchangedLink.Id, UserId = 42 }));
    }

    [Fact]
    public async Task Update_WhenTransactionRollsBack_RestoresCollectionAndProjections()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        ILinkRepository linkRepository = new SqliteLinkRepository(connection);
        Collection collection = Collection.Create(42, "Original", null, false);
        await collectionRepository.Insert(collection);
        Link link = Link.Create(42, collection, "https://example.test/rollback", "Rollback", null, null);
        await linkRepository.Insert(link);
        collection.Rename("Renamed");
        await connection.ExecuteAsync("BEGIN IMMEDIATE;");
        await connection.ExecuteAsync("DROP TABLE LinkSearch;");

        await Assert.ThrowsAsync<SqliteException>(() => collectionRepository.Update(collection));
        await connection.ExecuteAsync("ROLLBACK;");

        Assert.Equal("Original", (await collectionRepository.GetById(collection.Id, 42))!.Name);
        Assert.Equal("Original", await connection.QuerySingleAsync<string>(
            "SELECT Name FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = collection.Id, UserId = 42 }));
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'LinkSearch';"));
        Assert.Equal("Original", await connection.QuerySingleAsync<string>(
            "SELECT CollectionName FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = link.Id, UserId = 42 }));
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotOwnCollection_LeavesCollectionUnchanged()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(99, "Other", "other", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        await repository.Delete(collectionId, 42);
        Collection? result = await repository.GetById(collectionId, 99);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Delete_WhenUserOwnsCollection_RemovesCollection()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        await repository.Delete(collectionId, 42);
        Collection? result = await repository.GetById(collectionId, 42);

        Assert.Null(result);
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = collectionId, UserId = 42 }));
    }

    [Fact]
    public async Task GetPageByUser_FiltersCollectionsByUserAndCalculatesLinkCounts()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long ownCollectionId = await database.SeedCollection(42, "Mine", "mine", false);
        long otherCollectionId = await database.SeedCollection(99, "Other", "other", false);
        await database.SeedLink(42, ownCollectionId, "https://example.test/own", "https://example.test/own", "Own", false);
        await database.SeedLink(99, otherCollectionId, "https://example.test/other", "https://example.test/other", "Other", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.CollectionDto> result = await repository.GetPageByUser(42, null, 1);

        Assert.Equal("Mine", result.Single().Name);
        Assert.Equal(1, result.Single().LinkCount);
    }

    [Fact]
    public async Task GetPageByUser_WhenQueryIsBlank_ReturnsAlphabeticalPageUsingSpanishCollation()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollection(42, "barranco", "barranco", false);
        await database.SeedCollection(42, "Árbol", "arbol", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.CollectionDto> result = await repository.GetPageByUser(42, "  ", 1);

        Assert.Equal(new[] { "Árbol", "barranco" }, result.Select(item => item.Name));
    }

    [Theory]
    [InlineData("tree")]
    [InlineData(" tree ")]
    public async Task GetPageByUser_WhenQueryHasTerms_ThrowsBeforeS6(string query)
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetPageByUser(42, query, 1));

        Assert.Equal("Search queries are not supported before S6.", exception.Message);
    }

    [Fact]
    public async Task GetPageCountByUser_WhenUserHasNoCollections_ReturnsZero()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        int result = await repository.GetPageCountByUser(42, null);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetPageCountByUser_WhenCollectionsExist_ReturnsNumberOfPages()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollectionRange(42, 16);

        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        int result = await repository.GetPageCountByUser(42, string.Empty);

        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetPageByUser_WhenPageIsSecond_ReturnsRemainingCollections()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollectionRange(42, 16);

        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.CollectionDto> result = await repository.GetPageByUser(42, null, 2);

        Assert.Single(result);
        Assert.Equal("Collection 15", result.Single().Name);
    }

    [Fact]
    public async Task GetPageCountByUser_WhenQueryHasTerms_ThrowsBeforeS6()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository repository = new SqliteCollectionRepository(connection);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetPageCountByUser(42, "term"));

        Assert.Equal("Search queries are not supported before S6.", exception.Message);
    }
}
