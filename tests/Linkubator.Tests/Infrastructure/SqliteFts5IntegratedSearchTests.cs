using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Repositories;
using Linkubator.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteFts5IntegratedSearchTests
{
    [Theory]
    [InlineData("AND OR NOT NEAR")]
    [InlineData("AND-OR*NOT \"NEAR\"")]
    public async Task Search_WithOperatorWords_TreatsEveryTermAsRequiredText(string query)
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long matching = await database.SeedCollection(42, "AND OR NOT NEAR", "matching", false);
        long partial = await database.SeedCollection(42, "AND OR NOT", "partial", false);
        long linkId = await database.SeedLink(42, matching, "https://example.test/matching", "matching", "AND OR NOT NEAR", false);
        await database.SeedLink(42, partial, "https://example.test/partial", "partial", "AND OR NOT", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collections = new SqliteCollectionRepository(connection);
        ILinkRepository links = new SqliteLinkRepository(connection);

        var collectionResult = await collections.GetPageByUser(42, query, 1);
        var linkResult = await links.GetPageByUser(42, query, null, 1);

        Assert.Equal(new[] { matching }, collectionResult.Select(item => item.Id));
        Assert.Equal(new[] { linkId }, linkResult.Select(item => item.Id));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Search_WithUnicodeForms_NormalizesQueryAndStoredFields(bool storedDecomposed, bool queryDecomposed)
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        string stored = storedDecomposed ? "Cafe\u0301" : "Caf\u00e9";
        string query = queryDecomposed ? "Cafe\u0301" : "Caf\u00e9";
        long collectionId = await database.SeedCollection(42, stored, "unicode", false, stored);
        long linkId = await database.SeedLink(42, collectionId, "https://example.test/unicode", "unicode", stored, false);
        await database.SeedCollection(42, "Unrelated", "unrelated", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collections = new SqliteCollectionRepository(connection);
        ILinkRepository links = new SqliteLinkRepository(connection);

        var collectionResult = await collections.GetPageByUser(42, query, 1);
        var linkResult = await links.GetPageByUser(42, query, null, 1);

        Assert.Equal(new[] { collectionId }, collectionResult.Select(item => item.Id));
        Assert.Equal(new[] { linkId }, linkResult.Select(item => item.Id));
        Assert.Equal(stored, (await links.GetById(linkId, 42))!.Title);
        Assert.Equal(stored, (await collections.GetById(collectionId, 42))!.Name);
    }

    [Fact]
    public async Task Search_WithTermsDistributedAcrossLinkFields_RequiresAllFields()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "collectionword", "distributed", false);
        long matching = await database.SeedLink(42, collectionId, "https://example.test/urlword", "matching", "titleword", false,
            new SqliteTestLinkOptions(Description: "descriptionword"));
        await database.SeedLink(42, collectionId, "https://example.test/partial", "partial", "titleword", false,
            new SqliteTestLinkOptions(Description: "descriptionword"));
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository links = new SqliteLinkRepository(connection);

        var result = await links.GetPageByUser(42, "urlword titleword descriptionword collectionword", null, 1);

        Assert.Equal(new[] { matching }, result.Select(item => item.Id));
    }

    [Fact]
    public async Task Writes_WhenEntitiesChange_KeepBothSearchListingsCurrent()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        ILinkRepository linkRepository = new SqliteLinkRepository(connection);
        Collection collection = Collection.Create(42, "Old garden", null, false);
        await collectionRepository.Insert(collection);
        Link link = Link.Create(42, collection, "https://example.test/rose", "Petalword title", null, null);

        await linkRepository.Insert(link);

        Assert.Single(await linkRepository.GetPageByUser(42, "petalword", null, 1));
        Assert.Empty(await collectionRepository.GetPageByUser(42, "rose", 1));
        Assert.Single(await collectionRepository.GetPageByUser(42, "old", 1));
        Assert.Single(await linkRepository.GetPageByUser(42, "old", null, 1));

        collection.Rename("New garden");
        await collectionRepository.Update(collection);
        link.Edit("Updated title", null, null);
        await linkRepository.Update(link);

        Assert.Single(await collectionRepository.GetPageByUser(42, "new", 1));
        Assert.Empty(await collectionRepository.GetPageByUser(42, "old", 1));
        Assert.Single(await linkRepository.GetPageByUser(42, "new", null, 1));
        Assert.Empty(await linkRepository.GetPageByUser(42, "old", null, 1));
        Assert.Single(await linkRepository.GetPageByUser(42, "updated", null, 1));
        Assert.Empty(await linkRepository.GetPageByUser(42, "petalword", null, 1));

        await linkRepository.Delete(link.Id, 42);
        await collectionRepository.Delete(collection.Id, 42);

        Assert.Empty(await linkRepository.GetPageByUser(42, "new", null, 1));
        Assert.Empty(await collectionRepository.GetPageByUser(42, "new", 1));
    }

    [Fact]
    public async Task Update_WhenProjectionWriteFails_RollsBackEntityProjectionAndSearchResults()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Atomic garden", "atomic-garden", false);
        long linkId = await database.SeedLink(42, collectionId, "https://example.test/atomic", "https://example.test/atomic", "Priorword title", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);
        Link link = (await repository.GetById(linkId, 42))!;
        link.Edit("Changedword title", null, null);
        await connection.ExecuteAsync("BEGIN IMMEDIATE;");
        await connection.ExecuteAsync("DROP TABLE LinkSearch;");

        await Assert.ThrowsAsync<SqliteException>(() => repository.Update(link));
        await connection.ExecuteAsync("ROLLBACK;");

        Assert.Equal("Priorword title", (await repository.GetById(linkId, 42))!.Title);
        Assert.Equal("Priorword title", await connection.QuerySingleAsync<string>(
            "SELECT Title FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = linkId, UserId = 42 }));
        Assert.Single(await repository.GetPageByUser(42, "priorword", null, 1));
        Assert.Empty(await repository.GetPageByUser(42, "changedword", null, 1));
    }

    [Fact]
    public async Task Search_WhenProjectionOwnerDiffersOrTermMatchesOtherEntity_ExcludesTheResult()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long ownCollectionId = await database.SeedCollection(42, "Canopy collection", "canopy-collection", false);
        long foreignCollectionId = await database.SeedCollection(99, "Foreign canopy", "foreign-canopy", false);
        long ownLinkId = await database.SeedLink(42, ownCollectionId, "https://example.test/own", "https://example.test/own", "Cedar link", false);
        long foreignLinkId = await database.SeedLink(99, foreignCollectionId, "https://example.test/foreign", "https://example.test/foreign", "Cedar link", false);
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync("UPDATE LinkSearch SET UserId = 42 WHERE rowid = @Id;", new { Id = foreignLinkId });
        ILinkRepository linkRepository = new SqliteLinkRepository(connection);
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> links = await linkRepository.GetPageByUser(42, "cedar", null, 1);
        IReadOnlyCollection<Linkubator.Application.Dtos.Models.CollectionDto> collections = await collectionRepository.GetPageByUser(42, "canopy", 1);

        Assert.Equal(new[] { ownLinkId }, links.Select(link => link.Id));
        Assert.Equal(new[] { ownCollectionId }, collections.Select(collection => collection.Id));
        Assert.Empty(await collectionRepository.GetPageByUser(42, "cedar", 1));
        Assert.Empty(await linkRepository.GetPageByUser(99, "cedar", null, 1));
    }

    [Fact]
    public async Task Search_WhenTextUsesEquivalentUnicodeForms_MatchesAndPreservesEntityValues()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        ILinkRepository linkRepository = new SqliteLinkRepository(connection);
        const string decomposedCafe = "Cafe\u0301";
        Collection collection = Collection.Create(42, $"{decomposedCafe} garden", null, false);
        await collectionRepository.Insert(collection);
        Link link = Link.Create(42, collection, "https://example.test/cafe", $"{decomposedCafe} guide", null, null);

        await linkRepository.Insert(link);

        Assert.Single(await collectionRepository.GetPageByUser(42, "Café", 1));
        Assert.Single(await linkRepository.GetPageByUser(42, "Café", null, 1));
        Assert.Equal($"{decomposedCafe} garden", (await collectionRepository.GetById(collection.Id, 42))!.Name);
        Assert.Equal($"{decomposedCafe} guide", (await linkRepository.GetById(link.Id, 42))!.Title);
        Assert.Equal("Café garden", await connection.QuerySingleAsync<string>(
            "SELECT Name FROM CollectionSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = collection.Id, UserId = 42 }));
        Assert.Equal("Café guide", await connection.QuerySingleAsync<string>(
            "SELECT Title FROM LinkSearch WHERE rowid = @Id AND UserId = @UserId;",
            new { Id = link.Id, UserId = 42 }));
    }
}
