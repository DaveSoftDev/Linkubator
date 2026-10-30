using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteDashboardRepositoryTests
{
    [Fact]
    public async Task GetByUser_WhenUserHasNoData_ReturnsEmptyRecentLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();
        IDashboardRepository repository = new SqliteDashboardRepository(connection);

        Linkubator.Application.Dtos.Models.DashboardDto result = await repository.GetByUser(42);

        Assert.Empty(result.RecentLinks);
    }

    [Fact]
    public async Task GetByUser_WhenUserHasData_ReturnsOnlyTheirRecentLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long ownCollectionId = await database.SeedCollection(42, "Árbol", "arbol", false);
        long otherCollectionId = await database.SeedCollection(99, "Other", "other", false);
        await database.SeedLink(42, ownCollectionId, "https://example.test/own", "https://example.test/own", "Own", false);
        await database.SeedLink(99, otherCollectionId, "https://example.test/other", "https://example.test/other", "Other", false);
        await using SqliteConnection connection = database.OpenConnection();
        IDashboardRepository repository = new SqliteDashboardRepository(connection);

        Linkubator.Application.Dtos.Models.DashboardDto result = await repository.GetByUser(42);

        Assert.Equal(new[] { "https://example.test/own" }, result.RecentLinks.Select(item => item.UrlOriginal));
        Assert.Equal(new[] { "Árbol" }, result.RecentLinks.Select(item => item.CollectionName));
    }

    [Fact]
    public async Task GetByUser_WhenOtherUserHasManyLinks_ReturnsCompleteOwnLinksAcrossPrivacyStates()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long privateCollection = await database.SeedCollection(42, "Private", "private", false);
        long publicCollection = await database.SeedCollection(42, "Public", "public", true);
        long otherCollection = await database.SeedCollection(99, "Other", "other", true);
        DateTime older = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long privateId = await database.SeedLink(42, privateCollection, "https://example.test/private", "private-key", "Private title", false,
            new SqliteTestLinkOptions("Private description", "https://example.test/private.png", older));
        long publicId = await database.SeedLink(42, publicCollection, "https://example.test/public", "public-key", "Public title", true,
            new SqliteTestLinkOptions("Public description", "https://example.test/public.png", older.AddDays(1)));
        await database.SeedLinkRange(99, otherCollection, 13, true);
        await using SqliteConnection connection = database.OpenConnection();
        IDashboardRepository repository = new SqliteDashboardRepository(connection);

        Linkubator.Application.Dtos.Models.DashboardDto result = await repository.GetByUser(42);

        Assert.Collection(result.RecentLinks,
            link => Assert.Equal((publicId, publicCollection, "https://example.test/public", "public-key", "Public title", "Public description", "https://example.test/public.png", true, older.AddDays(1), "Public"),
                (link.Id, link.CollectionId, link.UrlOriginal, link.UrlNormalized, link.Title, link.Description, link.Image, link.IsPublic, link.CreatedAt, link.CollectionName)),
            link => Assert.Equal((privateId, privateCollection, "https://example.test/private", "private-key", "Private title", "Private description", "https://example.test/private.png", false, older, "Private"),
                (link.Id, link.CollectionId, link.UrlOriginal, link.UrlNormalized, link.Title, link.Description, link.Image, link.IsPublic, link.CreatedAt, link.CollectionName)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetByUser_WhenUserHasMoreThanTwelveLinks_ReturnsTwelveMostRecent(bool sameCreatedAt)
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(42, "Mine", "mine", false);
        await database.SeedLinkRange(42, collectionId, 13);

        await using SqliteConnection connection = database.OpenConnection();
        await Dapper.SqlMapper.ExecuteAsync(connection,
            "UPDATE Link SET CreatedAt = CASE WHEN @SameCreatedAt = 1 THEN @CreatedAt ELSE datetime(@CreatedAt, printf('+%d minutes', 13 - Id)) END;",
            new { CreatedAt = DateTime.UnixEpoch, SameCreatedAt = sameCreatedAt });
        IDashboardRepository repository = new SqliteDashboardRepository(connection);

        Linkubator.Application.Dtos.Models.DashboardDto result = await repository.GetByUser(42);

        IEnumerable<int> expected = sameCreatedAt ? Enumerable.Range(1, 12).Reverse() : Enumerable.Range(0, 12);
        Assert.Equal(expected.Select(index => $"https://example.test/{index:D2}"), result.RecentLinks.Select(link => link.UrlOriginal));
    }
}
