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
        await database.SeedLink(42, privateCollectionId, "https://example.test/private", "https://example.test/private", "Private link", false);
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
    public async Task GetUserPage_WhenAliasDoesNotExist_ReturnsNull()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicUserDto? result = await repository.GetUserPage("missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCollectionsPage_WhenCollectionIsPrivate_ExcludesIt()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollection(42, "Private", "private", false);
        await using SqliteConnection connection = database.OpenConnection();
        IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.PublicCollectionSummaryDto>? result = await repository.GetCollectionsPage("owner");

        Assert.Empty(result!);
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
