using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqlitePublicReadRepositoryTests
{
    [Fact]
    public async Task GetUserPage_WhenUserHasPublicAndPrivateData_ExposesOnlyPublishedRows()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long publicCollectionId = await database.SeedCollection(42, "Public", "public", true, "Public description");
        long privateCollectionId = await database.SeedCollection(42, "Private", "private", false);
        await database.SeedLink(42, publicCollectionId, "https://example.test/public", "https://example.test/public", "Public link", true);
        await database.SeedLink(42, publicCollectionId, "https://example.test/unpublished", "https://example.test/unpublished", "Unpublished link", false);
        await database.SeedLink(42, privateCollectionId, "https://example.test/private", "https://example.test/private", "Private link", false);
        await database.SeedLink(42, privateCollectionId, "https://example.test/private-collection", "https://example.test/private-collection", "Hidden link", true);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicUserDto? result = await repository.GetUserPage("owner");

        Assert.NotNull(result);
        Assert.Equal(new[] { "public" }, result.PublicCollections.Select(item => item.Slug));
        Assert.Equal(new[] { "https://example.test/public" }, result.RecentLinks.Select(item => item.UrlOriginal));
    }

    [Fact]
    public async Task GetUserPage_WhenPublicCollectionHasNoPublicLinks_DoesNotIncludeCollection()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollection(42, "No links", "no-links", true);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicUserDto? result = await repository.GetUserPage("owner");

        Assert.Empty(result!.PublicCollections);
    }

    [Fact]
    public async Task GetUserPage_WhenUserHasMoreThanFivePublicCollections_ReturnsFiveMostRecentInOrder()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        DateTime baseDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long oldestId = await database.SeedCollection(42, "Oldest", "oldest", true, createdAt: baseDate);
        long oldestIncludedId = await database.SeedCollection(42, "Oldest included", "oldest-included", true, createdAt: baseDate.AddDays(1));
        long firstTieId = await database.SeedCollection(42, "First tie", "first-tie", true, createdAt: baseDate.AddDays(2));
        long secondTieId = await database.SeedCollection(42, "Second tie", "second-tie", true, createdAt: baseDate.AddDays(2));
        long newerId = await database.SeedCollection(42, "Newer", "newer", true, createdAt: baseDate.AddDays(3));
        long newestId = await database.SeedCollection(42, "Newest", "newest", true, createdAt: baseDate.AddDays(4));
        await database.SeedLink(42, oldestId, "https://example.test/oldest", "https://example.test/oldest", "Oldest", true);
        await database.SeedLink(42, oldestIncludedId, "https://example.test/oldest-included", "https://example.test/oldest-included", "Oldest included", true);
        await database.SeedLink(42, firstTieId, "https://example.test/first-tie", "https://example.test/first-tie", "First tie", true);
        await database.SeedLink(42, secondTieId, "https://example.test/second-tie", "https://example.test/second-tie", "Second tie", true);
        await database.SeedLink(42, newerId, "https://example.test/newer", "https://example.test/newer", "Newer", true);
        await database.SeedLink(42, newestId, "https://example.test/newest", "https://example.test/newest", "Newest", true);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicUserDto? result = await repository.GetUserPage("owner");

        Assert.NotNull(result);
        Assert.Equal(
            new[] { "newest", "newer", "second-tie", "first-tie", "oldest-included" },
            result.PublicCollections.Select(collection => collection.Slug));
    }

    [Fact]
    public async Task GetUserPage_WhenUserHasMoreThanSixPublicLinks_ReturnsSixMostRecentInOrder()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Public", "public", true);
        DateTime baseDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await database.SeedLink(42, collectionId, "https://example.test/excluded", "https://example.test/excluded", "Excluded", true, new SqliteTestLinkOptions(CreatedAt: baseDate.AddDays(-1)));
        await database.SeedLink(42, collectionId, "https://example.test/oldest", "https://example.test/oldest", "Oldest", true, new SqliteTestLinkOptions(CreatedAt: baseDate));
        await database.SeedLink(42, collectionId, "https://example.test/oldest-included", "https://example.test/oldest-included", "Oldest included", true, new SqliteTestLinkOptions(CreatedAt: baseDate.AddDays(1)));
        await database.SeedLink(42, collectionId, "https://example.test/first-tie", "https://example.test/first-tie", "First tie", true, new SqliteTestLinkOptions(CreatedAt: baseDate.AddDays(2)));
        await database.SeedLink(42, collectionId, "https://example.test/second-tie", "https://example.test/second-tie", "Second tie", true, new SqliteTestLinkOptions(CreatedAt: baseDate.AddDays(2)));
        await database.SeedLink(42, collectionId, "https://example.test/newer", "https://example.test/newer", "Newer", true, new SqliteTestLinkOptions(CreatedAt: baseDate.AddDays(3)));
        await database.SeedLink(42, collectionId, "https://example.test/newest", "https://example.test/newest", "Newest", true, new SqliteTestLinkOptions(CreatedAt: baseDate.AddDays(4)));
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicUserDto? result = await repository.GetUserPage("owner");

        Assert.NotNull(result);
        Assert.Equal(
            new[]
            {
                "https://example.test/newest",
                "https://example.test/newer",
                "https://example.test/second-tie",
                "https://example.test/first-tie",
                "https://example.test/oldest-included",
                "https://example.test/oldest",
            },
            result.RecentLinks.Select(link => link.UrlOriginal));
    }

    [Fact]
    public async Task GetUserPage_WhenAliasDoesNotExist_ReturnsNull()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicUserDto? result = await repository.GetUserPage("missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserPage_WhenOtherUserHasPublicContent_ReturnsOnlyRequestedUsersData()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long ownerCollectionId = await database.SeedCollection(42, "Owner", "owner-public", true);
        long otherCollectionId = await database.SeedCollection(99, "Other", "other-public", true);
        await database.SeedLink(42, ownerCollectionId, "https://example.test/owner", "https://example.test/owner", "Owner link", true);
        await database.SeedLink(99, otherCollectionId, "https://example.test/other", "https://example.test/other", "Other link", true);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicUserDto? result = await repository.GetUserPage("owner");

        Assert.NotNull(result);
        Assert.Equal("owner", result.Alias);
        Assert.Equal(new[] { "owner-public" }, result.PublicCollections.Select(collection => collection.Slug));
        Assert.Equal(new[] { "https://example.test/owner" }, result.RecentLinks.Select(link => link.UrlOriginal));
    }

    [Fact]
    public async Task GetCollectionsPage_WhenPrivateCollectionContainsPublicLink_ExcludesIt()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Private", "private", false);
        await database.SeedLink(42, collectionId, "https://example.test/hidden", "https://example.test/hidden", "Hidden link", true);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.PublicCollectionSummaryDto>? result = await repository.GetCollectionsPage("owner");

        Assert.Empty(result!);
    }

    [Fact]
    public async Task GetCollectionsPage_WhenOtherUserHasPublicContent_ReturnsOnlyRequestedUsersCollections()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long ownerCollectionId = await database.SeedCollection(42, "Owner", "owner-public", true);
        long otherCollectionId = await database.SeedCollection(99, "Other", "other-public", true);
        await database.SeedLink(42, ownerCollectionId, "https://example.test/owner", "https://example.test/owner", "Owner link", true);
        await database.SeedLink(99, otherCollectionId, "https://example.test/other", "https://example.test/other", "Other link", true);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.PublicCollectionSummaryDto>? result = await repository.GetCollectionsPage("owner");

        Assert.NotNull(result);
        Assert.Equal(new[] { "owner-public" }, result.Select(collection => collection.Slug));
    }

    [Fact]
    public async Task GetCollectionsPage_WhenCollectionIsPublicAndHasPublicLink_IncludesIt()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Public", "public", true, "Description");
        await database.SeedLink(42, collectionId, "https://example.test/public", "https://example.test/public", "Public", true);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.PublicCollectionSummaryDto>? result = await repository.GetCollectionsPage("owner");

        Assert.Equal("public", result!.Single().Slug);
    }

    [Fact]
    public async Task GetCollectionPage_WhenCollectionIsPrivate_ReturnsNull()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollection(42, "Private", "private", false);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicCollectionDto? result = await repository.GetCollectionPage("owner", "private", 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCollectionPage_WhenOtherUserHasSamePublicSlug_ReturnsOnlyRequestedUsersLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long ownerCollectionId = await database.SeedCollection(42, "Owner", "shared", true);
        long otherCollectionId = await database.SeedCollection(99, "Other", "shared", true);
        await database.SeedLink(42, ownerCollectionId, "https://example.test/owner", "https://example.test/owner", "Owner link", true);
        await database.SeedLink(99, otherCollectionId, "https://example.test/other", "https://example.test/other", "Other link", true);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicCollectionDto? result = await repository.GetCollectionPage("owner", "shared", 1);

        Assert.NotNull(result);
        Assert.Equal("Owner", result.CollectionName);
        Assert.Equal(new[] { "https://example.test/owner" }, result.Links.Select(link => link.UrlOriginal));
    }

    [Fact]
    public async Task GetCollectionPage_ReturnsPublicLinksInAlphabeticalOrderWithIdentifierTieBreak()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Public", "public", true);
        await database.SeedLink(42, collectionId, "https://example.test/z", "https://example.test/z", "Zulu", true);
        await database.SeedLink(42, collectionId, "https://example.test/a", "https://example.test/a", "Árbol", true);
        await database.SeedLink(42, collectionId, "https://example.test/b", "https://example.test/b", "arbol", true);
        await database.SeedLink(42, collectionId, "https://example.test/private", "https://example.test/private", "Private", false);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicCollectionDto? result = await repository.GetCollectionPage("owner", "public", 1);

        Assert.Equal(new[] { "https://example.test/a", "https://example.test/b", "https://example.test/z" }, result!.Links.Select(item => item.UrlOriginal));
    }

    [Fact]
    public async Task GetCollectionPageCount_WhenCollectionHasPublicAndPrivateLinks_CountsOnlyPublicLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Public", "public", true);
        await database.SeedLink(42, collectionId, "https://example.test/public", "https://example.test/public", "Public", true);
        await database.SeedLink(42, collectionId, "https://example.test/private", "https://example.test/private", "Private", false);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        int result = await repository.GetCollectionPageCount("owner", "public");

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetCollectionPage_WhenMoreThanOnePageExists_ReturnsSixteenPublicLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Public", "public", true);
        await database.SeedLinkRange(42, collectionId, 17, isPublic: true);
        await database.SeedLink(42, collectionId, "https://example.test/private", "https://example.test/private", "A private link", false);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicCollectionDto? result = await repository.GetCollectionPage("owner", "public", 1);

        Assert.NotNull(result);
        Assert.Equal(Enumerable.Range(0, 16).Select(index => $"https://example.test/{index:D2}"), result.Links.Select(item => item.UrlOriginal));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(16, 1)]
    [InlineData(17, 2)]
    [InlineData(32, 2)]
    [InlineData(33, 3)]
    public async Task GetCollectionPageCount_WithDifferentPublicLinkCounts_UsesExactPageBoundaries(int count, int expectedPages)
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Public", "public", true);
        await database.SeedLinkRange(42, collectionId, count, isPublic: true);
        await database.SeedLink(42, collectionId, "https://example.test/private", "https://example.test/private", "Private link", false);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        int result = await repository.GetCollectionPageCount("owner", "public");

        Assert.Equal(expectedPages, result);
    }

    [Fact]
    public async Task GetCollectionPage_WhenPageIsSecond_ReturnsRemainingPublicLink()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Public", "public", true);
        await database.SeedLinkRange(42, collectionId, 17, isPublic: true);

        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicCollectionDto? result = await repository.GetCollectionPage("owner", "public", 2);

        Assert.Single(result!.Links);
        Assert.Equal("https://example.test/16", result.Links.Single().UrlOriginal);
    }

    [Fact]
    public async Task GetCollectionPageCount_WhenCollectionDoesNotExist_ReturnsZero()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        int result = await repository.GetCollectionPageCount("owner", "missing");

        Assert.Equal(0, result);
    }
}
