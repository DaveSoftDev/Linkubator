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
            "UPDATE Link SET CreatedAt = @CreatedAt WHERE @SameCreatedAt = 1;",
            new { CreatedAt = DateTime.UnixEpoch, SameCreatedAt = sameCreatedAt });
        IDashboardRepository repository = new SqliteDashboardRepository(connection);

        Linkubator.Application.Dtos.Models.DashboardDto result = await repository.GetByUser(42);

        Assert.Equal(
            Enumerable.Range(1, 12).Reverse().Select(index => $"https://example.test/{index:D2}"),
            result.RecentLinks.Select(link => link.UrlOriginal));
    }
}
