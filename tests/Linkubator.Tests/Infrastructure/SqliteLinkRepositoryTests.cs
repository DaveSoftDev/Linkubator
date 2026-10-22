using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Repositories;
using Linkubator.Domain.Entities;
using DomainEnums = Linkubator.Domain.Enums;
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
    public async Task Dapper_WhenMappingLink_UsesPrivateConstructorAndSetters()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Private", "private", false);
        DateTime createdAt = new(2026, 3, 7, 11, 13, 17, DateTimeKind.Utc);
        long linkId = await database.SeedLink(
            42,
            collectionId,
            "https://example.test/path",
            "https://example.test/normalized",
            "Stored title",
            true,
            new SqliteTestLinkOptions("Stored description", "https://example.test/image.png", createdAt));
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync(
            "UPDATE Link SET ScrapingStatus = @ScrapingStatus WHERE Id = @Id;",
            new { Id = linkId, ScrapingStatus = (int)DomainEnums.ScrapingStatus.Completed });

        ILinkRepository repository = new SqliteLinkRepository(connection);
        Link result = (await repository.GetById(linkId, 42))!;

        Assert.Equal(linkId, result.Id);
        Assert.Equal(42, result.UserId);
        Assert.Equal(collectionId, result.CollectionId);
        Assert.Same(result, Assert.Single(result.Collection.Links));
        Assert.Equal("https://example.test/path", result.UrlOriginal);
        Assert.Equal("https://example.test/normalized", result.UrlNormalized);
        Assert.Equal("Stored title", result.Title);
        Assert.Equal("Stored description", result.Description);
        Assert.Equal("https://example.test/image.png", result.Image);
        Assert.True(result.IsPublic);
        Assert.Equal(DomainEnums.ScrapingStatus.Completed, result.ScrapingStatus);
        Assert.Equal(createdAt, result.CreatedAt);
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
        Assert.Equal("Private", result.Collection.Name);
        Assert.Same(result, Assert.Single(result.Collection.Links));
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
    public async Task Insert_WhenProjectionWriteFails_DoesNotAssignId()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        await using SqliteConnection connection = database.OpenConnection();
        ICollectionRepository collectionRepository = new SqliteCollectionRepository(connection);
        ILinkRepository repository = new SqliteLinkRepository(connection);
        Collection collection = (await collectionRepository.GetById(collectionId, 42))!;
        Link link = Link.Create(42, collection, "https://example.test/path", "Title", null, null);
        await connection.ExecuteAsync("BEGIN IMMEDIATE;");
        await connection.ExecuteAsync("DROP TABLE LinkSearch;");

        await Assert.ThrowsAsync<SqliteException>(() => repository.Insert(link));

        Assert.Equal(0, link.Id);
        await connection.ExecuteAsync("ROLLBACK;");
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link WHERE UrlOriginal = @Url;", new { Url = link.UrlOriginal }));
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

        Assert.Equal("Target", link.Collection.Name);
        Assert.Equal(targetCollectionId, result!.CollectionId);
        Assert.Equal("Target", result.Collection.Name);
        Assert.Same(result, Assert.Single(result.Collection.Links));
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPageByUser_WhenPageIsLessThanOne_ThrowsArgumentOutOfRangeException(int page)
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetPageByUser(42, null, null, page));
    }

    [Fact]
    public async Task GetPageByUser_WhenPageIsIntMaxValue_ReturnsEmptyPageWithoutOverflow()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, null, null, int.MaxValue);

        Assert.Empty(result);
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

    [Fact]
    public async Task GetPageByUser_WhenLinkHasEveryDtoValue_ReturnsCompleteDto()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        DateTime createdAt = new(2026, 4, 8, 10, 12, 14, DateTimeKind.Utc);
        long linkId = await database.SeedLink(
            42,
            collectionId,
            "https://example.test/path",
            "https://example.test/normalized",
            "Stored title",
            true,
            new SqliteTestLinkOptions("Stored description", "https://example.test/image.png", createdAt));
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, null, collectionId, 1);

        Assert.Equal(
            new Linkubator.Application.Dtos.Models.LinkDto(
                linkId,
                collectionId,
                "https://example.test/path",
                "https://example.test/normalized",
                "Stored title",
                "Stored description",
                "https://example.test/image.png",
                true,
                createdAt),
            Assert.Single(result));
    }

    [Fact]
    public async Task GetPageByUser_WhenProjectionUserDoesNotMatchLinkOwner_ExcludesTheResult()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long foreignCollectionId = await database.SeedCollection(99, "Foreign", "foreign", false);
        await database.SeedLink(99, foreignCollectionId, "https://example.test/foreign", "https://example.test/foreign", "needle", false);
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync("UPDATE LinkSearch SET UserId = 42 WHERE UserId = 99;");
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, "needle", null, 1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPageByUser_WhenQueryMatchesLinks_ReturnsOnlyOwnedMatchesInRankOrder()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long ownCollectionId = await database.SeedCollection(42, "Own", "own", false);
        long otherCollectionId = await database.SeedCollection(99, "Other", "other", false);
        await database.SeedLink(42, ownCollectionId, "https://example.test/query", "https://example.test/query", "query query", false);
        await database.SeedLink(42, ownCollectionId, "https://example.test/other", "https://example.test/other", "query", false);
        await database.SeedLink(99, otherCollectionId, "https://example.test/foreign", "https://example.test/foreign", "query query query", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, "query", null, 1);

        Assert.Equal(new[] { "https://example.test/query", "https://example.test/other" }, result.Select(item => item.UrlOriginal));
    }


    [Fact]
    public async Task GetPageByUser_WhenRankAndCreatedAtAreEqual_UsesIdAsFinalTieBreaker()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        DateTime createdAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await database.SeedLink(42, collectionId, "https://example.test/first", "https://example.test/first", "needle", false, new SqliteTestLinkOptions(CreatedAt: createdAt));
        await database.SeedLink(42, collectionId, "https://example.test/second", "https://example.test/second", "needle", false, new SqliteTestLinkOptions(CreatedAt: createdAt));
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, "needle", null, 1);

        Assert.Equal(new[] { "https://example.test/second", "https://example.test/first" }, result.Select(item => item.UrlOriginal));
    }

    [Fact]
    public async Task GetPageByUser_WhenQueryHasMultipleTermsAndAccents_RequiresAllNormalizedTerms()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        await database.SeedLink(42, collectionId, "https://example.test/guia", "https://example.test/guia", "cancio\u0301n guia", false);
        await database.SeedLink(42, collectionId, "https://example.test/other", "https://example.test/other", "canción distinta", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, "cancion guia", null, 1);

        Assert.Equal(new[] { "https://example.test/guia" }, result.Select(item => item.UrlOriginal));
    }

    [Fact]
    public async Task GetPageByUser_WhenQueryHasNoTerms_UsesCollectionFilterAndUnsearchedOrder()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        long otherCollectionId = await database.SeedCollection(42, "Other", "other", false);
        await database.SeedLink(42, collectionId, "https://example.test/earlier", "https://example.test/earlier", "earlier", false, new SqliteTestLinkOptions(CreatedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        await database.SeedLink(42, collectionId, "https://example.test/later", "https://example.test/later", "later", false, new SqliteTestLinkOptions(CreatedAt: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)));
        await database.SeedLink(42, otherCollectionId, "https://example.test/unfiltered", "https://example.test/unfiltered", "unfiltered", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, "---", collectionId, 1);

        Assert.Equal(new[] { "https://example.test/later", "https://example.test/earlier" }, result.Select(item => item.UrlOriginal));
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

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> result = await repository.GetPageByUser(42, "link", collectionId, 2);

        Assert.Single(result);
        Assert.Equal("https://example.test/00", result.Single().UrlOriginal);
    }

    [Fact]
    public async Task GetPageCountByUser_WhenQueryHasTerms_CountsOnlyMatchesAndCollectionFilter()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        long otherCollectionId = await database.SeedCollection(42, "Other", "other", false);
        long foreignCollectionId = await database.SeedCollection(99, "Foreign", "foreign", false);
        await database.SeedLink(42, collectionId, "https://example.test/match-one", "https://example.test/match-one", "needle", false);
        await database.SeedLink(42, collectionId, "https://example.test/match-two", "https://example.test/match-two", "needle", false);
        await database.SeedLink(42, collectionId, "https://example.test/no-match", "https://example.test/no-match", "different", false);
        await database.SeedLink(42, otherCollectionId, "https://example.test/other-collection", "https://example.test/other-collection", "needle", false);
        await database.SeedLink(99, foreignCollectionId, "https://example.test/foreign-match", "https://example.test/foreign-match", "needle", false);
        await using SqliteConnection connection = database.OpenConnection();
        ILinkRepository repository = new SqliteLinkRepository(connection);

        int result = await repository.GetPageCountByUser(42, "needle", collectionId);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetPageCountByUser_WhenQueryHasTerms_CountsMatchingPages()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        await database.SeedLinkRange(42, collectionId, 16);
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync("UPDATE LinkSearch SET Title = 'needle' WHERE UserId = 42;");
        ILinkRepository repository = new SqliteLinkRepository(connection);

        int result = await repository.GetPageCountByUser(42, "needle", collectionId);

        Assert.Equal(2, result);
    }
}
