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
    private const int SqliteUniqueConstraintError = 2067;
    private const long OwnerUserId = 42;
    private const long OtherUserId = 99;

    [Fact]
    public async Task UniqueEmail_WhenDuplicateIsInserted_RejectsInsert()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        await using SqliteConnection connection = database.OpenConnection();

        string[] before = (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'Email', Email, 'Alias', Alias) FROM User ORDER BY Id;")).ToArray();
        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => InsertUser(connection, 100, "owner@example.test", "new-alias"));
        Assert.Equal(SqliteUniqueConstraintError, exception.SqliteExtendedErrorCode);
        Assert.Equal(before, (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'Email', Email, 'Alias', Alias) FROM User ORDER BY Id;")).ToArray());
        await InsertUser(connection, 101, "valid@example.test", "valid-alias");
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

        string[] before = (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'Email', Email, 'Alias', Alias) FROM User ORDER BY Id;")).ToArray();
        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => InsertUser(connection, 100, "new@example.test", "owner"));
        Assert.Equal(SqliteUniqueConstraintError, exception.SqliteExtendedErrorCode);
        Assert.Equal(before, (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'Email', Email, 'Alias', Alias) FROM User ORDER BY Id;")).ToArray());
        await InsertUser(connection, 101, "valid@example.test", "valid-alias");
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
        await database.SeedCollection(OtherUserId, "Other owner", "duplicate-slug", false);
        await using SqliteConnection connection = database.OpenConnection();
        string[] before = (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'UserId', UserId, 'Name', Name, 'Slug', Slug) FROM Collection ORDER BY Id;")).ToArray();

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => database.SeedCollection(OwnerUserId, "Second", "duplicate-slug", false));
        Assert.Equal(SqliteUniqueConstraintError, exception.SqliteExtendedErrorCode);
        Assert.Equal(before, (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'UserId', UserId, 'Name', Name, 'Slug', Slug) FROM Collection ORDER BY Id;")).ToArray());
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE Slug = 'duplicate-slug';"));
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
        long otherCollectionId = await database.SeedCollection(OtherUserId, "Other", "other", false);
        await database.SeedLink(OtherUserId, otherCollectionId, "https://example.test/other", "https://example.test/normalized", "Other", false);
        await using SqliteConnection connection = database.OpenConnection();
        string[] before = (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'UserId', UserId, 'CollectionId', CollectionId, 'UrlOriginal', UrlOriginal, 'UrlNormalized', UrlNormalized, 'Title', Title) FROM Link ORDER BY Id;")).ToArray();

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => database.SeedLink(OwnerUserId, collectionId, "https://example.test/two", "https://example.test/normalized", "Second", false));
        Assert.Equal(SqliteUniqueConstraintError, exception.SqliteExtendedErrorCode);
        Assert.Equal(before, (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'UserId', UserId, 'CollectionId', CollectionId, 'UrlOriginal', UrlOriginal, 'UrlNormalized', UrlNormalized, 'Title', Title) FROM Link ORDER BY Id;")).ToArray());
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link WHERE UrlNormalized = 'https://example.test/normalized';"));
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
        var rejected = firstResult.IsSuccess ? secondResult : firstResult;
        Assert.Equal("LAST_COLLECTION_CANNOT_BE_DELETED", Assert.Single(rejected.Errors).Code);
        long survivorId = firstResult.IsSuccess ? secondCollectionId : firstCollectionId;
        Assert.Equal(survivorId, await connection.QuerySingleAsync<long>("SELECT Id FROM Collection WHERE UserId = @UserId;", new { UserId = OwnerUserId }));
        Assert.Equal(1, remainingCollectionCount);
    }

    [Fact]
    public async Task UpdateCollection_WhenPrivatizedAndRenamed_UpdatesLinksAndSearchWithoutChangingOtherCollections()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Original", "original", true);
        long otherCollectionId = await database.SeedCollection(OwnerUserId, "Other", "other", true);
        long publicId = await database.SeedLink(OwnerUserId, collectionId, "https://example.test/public", "https://example.test/public", "Public", true);
        long privateId = await database.SeedLink(OwnerUserId, collectionId, "https://example.test/private", "https://example.test/private", "Private", false);
        long otherId = await database.SeedLink(OwnerUserId, otherCollectionId, "https://example.test/other", "https://example.test/other", "Other", true);
        await using SqliteConnection connection = database.OpenConnection();
        InfrastructureSqlite.SqliteConnectionFactory connectionFactory = new(database.ConnectionString);
        AppContracts.ICollectionRepository collectionRepository = new InfrastructureRepositories.SqliteCollectionRepository(connection);
        AppContracts.ILinkRepository linkRepository = new InfrastructureRepositories.SqliteLinkRepository(connection);
        AppContracts.IUnitOfWorkFactory unitOfWorkFactory = new InfrastructureSqlite.SqliteUnitOfWorkFactory(connectionFactory);
        CollectionService service = new(NullLogger<CollectionService>.Instance, new AppModels.CurrentUser(OwnerUserId), collectionRepository, unitOfWorkFactory);

        Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.CollectionDto> result = await service.UpdateCollection(
            new AppRequests.CollectionUpdateRequest(collectionId, "Renamed", "Description", false));
        Dictionary<long, long> states = (await connection.QueryAsync<(long Id, long IsPublic)>("SELECT Id, IsPublic FROM Link;"))
            .ToDictionary(item => item.Id, item => item.IsPublic);
        IReadOnlyCollection<Linkubator.Application.Dtos.Models.LinkDto> searchResults = await linkRepository.GetPageByUser(OwnerUserId, "Renamed", null, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.LinkCount);
        Assert.Equal(0, states[publicId]);
        Assert.Equal(0, states[privateId]);
        Assert.Equal(1, states[otherId]);
        Assert.Equal(new[] { publicId, privateId }.OrderBy(id => id), searchResults.Select(item => item.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task UpdateCollection_WhenPrivatizingCollectionAndCreatingPublicLinkConcurrently_LeavesNoPublicLinks()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Public", "public", true);
        await using SqliteConnection connection = database.OpenConnection();
        InfrastructureSqlite.SqliteConnectionFactory connectionFactory = new(database.ConnectionString);
        AppModels.CurrentUser currentUser = new(OwnerUserId);
        AppContracts.ICollectionRepository collectionRepository = new InfrastructureRepositories.SqliteCollectionRepository(connection);
        AppContracts.ILinkRepository linkRepository = new InfrastructureRepositories.SqliteLinkRepository(connection);
        AppContracts.IUnitOfWorkFactory unitOfWorkFactory = new InfrastructureSqlite.SqliteUnitOfWorkFactory(connectionFactory);
        CollectionService collectionService = new(NullLogger<CollectionService>.Instance, currentUser, collectionRepository, unitOfWorkFactory);
        LinkService linkService = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        (Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.CollectionDto> collectionResult,
            Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.LinkDto> linkResult) = await RunConcurrently(
            () => collectionService.UpdateCollection(new AppRequests.CollectionUpdateRequest(collectionId, "Public", null, false)),
            () => linkService.CreateLink(new AppRequests.LinkCreateRequest(collectionId, "https://example.test/concurrent", IsPublic: true)));
        long publicLinkCount = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM Link WHERE UserId = @UserId AND CollectionId = @CollectionId AND IsPublic = 1;",
            new { UserId = OwnerUserId, CollectionId = collectionId });

        Assert.True(collectionResult.IsSuccess);
        Assert.True(linkResult.IsSuccess || linkResult.Errors.Any(error => error.Code == "LINK_PUBLIC_IN_PRIVATE_COLLECTION"));
        Assert.Equal(0, publicLinkCount);
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

        var (collectionResult, linkResult) = await RunConcurrently(
            () => collectionService.UpdateCollection(new AppRequests.CollectionUpdateRequest(collectionId, "Public", null, false)),
            () => linkService.UpdateLink(new AppRequests.LinkUpdateRequest(linkId, collectionId, "Link", null, null, true)));
        int collectionIsPublic = await connection.ExecuteScalarAsync<int>(
            "SELECT IsPublic FROM Collection WHERE Id = @CollectionId;",
            new { CollectionId = collectionId });
        int linkIsPublic = await connection.ExecuteScalarAsync<int>(
            "SELECT IsPublic FROM Link WHERE Id = @LinkId;",
            new { LinkId = linkId });

        Assert.True(collectionResult.IsSuccess);
        Assert.True(linkResult.IsSuccess || (linkResult.Errors.Count == 1 && linkResult.Errors[0].Code == "LINK_PUBLIC_IN_PRIVATE_COLLECTION"));
        Assert.Equal(0, collectionIsPublic);
        Assert.Equal(0, linkIsPublic);
    }

    [Fact]
    public async Task UpdateCollection_WhenLinkUpdateFails_RollsBackCollectionAndSearchProjections()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Original name", "original-name", true, "Original description");
        long linkId = await database.SeedLink(
            OwnerUserId,
            collectionId,
            "https://example.test/rollback",
            "https://example.test/rollback",
            "Public link",
            true);
        long failingLinkId = await database.SeedLink(
            OwnerUserId,
            collectionId,
            "https://example.test/rollback-fail",
            "https://example.test/rollback-fail",
            "Second public link",
            true);
        await using SqliteConnection connection = database.OpenConnection();
        await connection.ExecuteAsync(
            """
            CREATE TRIGGER FailCollectionLinkPrivacyUpdate
            BEFORE UPDATE OF IsPublic ON Link
            WHEN OLD.UrlNormalized = 'https://example.test/rollback-fail' AND NEW.IsPublic = 0
            BEGIN
                SELECT RAISE(ABORT, 'Injected link update failure');
            END;
            """);
        InfrastructureSqlite.SqliteConnectionFactory connectionFactory = new(database.ConnectionString);
        AppModels.CurrentUser currentUser = new(OwnerUserId);
        AppContracts.ICollectionRepository collectionRepository = new InfrastructureRepositories.SqliteCollectionRepository(connection);
        AppContracts.IUnitOfWorkFactory unitOfWorkFactory = new InfrastructureSqlite.SqliteUnitOfWorkFactory(connectionFactory);
        CollectionService collectionService = new(
            NullLogger<CollectionService>.Instance,
            currentUser,
            collectionRepository,
            unitOfWorkFactory);

        string[] linksBefore = (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'UserId', UserId, 'CollectionId', CollectionId, 'UrlOriginal', UrlOriginal, 'UrlNormalized', UrlNormalized, 'Title', Title, 'Description', Description, 'Image', Image, 'IsPublic', IsPublic, 'CreatedAt', CreatedAt) FROM Link ORDER BY Id;")).ToArray();
        string[] projectionsBefore = (await connection.QueryAsync<string>("SELECT json_object('Id', rowid, 'UserId', UserId, 'UrlOriginal', UrlOriginal, 'Title', Title, 'Description', Description, 'CollectionName', CollectionName) FROM LinkSearch ORDER BY rowid;")).ToArray();
        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(
            () => collectionService.UpdateCollection(
                new AppRequests.CollectionUpdateRequest(collectionId, "Updated name", "Updated description", false)));
        Assert.Contains("Injected link update failure", exception.Message, StringComparison.Ordinal);
        Assert.Equal(linksBefore, (await connection.QueryAsync<string>("SELECT json_object('Id', Id, 'UserId', UserId, 'CollectionId', CollectionId, 'UrlOriginal', UrlOriginal, 'UrlNormalized', UrlNormalized, 'Title', Title, 'Description', Description, 'Image', Image, 'IsPublic', IsPublic, 'CreatedAt', CreatedAt) FROM Link ORDER BY Id;")).ToArray());
        Assert.Equal(projectionsBefore, (await connection.QueryAsync<string>("SELECT json_object('Id', rowid, 'UserId', UserId, 'UrlOriginal', UrlOriginal, 'Title', Title, 'Description', Description, 'CollectionName', CollectionName) FROM LinkSearch ORDER BY rowid;")).ToArray());

        PersistedCollectionState collectionState = await connection.QuerySingleAsync<PersistedCollectionState>(
            "SELECT Name, Slug, Description, IsPublic FROM Collection WHERE Id = @CollectionId;",
            new { CollectionId = collectionId });
        PersistedCollectionSearchState collectionSearchState = await connection.QuerySingleAsync<PersistedCollectionSearchState>(
            "SELECT Name, Description FROM CollectionSearch WHERE rowid = @CollectionId AND UserId = @UserId;",
            new { CollectionId = collectionId, UserId = OwnerUserId });
        int linkVisibility = await connection.ExecuteScalarAsync<int>(
            "SELECT IsPublic FROM Link WHERE Id = @LinkId;",
            new { LinkId = linkId });
        int failingLinkVisibility = await connection.ExecuteScalarAsync<int>(
            "SELECT IsPublic FROM Link WHERE Id = @LinkId;",
            new { LinkId = failingLinkId });
        string? linkSearchCollectionName = await connection.ExecuteScalarAsync<string?>(
            "SELECT CollectionName FROM LinkSearch WHERE rowid = @LinkId AND UserId = @UserId;",
            new { LinkId = linkId, UserId = OwnerUserId });

        Assert.Equal(new PersistedCollectionState("Original name", "original-name", "Original description", 1), collectionState);
        Assert.Equal(new PersistedCollectionSearchState("Original name", "Original description"), collectionSearchState);
        Assert.Equal(1, linkVisibility);
        Assert.Equal(1, failingLinkVisibility);
        Assert.Equal("Original name", linkSearchCollectionName);
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PrivacyOperations_WhenPublicationCommitsFirst_PrivatizationWins(bool createLink)
    {
        await VerifyOrderedPrivacyOperations(createLink, false);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PrivacyOperations_WhenPrivatizationCommitsFirst_PublicationIsRejected(bool createLink)
    {
        await VerifyOrderedPrivacyOperations(createLink, true);
    }

    private static async Task VerifyOrderedPrivacyOperations(bool createLink, bool privatizeFirst)
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        long collectionId = await database.SeedCollection(OwnerUserId, "Public", "ordered", true);
        long linkId = await database.SeedLink(OwnerUserId, collectionId, "https://example.test/existing", "existing", "Existing", false);
        await using SqliteConnection connection = database.OpenConnection();
        InfrastructureSqlite.SqliteConnectionFactory connections = new(database.ConnectionString);
        AppContracts.ICollectionRepository collections = new InfrastructureRepositories.SqliteCollectionRepository(connection);
        AppContracts.ILinkRepository links = new InfrastructureRepositories.SqliteLinkRepository(connection);
        InfrastructureSqlite.SqliteUnitOfWorkFactory inner = new(connections);
        GatedUnitOfWorkFactory gated = new(inner);
        AppModels.CurrentUser user = new(OwnerUserId);
        CollectionService collectionService = new(NullLogger<CollectionService>.Instance, user, collections, privatizeFirst ? inner : gated);
        LinkService linkService = new(NullLogger<LinkService>.Instance, user, collections, links, privatizeFirst ? gated : inner);
        Func<Task<Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.LinkDto>>> publish = () => createLink
            ? linkService.CreateLink(new AppRequests.LinkCreateRequest(collectionId, "https://example.test/created", IsPublic: true))
            : linkService.UpdateLink(new AppRequests.LinkUpdateRequest(linkId, collectionId, "Existing", IsPublic: true));
        Func<Task<Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.CollectionDto>>> privatize = () =>
            collectionService.UpdateCollection(new AppRequests.CollectionUpdateRequest(collectionId, "Public", null, false));
        Task<Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.LinkDto>> publication;
        Task<Linkubator.Application.Results.Result<Linkubator.Application.Dtos.Models.CollectionDto>> privacy;
        if (privatizeFirst)
        {
            publication = publish();
            try
            {
                await gated.WaitUntilRequested();
                privacy = privatize();
                Assert.True((await privacy).IsSuccess);
            }
            finally { gated.Release(); }
        }
        else
        {
            privacy = privatize();
            try
            {
                await gated.WaitUntilRequested();
                publication = publish();
                Assert.True((await publication).IsSuccess);
            }
            finally { gated.Release(); }
        }
        var publicationResult = await publication;
        Assert.True((await privacy).IsSuccess);
        Assert.Equal(!privatizeFirst, publicationResult.IsSuccess);
        Assert.Equal(privatizeFirst ? new[] { "LINK_PUBLIC_IN_PRIVATE_COLLECTION" } : Array.Empty<string>(),
            publicationResult.Errors.Select(error => error.Code));
        Assert.Equal(0, await connection.ExecuteScalarAsync<long>("SELECT IsPublic FROM Collection WHERE Id = @Id;", new { Id = collectionId }));
        Assert.Equal(0, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Link WHERE CollectionId = @Id AND IsPublic = 1;", new { Id = collectionId }));
        Assert.Equal(createLink && !privatizeFirst ? 2 : 1, await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Link WHERE CollectionId = @Id;", new { Id = collectionId }));
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

    private sealed record PersistedCollectionState(string Name, string Slug, string? Description, long IsPublic);

    private sealed record PersistedCollectionSearchState(string Name, string? Description);
}