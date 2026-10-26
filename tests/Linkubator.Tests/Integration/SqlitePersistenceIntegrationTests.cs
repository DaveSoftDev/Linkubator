using Dapper;
using AppContracts = Linkubator.Application.Contracts;
using AppModels = Linkubator.Application.Models;
using AppRequests = Linkubator.Application.Dtos.Requests;
using LinkService = Linkubator.Application.UseCases.Links.LinkService;
using CollectionService = Linkubator.Application.UseCases.Collections.CollectionService;
using AppUnitOfWork = Linkubator.Application.Contracts.IUnitOfWork;
using AppUnitOfWorkFactory = Linkubator.Application.Contracts.IUnitOfWorkFactory;
using InfrastructureRepositories = Linkubator.Infrastructure.Repositories;
using InfrastructureSqlite = Linkubator.Infrastructure.Sqlite;
using SqliteCollectionRepository = Linkubator.Infrastructure.Repositories.SqliteCollectionRepository;
using SqlitePublicReadRepository = Linkubator.Infrastructure.Repositories.SqlitePublicReadRepository;
using Linkubator.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Integration;

public sealed class SqlitePersistenceIntegrationTests
{
    private const int SqliteConstraintError = 19;
    private const long OwnerUserId = 42;
    private const long OtherUserId = 99;

    [Fact]
    public async Task UniqueEmail_WhenDuplicateIsInserted_RejectsInsert()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => InsertUser(connection, 100, "owner@example.test", "new-alias"));
        long userCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM User WHERE Email = @Email;",
            new { Email = "owner@example.test" });

        Assert.Equal(SqliteConstraintError, exception.SqliteErrorCode);
        Assert.Equal(1, userCount);
    }

    [Fact]
    public async Task UniqueAlias_WhenDuplicateIsInserted_RejectsInsert()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => InsertUser(connection, 100, "new@example.test", "owner"));
        long userCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM User WHERE Alias = @Alias;",
            new { Alias = "owner" });

        Assert.Equal(SqliteConstraintError, exception.SqliteErrorCode);
        Assert.Equal(1, userCount);
    }

    [Fact]
    public async Task UniqueAlias_WhenMultipleIncompleteUsersHaveNullAlias_AllowsInsert()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();

        await InsertUser(connection, 100, "incomplete-one@example.test", null);
        await InsertUser(connection, 101, "incomplete-two@example.test", null);
        long userCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM User WHERE Alias IS NULL;");

        Assert.Equal(2, userCount);
    }

    [Fact]
    public async Task UniqueCollectionSlug_WhenDuplicateForSameUserIsInserted_RejectsInsert()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await database.SeedCollection(OwnerUserId, "First", "duplicate-slug", false);

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => database.SeedCollection(OwnerUserId, "Second", "duplicate-slug", false));
        await using SqliteConnection connection = database.OpenConnection();
        long collectionCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM Collection WHERE UserId = @UserId AND Slug = @Slug;",
            new { UserId = OwnerUserId, Slug = "duplicate-slug" });

        Assert.Equal(SqliteConstraintError, exception.SqliteErrorCode);
        Assert.Equal(1, collectionCount);
    }

    [Fact]
    public async Task UniqueNormalizedUrl_WhenDuplicateForSameUserIsInserted_RejectsInsert()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Links", "links", false);
        await database.SeedLink(OwnerUserId, collectionId, "https://example.test/one", "https://example.test/normalized", "First", false);

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => database.SeedLink(OwnerUserId, collectionId, "https://example.test/two", "https://example.test/normalized", "Second", false));
        await using SqliteConnection connection = database.OpenConnection();
        long linkCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM Link WHERE UserId = @UserId AND UrlNormalized = @UrlNormalized;",
            new { UserId = OwnerUserId, UrlNormalized = "https://example.test/normalized" });

        Assert.Equal(SqliteConstraintError, exception.SqliteErrorCode);
        Assert.Equal(1, linkCount);
    }

    [Fact]
    public async Task CompositeForeignKey_WhenLinkReferencesAnotherUsersCollection_RejectsInsert()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long otherCollectionId = await database.SeedCollection(OtherUserId, "Other", "other", false);
        await using SqliteConnection connection = database.OpenConnection();

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(() => connection.ExecuteAsync(
            """
            INSERT INTO Link (UserId, CollectionId, UrlOriginal, UrlNormalized, IsPublic)
            VALUES (@UserId, @CollectionId, @UrlOriginal, @UrlNormalized, 0);
            """,
            new
            {
                UserId = OwnerUserId,
                CollectionId = otherCollectionId,
                UrlOriginal = "https://example.test/foreign",
                UrlNormalized = "https://example.test/foreign"
            }));
        long linkCount = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Link;");

        Assert.Equal(SqliteConstraintError, exception.SqliteErrorCode);
        Assert.Equal(0, linkCount);
    }

    [Fact]
    public async Task MoveLink_WhenDestinationBelongsToAnotherUser_RejectsMoveAndPreservesLink()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long sourceCollectionId = await database.SeedCollection(OwnerUserId, "Source", "source", false);
        long destinationCollectionId = await database.SeedCollection(OtherUserId, "Other", "other", false);
        long linkId = await database.SeedLink(OwnerUserId, sourceCollectionId, "https://example.test/move", "https://example.test/move", "Original", false);
        await using SqliteConnection connection = database.OpenConnection();
        InfrastructureSqlite.SqliteConnectionFactory connectionFactory = new(database.ConnectionString);
        AppContracts.ICollectionRepository collectionRepository = new InfrastructureRepositories.SqliteCollectionRepository(connection);
        AppContracts.ILinkRepository linkRepository = new InfrastructureRepositories.SqliteLinkRepository(connection);
        AppContracts.IUnitOfWorkFactory unitOfWorkFactory = new InfrastructureSqlite.SqliteUnitOfWorkFactory(connectionFactory);
        LinkService service = new(
            NullLogger<LinkService>.Instance,
            new AppModels.CurrentUser(OwnerUserId),
            collectionRepository,
            linkRepository,
            unitOfWorkFactory);

        Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.LinkDto> result = await service.MoveLink(
            new AppRequests.LinkMoveRequest(linkId, destinationCollectionId));
        SqliteException foreignKeyException = await Assert.ThrowsAsync<SqliteException>(() => connection.ExecuteAsync(
            "UPDATE Link SET CollectionId = @DestinationCollectionId WHERE Id = @LinkId;",
            new { DestinationCollectionId = destinationCollectionId, LinkId = linkId }));
        Linkubator.Domain.Entities.Link? persistedLink = await linkRepository.GetById(linkId, OwnerUserId);

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.Equal(SqliteConstraintError, foreignKeyException.SqliteErrorCode);
        Assert.NotNull(persistedLink);
        Assert.Equal(sourceCollectionId, persistedLink.CollectionId);
        Assert.Equal("Original", persistedLink.Title);
    }

    [Fact]
    public async Task DeleteCollections_WhenDeletingLastCollectionsConcurrently_PreservesOneCollection()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long firstCollectionId = await database.SeedCollection(OwnerUserId, "First", "first", false);
        long secondCollectionId = await database.SeedCollection(OwnerUserId, "Second", "second", false);
        await using SqliteConnection connection = database.OpenConnection();
        InfrastructureSqlite.SqliteConnectionFactory connectionFactory = new(database.ConnectionString);
        AppModels.CurrentUser currentUser = new(OwnerUserId);
        AppContracts.ICollectionRepository collectionRepository = new InfrastructureRepositories.SqliteCollectionRepository(connection);
        AppContracts.IUnitOfWorkFactory unitOfWorkFactory = new InfrastructureSqlite.SqliteUnitOfWorkFactory(connectionFactory);
        CollectionService service = new(NullLogger<CollectionService>.Instance, currentUser, collectionRepository, unitOfWorkFactory);

        (Linkubator.Application.Results.Result firstResult, Linkubator.Application.Results.Result secondResult) = await RunConcurrently(
            () => service.DeleteCollection(new AppRequests.CollectionDeleteRequest(firstCollectionId)),
            () => service.DeleteCollection(new AppRequests.CollectionDeleteRequest(secondCollectionId)));
        long remainingCollectionCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM Collection WHERE UserId = @UserId;",
            new { UserId = OwnerUserId });

        Assert.NotEqual(firstResult.IsSuccess, secondResult.IsSuccess);
        Assert.Equal(1, remainingCollectionCount);
    }

    [Fact]
    public async Task UpdateCollection_WhenPrivatizingCollectionAndPublishingLinkConcurrently_KeepsLinkPrivate()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Public", "public", true);
        long linkId = await database.SeedLink(OwnerUserId, collectionId, "https://example.test/visibility", "https://example.test/visibility", "Link", false);
        await using SqliteConnection connection = database.OpenConnection();
        InfrastructureSqlite.SqliteConnectionFactory connectionFactory = new(database.ConnectionString);
        AppModels.CurrentUser currentUser = new(OwnerUserId);
        AppContracts.ICollectionRepository collectionRepository = new InfrastructureRepositories.SqliteCollectionRepository(connection);
        AppContracts.ILinkRepository linkRepository = new InfrastructureRepositories.SqliteLinkRepository(connection);
        AppContracts.IUnitOfWorkFactory unitOfWorkFactory = new InfrastructureSqlite.SqliteUnitOfWorkFactory(connectionFactory);
        CollectionService collectionService = new(NullLogger<CollectionService>.Instance, currentUser, collectionRepository, unitOfWorkFactory);
        LinkService linkService = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await RunConcurrently(
            () => collectionService.UpdateCollection(new AppRequests.CollectionUpdateRequest(collectionId, "Public", null, false)),
            () => linkService.UpdateLink(new AppRequests.LinkUpdateRequest(linkId, collectionId, "Link", null, null, true)));
        int collectionIsPublic = await connection.ExecuteScalarAsync<int>(
            "SELECT IsPublic FROM Collection WHERE Id = @CollectionId;",
            new { CollectionId = collectionId });
        int linkIsPublic = await connection.ExecuteScalarAsync<int>(
            "SELECT IsPublic FROM Link WHERE Id = @LinkId;",
            new { LinkId = linkId });

        Assert.Equal(0, collectionIsPublic);
        Assert.Equal(0, linkIsPublic);
    }

    [Fact]
    public async Task CreatePublicLink_WhenCollectionBecomesPrivateBeforeUnitOfWork_RejectsCreation()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Public", "public-create", true);
        await using SqliteConnection connection = database.OpenConnection();
        InfrastructureSqlite.SqliteConnectionFactory connectionFactory = new(database.ConnectionString);
        AppModels.CurrentUser currentUser = new(OwnerUserId);
        AppContracts.ICollectionRepository collectionRepository = new InfrastructureRepositories.SqliteCollectionRepository(connection);
        AppContracts.ILinkRepository linkRepository = new InfrastructureRepositories.SqliteLinkRepository(connection);
        InfrastructureSqlite.SqliteUnitOfWorkFactory innerUnitOfWorkFactory = new(connectionFactory);
        GatedUnitOfWorkFactory gatedUnitOfWorkFactory = new(innerUnitOfWorkFactory);
        LinkService linkService = new(
            NullLogger<LinkService>.Instance,
            currentUser,
            collectionRepository,
            linkRepository,
            gatedUnitOfWorkFactory);
        CollectionService collectionService = new(
            NullLogger<CollectionService>.Instance,
            currentUser,
            collectionRepository,
            innerUnitOfWorkFactory);
        Task<Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.LinkDto>> createTask = linkService.CreateLink(
            new AppRequests.LinkCreateRequest(
                collectionId,
                "https://example.test/create-public",
                IsPublic: true));

        try
        {
            await gatedUnitOfWorkFactory.WaitUntilRequested();
            Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.CollectionDto> privacyResult = await collectionService.UpdateCollection(
                new AppRequests.CollectionUpdateRequest(collectionId, "Public", null, false));
            Assert.True(privacyResult.IsSuccess);
        }
        finally
        {
            gatedUnitOfWorkFactory.Release();
        }

        Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.LinkDto> createResult = await createTask;
        long persistedLinkCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM Link WHERE UserId = @UserId AND UrlNormalized = @Url;",
            new { UserId = OwnerUserId, Url = "https://example.test/create-public" });

        Assert.False(createResult.IsSuccess);
        Assert.Equal("LINK_PUBLIC_IN_PRIVATE_COLLECTION", createResult.Errors[0].Code);
        Assert.Equal(0, persistedLinkCount);
    }

    [Fact]
    public async Task GetPrivateCollections_WhenNamesAreEquivalent_UsesIdentifierTieBreak()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long firstId = await database.SeedCollection(OwnerUserId, "Árbol", "arbol-first", false);
        long secondId = await database.SeedCollection(OwnerUserId, "arbol", "arbol-second", false);
        await database.SeedCollection(OwnerUserId, "Boca", "boca", false);
        await using SqliteConnection connection = database.OpenConnection();
        AppContracts.ICollectionRepository repository = new SqliteCollectionRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.CollectionDto> result = await repository.GetPageByUser(
            OwnerUserId,
            null,
            1);

        Assert.Equal(new[] { firstId, secondId }, result.Take(2).Select(collection => collection.Id));
        Assert.Equal(new[] { "Árbol", "arbol", "Boca" }, result.Select(collection => collection.Name));
    }

    [Fact]
    public async Task GetPublicCollections_WhenNamesAreEquivalent_UsesIdentifierTieBreak()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long firstId = await database.SeedCollection(OwnerUserId, "Árbol", "arbol-public-first", true);
        long secondId = await database.SeedCollection(OwnerUserId, "arbol", "arbol-public-second", true);
        long laterId = await database.SeedCollection(OwnerUserId, "Boca", "boca-public", true);
        await database.SeedLink(OwnerUserId, firstId, "https://example.test/public-first", "https://example.test/public-first", "First", true);
        await database.SeedLink(OwnerUserId, secondId, "https://example.test/public-second", "https://example.test/public-second", "Second", true);
        await database.SeedLink(OwnerUserId, laterId, "https://example.test/public-later", "https://example.test/public-later", "Later", true);
        await using SqliteConnection connection = database.OpenConnection();
        AppContracts.IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        IReadOnlyCollection<Linkubator.Application.Dtos.Models.PublicCollectionSummaryDto>? result = await repository.GetCollectionsPage("owner");

        Assert.Equal(new[] { "arbol-public-first", "arbol-public-second", "boca-public" }, result!.Select(collection => collection.Slug));
    }

    [Fact]
    public async Task GetPublicCollection_WhenTitleIsMissing_OrdersByOriginalUrl()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Public", "public-fallback", true);
        await database.SeedLink(OwnerUserId, collectionId, "https://example.test/z", "https://example.test/z", "Zulu", true);
        await database.SeedLink(OwnerUserId, collectionId, "https://example.test/a", "https://example.test/a", "Árbol", true);
        await database.SeedLink(OwnerUserId, collectionId, "https://example.test/b", "https://example.test/b", "arbol", true);
        await database.SeedLink(OwnerUserId, collectionId, "https://example.test/fallback", "https://example.test/fallback", null, true);
        await using SqliteConnection connection = database.OpenConnection();
        AppContracts.IPublicReadRepository repository = new SqlitePublicReadRepository(connection);

        Linkubator.Application.Dtos.Models.PublicCollectionDto? result = await repository.GetCollectionPage("owner", "public-fallback", 1);

        Assert.Equal(
            new[] { "https://example.test/a", "https://example.test/b", "https://example.test/fallback", "https://example.test/z" },
            result!.Links.Select(link => link.UrlOriginal));
    }

    private static async Task<(TFirst First, TSecond Second)> RunConcurrently<TFirst, TSecond>(Func<Task<TFirst>> first, Func<Task<TSecond>> second)
    {
        TaskCompletionSource<bool> firstReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> secondReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<TFirst> firstTask = Task.Run(async () =>
        {
            firstReady.SetResult(true);
            await secondReady.Task;
            return await first();
        });
        Task<TSecond> secondTask = Task.Run(async () =>
        {
            secondReady.SetResult(true);
            await firstReady.Task;
            return await second();
        });

        await Task.WhenAll(firstTask, secondTask);
        return (await firstTask, await secondTask);
    }

    private static Task<int> InsertUser(SqliteConnection connection, long userId, string email, string? alias)
        => connection.ExecuteAsync(
            "INSERT INTO User (Id, Email, Alias, SecurityStamp) VALUES (@UserId, @Email, @Alias, @SecurityStamp);",
            new { UserId = userId, Email = email, Alias = alias, SecurityStamp = $"security-{userId}" });

    private sealed class GatedUnitOfWorkFactory(AppUnitOfWorkFactory inner) : AppUnitOfWorkFactory
    {
        private readonly TaskCompletionSource<bool> _requested = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public AppUnitOfWork Get(string contextName)
        {
            return new GatedUnitOfWork(inner.Get(contextName), this);
        }

        public Task WaitUntilRequested() => _requested.Task.WaitAsync(TimeSpan.FromSeconds(5));

        public void Release() => _released.TrySetResult(true);

        private async Task Begin(AppUnitOfWork unitOfWork, CancellationToken cancellationToken)
        {
            _requested.TrySetResult(true);
            await _released.Task.WaitAsync(cancellationToken);
            await unitOfWork.Begin(cancellationToken);
        }

        private sealed class GatedUnitOfWork(AppUnitOfWork inner, GatedUnitOfWorkFactory owner) : AppUnitOfWork
        {
            public TRepository GetRepository<TRepository>() where TRepository : class
                => inner.GetRepository<TRepository>();

            public Task Begin(CancellationToken cancellationToken = default)
                => owner.Begin(inner, cancellationToken);

            public Task Commit(CancellationToken cancellationToken = default)
                => inner.Commit(cancellationToken);

            public Task Rollback(CancellationToken cancellationToken = default)
                => inner.Rollback(cancellationToken);

            public void Dispose() => inner.Dispose();

            public ValueTask DisposeAsync() => inner.DisposeAsync();
        }
    }
}