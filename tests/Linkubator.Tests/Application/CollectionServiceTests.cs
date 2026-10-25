using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Enums;
using Linkubator.Application.Models;
using Linkubator.Application.Results;
using Linkubator.Application.UseCases.Collections;
using Linkubator.Application.Validation.Collections;
using Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Tests.Application;

public class CollectionServiceTests
{
    [Fact]
    public void Constructor_WhenDependenciesAreNull_ThrowsArgumentNullException()
    {
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CurrentUser currentUser = new CurrentUser(7);

        Assert.Throws<ArgumentNullException>(() => new CollectionService(null!, currentUser, repository, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new CollectionService(NullLogger<CollectionService>.Instance, currentUser, null!, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, null!));
        Assert.Throws<ArgumentNullException>(() => new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, null!));
    }

    [Fact]
    public void Constructor_WhenValidatorIsNull_UsesDefaultValidator()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);

        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WhenValidatorIsProvided_UsesProvidedValidator()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionServiceValidators validators = new(
            new AcceptAllGetCollectionValidator(),
            new AcceptAllCreateCollectionValidator(),
            new AcceptAllRenameCollectionValidator(),
            new AcceptAllChangeCollectionVisibilityValidator(),
            new AcceptAllDeleteCollectionValidator());

        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory, validators);

        Assert.NotNull(service);
    }

    [Fact]
    public void DomainEntities_WhenCreated_SetCreatedAtToCurrentUtcTime()
    {
        DateTime before = DateTime.UtcNow;

        Collection collection = Collection.Create(7, "My Collection", "desc", isPublic: false);
        SetId(collection, 11);

        Link link = Link.Create(7, collection, "https://example.com/article", "Title", "desc", "https://example.com/image", false);

        DateTime after = DateTime.UtcNow;

        Assert.InRange(collection.CreatedAt, before, after);
        Assert.InRange(link.CreatedAt, before, after);
    }

    [Fact]
    public async Task GetCollection_WhenUserIsUnauthenticated_ReturnsNotAuthenticated()
    {
        CurrentUser currentUser = new CurrentUser(0);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, new InMemoryLinkRepository());
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.GetCollection(new CollectionGetRequest(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(CommonErrors.NotAuthenticated().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollection_WhenCollectionIdIsInvalid_ReturnsValidationError()
    {
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        CollectionService service = new CollectionService(
            NullLogger<CollectionService>.Instance,
            new CurrentUser(7),
            repository,
            new FakeUnitOfWorkFactory(repository, new InMemoryLinkRepository()));

        Result<CollectionDto> result = await service.GetCollection(new CollectionGetRequest(0));

        Assert.False(result.IsSuccess);
        Assert.Equal(CollectionErrors.CollectionIdInvalid().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollection_WhenCollectionBelongsToCurrentUser_ReturnsCollection()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        Collection collection = Collection.Create(currentUser.UserId, "My Collection", "Description", true);
        SetId(collection, 21);
        repository.Items.Add(collection);
        CollectionService service = new CollectionService(
            NullLogger<CollectionService>.Instance,
            currentUser,
            repository,
            new FakeUnitOfWorkFactory(repository, new InMemoryLinkRepository()));

        Result<CollectionDto> result = await service.GetCollection(new CollectionGetRequest(collection.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(collection.Id, result.Value!.Id);
        Assert.Equal(collection.Name, result.Value.Name);
        Assert.Empty(collection.Links);
        Assert.Equal(0, result.Value.LinkCount);
        Assert.Equal(collection.Description, result.Value.Description);
        Assert.True(result.Value.IsPublic);
    }

    [Fact]
    public async Task GetCollection_WhenCollectionBelongsToAnotherUser_ReturnsNotAccessible()
    {
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        Collection collection = Collection.Create(8, "Other Collection", null);
        SetId(collection, 21);
        repository.Items.Add(collection);
        CollectionService service = new CollectionService(
            NullLogger<CollectionService>.Instance,
            new CurrentUser(7),
            repository,
            new FakeUnitOfWorkFactory(repository, new InMemoryLinkRepository()));

        Result<CollectionDto> result = await service.GetCollection(new CollectionGetRequest(collection.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(CollectionErrors.GetNotAccessible().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateCollection_WhenRequestIsValid_ReturnsCreatedCollection()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("  My Collection  ", " Desc ", true));

        Assert.True(result.IsSuccess);
        CollectionDto value = result.Value!;
        Assert.Equal("My Collection", value.Name);
        Assert.Equal("my-collection", value.Slug);
        Assert.True(value.IsPublic);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task CreateCollection_WhenVisibilityIsNotSpecified_CreatesPrivateCollectionForCurrentUser()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("Private collection"));

        Assert.True(result.IsSuccess);
        Collection created = Assert.Single(repository.Items);
        Assert.Equal(currentUser.UserId, created.UserId);
        Assert.False(created.IsPublic);
    }

    [Fact]
    public async Task CreateCollection_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(0);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("My Collection", "desc", true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticatedCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateCollection_WhenNameAlreadyExists_ReturnsDomainError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection existing = Collection.Create(7, "My Collection", "desc");
        SetId(existing, 11);
        repository.Items.Add(existing);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("My Collection", "desc", true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("COLLECTION_NAME_IN_USE", result.Errors[0].Code);
        Assert.Equal($"{CollectionErrors.NameInUseMessage}: {existing.Name}.", result.Errors[0].Message);
    }

    [Fact]
    public async Task CreateCollection_WhenDuplicateSlugCheckRuns_UsesTransactionalRepository()
    {
        CurrentUser currentUser = new CurrentUser(7);
        ThrowIfCalledOutsideTransactionCollectionRepository outerRepository = new();
        InMemoryCollectionRepository transactionalRepository = new();
        Collection existing = Collection.Create(7, "My Collection", "desc");
        SetId(existing, 11);
        transactionalRepository.Items.Add(existing);
        InMemoryLinkRepository linkRepository = new();
        TransactionalUnitOfWorkFactory unitOfWorkFactory = new(transactionalRepository, linkRepository);
        CollectionService service = new(NullLogger<CollectionService>.Instance, currentUser, outerRepository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("My Collection", "desc", true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("COLLECTION_NAME_IN_USE", result.Errors[0].Code);
        Assert.Equal(0, outerRepository.GetBySlugCalls);
        Assert.Equal(1, transactionalRepository.Items.Count(item => item.UserId == currentUser.UserId && item.Slug == "my-collection"));
    }

    [Fact]
    public async Task CreateCollection_WhenRequestIsNull_ReturnsValidationError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("REQUEST_REQUIRED", result.Errors[0].Code);
    }

    [Fact]
    public async Task RenameCollection_WhenOwnedAndUnique_UpdatesSlug()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Old Name", "desc");
        SetId(collection, 42);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(repository.Items[0].Id, "Better Name"));

        Assert.True(result.IsSuccess);
        CollectionDto value = result.Value!;
        Assert.Equal("Better Name", value.Name);
        Assert.Equal("better-name", value.Slug);
    }

    [Fact]
    public async Task RenameCollection_WhenNameIsBlank_ReturnsValidationError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Old Name", "desc");
        SetId(collection, 42);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(collection.Id, "   "));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("COLLECTION_NAME_REQUIRED", result.Errors[0].Code);
        Assert.Equal("Old Name", collection.Name);
        Assert.Equal("old-name", collection.Slug);
        Assert.Equal(0, repository.UpdateCalls);
    }

    [Fact]
    public async Task RenameCollection_WhenSlugCannotBeGenerated_LeavesCollectionUnchanged()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Original name", "desc");
        SetId(collection, 42);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(collection.Id, "💫"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Original name", collection.Name);
        Assert.Equal("original-name", collection.Slug);
        Assert.Equal(0, repository.UpdateCalls);
    }

    [Fact]
    public async Task RenameCollection_WhenCollectionBelongsToAnotherUser_LeavesItUnchanged()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(9, "Other user's collection", "desc");
        SetId(collection, 42);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(collection.Id, "Changed name"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("Other user's collection", collection.Name);
        Assert.Equal("other-users-collection", collection.Slug);
        Assert.Equal(0, repository.UpdateCalls);
    }

    [Fact]
    public async Task RenameCollection_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(0);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(42, "New Name"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticatedCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateCollection_WhenRepositoryThrowsDomainException_ReturnsDomainError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        ThrowingCollectionRepository repository = new ThrowingCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        ThrowingUnitOfWorkFactory unitOfWorkFactory = new ThrowingUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("My Collection", "desc", true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task RenameCollection_WhenRepositoryThrowsDomainException_ReturnsDomainError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        ThrowingCollectionRepository repository = new ThrowingCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        ThrowingUnitOfWorkFactory unitOfWorkFactory = new ThrowingUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(42, "Better Name"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task ChangeCollectionVisibility_WhenRepositoryThrowsDomainException_ReturnsDomainError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        ThrowingCollectionRepository repository = new ThrowingCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        ThrowingUnitOfWorkFactory unitOfWorkFactory = new ThrowingUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.ChangeCollectionVisibility(new CollectionChangeVisibilityRequest(42, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteCollection_WhenRepositoryThrowsDomainException_ReturnsDomainError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        ThrowingCollectionRepository repository = new ThrowingCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        ThrowingUnitOfWorkFactory unitOfWorkFactory = new ThrowingUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(42));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task ChangeCollectionVisibility_WhenCollectionDoesNotBelongToUser_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection otherUserCollection = Collection.Create(9, "Other User Collection", null);
        SetId(otherUserCollection, 99);
        repository.Items.Add(otherUserCollection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.ChangeCollectionVisibility(new CollectionChangeVisibilityRequest(otherUserCollection.Id, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("COLLECTION_VISIBILITY_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.False(otherUserCollection.IsPublic);
        Assert.Equal(0, repository.UpdateCalls);
    }

    [Fact]
    public async Task ChangeCollectionVisibility_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(0);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.ChangeCollectionVisibility(new CollectionChangeVisibilityRequest(42, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticatedCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteCollection_WhenResourceIsNotAccessible_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(9, "Other User Collection", null);
        SetId(collection, 99);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(repository.Items[0].Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("COLLECTION_DELETE_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.Contains(collection, repository.Items);
        Assert.Equal(0, repository.DeleteCalls);
    }

    [Fact]
    public async Task DeleteCollection_WhenIdIsInvalid_ReturnsValidationWithoutDeleting()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(currentUser.UserId, "Keep this collection", null);
        SetId(collection, 12);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("COLLECTION_ID_INVALID", result.Errors[0].Code);
        Assert.Contains(collection, repository.Items);
        Assert.Equal(0, repository.DeleteCalls);
    }

    [Fact]
    public async Task DeleteCollection_WhenCollectionHasLinks_ReturnsDomainError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Owned", null);
        SetId(collection, 12);
        repository.Items.Add(collection);

        Link link = Link.Create(7, collection, "https://example.com", null, null, null, false);
        SetId(link, 91);
        linkRepository.Links.Add(link);

        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(collection.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("COLLECTION_HAS_LINKS", result.Errors[0].Code);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork!.BeginCount);
        Assert.Equal(0, unitOfWorkFactory.LastUnitOfWork.CommitCount);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork.RollbackCount);
        Assert.Contains(collection, repository.Items);
        Assert.Contains(link, linkRepository.Links);
    }

    [Fact]
    public async Task DeleteCollection_WhenItIsTheLastCollection_ReturnsDomainError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Only Collection", null);
        SetId(collection, 8);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(collection.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("LAST_COLLECTION_CANNOT_BE_DELETED", result.Errors[0].Code);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork!.BeginCount);
        Assert.Equal(0, unitOfWorkFactory.LastUnitOfWork.CommitCount);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork.RollbackCount);
        Assert.Contains(collection, repository.Items);
    }

    [Fact]
    public async Task DeleteCollection_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(0);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(42));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticatedCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task ListCollections_UsesUserScopeAndPaging()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection alpha = Collection.Create(7, "Alpha", null);
        Collection beta = Collection.Create(7, "Beta", null);
        Collection gamma = Collection.Create(3, "Gamma", null);
        SetId(alpha, 1);
        SetId(beta, 2);
        SetId(gamma, 3);
        repository.Items.Add(alpha);
        repository.Items.Add(beta);
        repository.Items.Add(gamma);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<IReadOnlyList<CollectionDto>> result = await service.ListCollections(page: 1);

        Assert.True(result.IsSuccess);
        IReadOnlyList<CollectionDto> list = result.Value!;
        Assert.Equal(2, list.Count);
        Assert.All(list, item => Assert.Equal(7, currentUser.UserId));
    }

    [Fact]
    public async Task ListAllCollections_WhenUserHasCollections_ReturnsAllOwnedCollections()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection alpha = Collection.Create(7, "Alpha", null);
        Collection beta = Collection.Create(7, "Beta", null);
        Collection gamma = Collection.Create(3, "Gamma", null);
        SetId(alpha, 1);
        SetId(beta, 2);
        SetId(gamma, 3);
        repository.Items.Add(alpha);
        repository.Items.Add(beta);
        repository.Items.Add(gamma);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<IReadOnlyCollection<CollectionDto>> result = await service.ListAllCollections();

        Assert.True(result.IsSuccess);
        IReadOnlyCollection<CollectionDto> list = result.Value!;
        Assert.Equal(2, list.Count);
        Assert.Contains(list, item => item.Name == "Alpha");
        Assert.Contains(list, item => item.Name == "Beta");
    }

    [Fact]
    public async Task ListAllCollections_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(0);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<IReadOnlyCollection<CollectionDto>> result = await service.ListAllCollections();

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticatedCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task ListCollectionsPage_ReturnsItemsAndPageCountForCurrentUser()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        repository.Items.Add(Collection.Create(7, "Alpha", null));
        repository.Items.Add(Collection.Create(7, "Beta", null));
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<Linkubator.Application.Dtos.Models.CollectionPageDto> result = await service.ListCollectionsPage(page: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Page);
        Assert.Equal(1, result.Value.PageCount);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task ListCollections_WhenPageIsInvalid_ReturnsValidationError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<IReadOnlyList<CollectionDto>> result = await service.ListCollections(page: 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("PAGE_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task ListCollections_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(0);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<IReadOnlyList<CollectionDto>> result = await service.ListCollections();

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticatedCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateCollection_WhenNameIsBlank_ReturnsValidationError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("   ", "desc", false));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("COLLECTION_NAME_REQUIRED", result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateCollection_WhenNameCannotProduceSlug_DoesNotInsertCollection()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("💫"));

        Assert.False(result.IsSuccess);
        Assert.Empty(repository.Items);
    }
    [Fact]
    public async Task CreateCollection_WhenNameIsAlreadyUsed_DoesNotInsertAnotherCollection()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection existing = Collection.Create(currentUser.UserId, "Same name", null);
        SetId(existing, 12);
        repository.Items.Add(existing);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("Same name"));

        Assert.False(result.IsSuccess);
        Assert.Single(repository.Items);
        Assert.Same(existing, repository.Items[0]);
    }

    [Fact]
    public async Task CreateCollection_WhenNameIsBlank_DoesNotInsertCollection()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.CreateCollection(new CollectionCreateRequest("   "));

        Assert.False(result.IsSuccess);
        Assert.Empty(repository.Items);
    }

    [Fact]
    public async Task RenameCollection_WhenDuplicateSlugExists_ReturnsDomainError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection alpha = Collection.Create(7, "Alpha", "desc");
        Collection beta = Collection.Create(7, "Beta", "desc");
        SetId(alpha, 1);
        SetId(beta, 2);
        repository.Items.Add(alpha);
        repository.Items.Add(beta);

        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(beta.Id, "Alpha"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("COLLECTION_NAME_IN_USE", result.Errors[0].Code);
        Assert.Equal($"{CollectionErrors.NameInUseMessage}: {alpha.Name}.", result.Errors[0].Message);
        Assert.Equal("Beta", beta.Name);
        Assert.Equal("beta", beta.Slug);
        Assert.Equal(0, repository.UpdateCalls);
    }
    [Fact]
    public async Task RenameCollection_WhenCollectionDoesNotExist_DoesNotUpdateAnyCollection()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection unrelated = Collection.Create(currentUser.UserId, "Unrelated", null);
        SetId(unrelated, 99);
        repository.Items.Add(unrelated);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(42, "Changed"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("Unrelated", unrelated.Name);
        Assert.Equal(0, repository.UpdateCalls);
    }

    [Fact]
    public async Task RenameCollection_WhenCollectionIdIsInvalid_ReturnsValidationError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(0, ""));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("COLLECTION_ID_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task RenameCollection_WhenNameMatchesCurrentCollection_AllowsItWithoutDuplicateError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Alpha", "desc");
        SetId(collection, 12);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(collection.Id, "alpha"));

        Assert.True(result.IsSuccess);
        Assert.Equal("alpha", result.Value!.Name);
        Assert.Equal("alpha", result.Value.Slug);
    }

    [Fact]
    public async Task ChangeCollectionVisibility_WhenCollectionIdIsInvalid_ReturnsValidationError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.ChangeCollectionVisibility(new CollectionChangeVisibilityRequest(0, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("COLLECTION_ID_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteCollection_WhenCollectionIdIsInvalid_ReturnsValidationError()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("COLLECTION_ID_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateCollection_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new CurrentUser(7);
        UnexpectedCollectionRepository repository = new UnexpectedCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new UnexpectedUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCollection(new CollectionCreateRequest("My Collection", "desc", true)));
    }

    [Fact]
    public async Task RenameCollection_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new CurrentUser(7);
        UnexpectedCollectionRepository repository = new UnexpectedCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new UnexpectedUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RenameCollection(new CollectionRenameRequest(42, "Better Name")));
    }

    [Fact]
    public async Task ChangeCollectionVisibility_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new CurrentUser(7);
        UnexpectedCollectionRepository repository = new UnexpectedCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new UnexpectedUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeCollectionVisibility(new CollectionChangeVisibilityRequest(42, true)));
    }

    [Fact]
    public async Task DeleteCollection_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new CurrentUser(7);
        UnexpectedCollectionRepository repository = new UnexpectedCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new UnexpectedUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteCollection(new CollectionDeleteRequest(42)));
    }

    [Fact]
    public async Task ListCollections_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new CurrentUser(7);
        UnexpectedCollectionRepository repository = new UnexpectedCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new UnexpectedUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ListCollections());
    }

    [Fact]
    public async Task RenameCollection_WhenCollectionDoesNotExist_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.RenameCollection(new CollectionRenameRequest(999, "Alpha"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("COLLECTION_RENAME_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteCollection_WhenCollectionDoesNotExist_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(999));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("COLLECTION_DELETE_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.Empty(repository.Items);
        Assert.Equal(0, repository.DeleteCalls);
    }

    [Fact]
    public async Task DeleteCollection_WhenCollectionIsNotAccessible_RollsBackUnitOfWork()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(999));

        Assert.False(result.IsSuccess);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork!.BeginCount);
        Assert.Equal(0, unitOfWorkFactory.LastUnitOfWork.CommitCount);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork.RollbackCount);
    }

    [Fact]
    public async Task DeleteCollection_WhenAllowed_CommitsExactlyOnce()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Collection to delete", null);
        Collection remaining = Collection.Create(7, "Remaining collection", null);
        SetId(collection, 1);
        SetId(remaining, 2);
        repository.Items.Add(collection);
        repository.Items.Add(remaining);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result result = await service.DeleteCollection(new CollectionDeleteRequest(collection.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork!.BeginCount);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork.CommitCount);
        Assert.Equal(0, unitOfWorkFactory.LastUnitOfWork.RollbackCount);
        Assert.DoesNotContain(repository.Items, item => item.Id == collection.Id);
    }

    [Fact]
    public async Task ChangeCollectionVisibility_WhenOwnedAndValid_UpdatesPrivacy()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Visible Collection", "desc", isPublic: false);
        SetId(collection, 8);
        repository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.ChangeCollectionVisibility(new CollectionChangeVisibilityRequest(collection.Id, true));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsPublic);
    }

    [Fact]
    public async Task ChangeCollectionVisibility_WhenMadePublic_KeepsLinksPrivate()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Private collection", null, isPublic: false);
        SetId(collection, 8);
        repository.Items.Add(collection);
        Link privateLink = Link.Create(7, collection, "https://example.com/private", "Private", null, null, isPublic: false);
        Link alsoPrivateLink = Link.Create(7, collection, "https://example.com/private-2", "Private 2", null, null, isPublic: false);
        SetId(privateLink, 91);
        SetId(alsoPrivateLink, 92);
        linkRepository.Links.Add(privateLink);
        linkRepository.Links.Add(alsoPrivateLink);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.ChangeCollectionVisibility(new CollectionChangeVisibilityRequest(collection.Id, true));

        Assert.True(result.IsSuccess);
        Assert.True(collection.IsPublic);
        Assert.False(privateLink.IsPublic);
        Assert.False(alsoPrivateLink.IsPublic);
        Assert.Equal(2, linkRepository.Links.Count);
        Assert.Empty(linkRepository.UpdatedLinks);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork!.CommitCount);
    }

    [Fact]
    public async Task ChangeCollectionVisibility_WhenMadePrivate_PersistsCollectionAndAllLinksInSameUnitOfWork()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection collection = Collection.Create(7, "Public collection", null, isPublic: true);
        SetId(collection, 8);
        repository.Items.Add(collection);
        Link publicLink = Link.Create(7, collection, "https://example.com/public", null, null, null, isPublic: true);
        Link privateLink = Link.Create(7, collection, "https://example.com/private", null, null, null, isPublic: false);
        SetId(publicLink, 91);
        SetId(privateLink, 92);
        linkRepository.Links.Add(publicLink);
        linkRepository.Links.Add(privateLink);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<CollectionDto> result = await service.ChangeCollectionVisibility(new CollectionChangeVisibilityRequest(collection.Id, false));

        Assert.True(result.IsSuccess);
        Assert.False(collection.IsPublic);
        Assert.False(publicLink.IsPublic);
        Assert.False(privateLink.IsPublic);
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork!.BeginCount);
        Assert.Equal(1, repository.UpdateCalls);
        Assert.Equal(1, linkRepository.UpdateCalls);
        Assert.Equal(publicLink, linkRepository.UpdatedLinks.Single());
        Assert.Equal(1, unitOfWorkFactory.LastUnitOfWork.CommitCount);
        Assert.Equal(0, unitOfWorkFactory.LastUnitOfWork.RollbackCount);
    }

    [Fact]
    public async Task ListCollections_WhenQueryIsProvided_FiltersByName()
    {
        CurrentUser currentUser = new CurrentUser(7);
        InMemoryCollectionRepository repository = new InMemoryCollectionRepository();
        InMemoryLinkRepository linkRepository = new InMemoryLinkRepository();
        Collection alpha = Collection.Create(7, "Alpha", null);
        Collection zulu = Collection.Create(7, "Zulu", null);
        SetId(alpha, 1);
        SetId(zulu, 2);
        repository.Items.Add(alpha);
        repository.Items.Add(zulu);
        FakeUnitOfWorkFactory unitOfWorkFactory = new FakeUnitOfWorkFactory(repository, linkRepository);
        CollectionService service = new CollectionService(NullLogger<CollectionService>.Instance, currentUser, repository, unitOfWorkFactory);

        Result<IReadOnlyList<CollectionDto>> result = await service.ListCollections("  al  ");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Alpha", result.Value![0].Name);
        Assert.Equal(currentUser.UserId, repository.LastUserId);
        Assert.Equal("al", repository.LastQuery);
        Assert.Equal(1, repository.LastPage);
    }

    [Fact]
    public void Collection_RehydrateLinks_WhenLinksBelongToCollection_UpdatesBackReferencesAndAllowsPrivacyChange()
    {
        Collection collection = Collection.Create(7, "Owned collection", null, isPublic: true);
        SetId(collection, 11);

        Link first = Link.Create(7, collection, "https://example.com/a", null, null, null, isPublic: true);
        Link second = Link.Create(7, collection, "https://example.com/b", null, null, null, isPublic: true);
        SetId(first, 21);
        SetId(second, 22);

        Collection rehydrated = Collection.Create(7, "Owned collection", null, isPublic: true);
        SetId(rehydrated, 11);
        rehydrated.RehydrateLinks([first, second]);

        rehydrated.ChangePrivacy(false, [first, second]);

        Assert.Equal(2, rehydrated.Links.Count);
        Assert.All(rehydrated.Links, link => Assert.False(link.IsPublic));
    }

    [Fact]
    public void CollectionValidation_WhenNameIsBlank_ReturnsRequiredError()
    {
        Result result = CollectionValidations.ValidateName("   ");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("COLLECTION_NAME_REQUIRED", result.Errors[0].Code);
    }

    [Fact]
    public void CollectionErrors_AllFactoryMethods_ReturnExpectedMetadata()
    {
        Assert.Equal("REQUEST_REQUIRED", CollectionErrors.RequestRequired().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, CollectionErrors.RequestRequired().Category);
        Assert.Equal("request", CollectionErrors.RequestRequired().Field);

        Assert.Equal("NOT_AUTHENTICATED", CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal("NOT_AUTHENTICATED", CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal("NOT_AUTHENTICATED", CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal("NOT_AUTHENTICATED", CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal("NOT_AUTHENTICATED", CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal("COLLECTION_NAME_REQUIRED", CollectionErrors.NameRequired().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, CollectionErrors.NameRequired().Category);
        Assert.Equal("Name", CollectionErrors.NameRequired().Field);

        Assert.Equal("COLLECTION_NAME_IN_USE", CollectionErrors.NameInUse().Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, CollectionErrors.NameInUse().Category);
        Assert.Equal("Name", CollectionErrors.NameInUse().Field);

        Assert.Equal("COLLECTION_RENAME_NOT_ACCESSIBLE", CollectionErrors.RenameNotAccessible().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CollectionErrors.RenameNotAccessible().Category);
        Assert.Equal("CollectionId", CollectionErrors.RenameNotAccessible().Field);

        Assert.Equal("COLLECTION_VISIBILITY_NOT_ACCESSIBLE", CollectionErrors.VisibilityNotAccessible().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CollectionErrors.VisibilityNotAccessible().Category);
        Assert.Equal("CollectionId", CollectionErrors.VisibilityNotAccessible().Field);

        Assert.Equal("COLLECTION_DELETE_NOT_ACCESSIBLE", CollectionErrors.DeleteNotAccessible().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CollectionErrors.DeleteNotAccessible().Category);
        Assert.Equal("CollectionId", CollectionErrors.DeleteNotAccessible().Field);

        Assert.Equal("COLLECTION_HAS_LINKS", CollectionErrors.HasLinks().Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, CollectionErrors.HasLinks().Category);
        Assert.Equal("CollectionId", CollectionErrors.HasLinks().Field);

        Assert.Equal("LAST_COLLECTION_CANNOT_BE_DELETED", CollectionErrors.LastCollectionCannotBeDeleted().Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, CollectionErrors.LastCollectionCannotBeDeleted().Category);
        Assert.Equal("CollectionId", CollectionErrors.LastCollectionCannotBeDeleted().Field);

        Assert.Equal("PAGE_INVALID", CollectionErrors.PageInvalid().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, CollectionErrors.PageInvalid().Category);
        Assert.Equal("page", CollectionErrors.PageInvalid().Field);
    }

    private sealed class UnexpectedCollectionRepository : ICollectionRepository
    {
        public Task<Collection?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromException<Collection?>(new InvalidOperationException("boom"));

        public Task<Collection?> GetBySlug(long userId, string slug, CancellationToken cancellationToken = default)
            => Task.FromException<Collection?>(new InvalidOperationException("boom"));

        public Task<IReadOnlyCollection<Collection>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<Collection>>(new InvalidOperationException("boom"));

        public Task<IReadOnlyCollection<CollectionDto>> GetPageByUser(long userId, string? query, int page, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<CollectionDto>>(new InvalidOperationException("boom"));

        public Task<int> GetPageCountByUser(long userId, string? query, CancellationToken cancellationToken = default)
            => Task.FromException<int>(new InvalidOperationException("boom"));

        public Task Insert(Collection collection, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("boom"));

        public Task Update(Collection collection, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("boom"));

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("boom"));
    }

    private sealed class UnexpectedUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly ILinkRepository _linkRepository;

        public UnexpectedUnitOfWorkFactory(ICollectionRepository collectionRepository, ILinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public IUnitOfWork Get(string contextName)
            => new UnexpectedUnitOfWork(_collectionRepository, _linkRepository);
    }

    private sealed class UnexpectedUnitOfWork : IUnitOfWork
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly ILinkRepository _linkRepository;

        public UnexpectedUnitOfWork(ICollectionRepository collectionRepository, ILinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public TRepository GetRepository<TRepository>() where TRepository : class
            => typeof(TRepository) == typeof(ICollectionRepository)
                ? (TRepository)(object)_collectionRepository
                : (TRepository)(object)_linkRepository;

        public Task Begin(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Commit(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Rollback(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class AcceptAllGetCollectionValidator : IValidator<CollectionGetRequest>
    {
        public Result Validate(CollectionGetRequest request) => Result.Success();
    }

    private sealed class AcceptAllCreateCollectionValidator : IValidator<CollectionCreateRequest>
    {
        public Result Validate(CollectionCreateRequest request) => Result.Success();
    }

    private sealed class AcceptAllRenameCollectionValidator : IValidator<CollectionRenameRequest>
    {
        public Result Validate(CollectionRenameRequest request) => Result.Success();
    }

    private sealed class AcceptAllChangeCollectionVisibilityValidator : IValidator<CollectionChangeVisibilityRequest>
    {
        public Result Validate(CollectionChangeVisibilityRequest request) => Result.Success();
    }

    private sealed class AcceptAllDeleteCollectionValidator : IValidator<CollectionDeleteRequest>
    {
        public Result Validate(CollectionDeleteRequest request) => Result.Success();
    }

    private sealed class ThrowingCollectionRepository : ICollectionRepository
    {
        public Task<Collection?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromException<Collection?>(new DomainExceptions.NotValidInputException());

        public Task<Collection?> GetBySlug(long userId, string slug, CancellationToken cancellationToken = default)
            => Task.FromException<Collection?>(new DomainExceptions.NotValidInputException());

        public Task<IReadOnlyCollection<Collection>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<Collection>>(new DomainExceptions.NotValidInputException());

        public Task<IReadOnlyCollection<CollectionDto>> GetPageByUser(long userId, string? query, int page, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<CollectionDto>>(new DomainExceptions.NotValidInputException());

        public Task<int> GetPageCountByUser(long userId, string? query, CancellationToken cancellationToken = default)
            => Task.FromException<int>(new DomainExceptions.NotValidInputException());

        public Task Insert(Collection collection, CancellationToken cancellationToken = default)
            => Task.FromException(new DomainExceptions.NotValidInputException());

        public Task Update(Collection collection, CancellationToken cancellationToken = default)
            => Task.FromException(new DomainExceptions.NotValidInputException());

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromException(new DomainExceptions.NotValidInputException());
    }

    private sealed class ThrowingUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly ILinkRepository _linkRepository;

        public ThrowingUnitOfWorkFactory(ICollectionRepository collectionRepository, ILinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public IUnitOfWork Get(string contextName)
            => new ThrowingUnitOfWork(_collectionRepository, _linkRepository);
    }

    private sealed class ThrowingUnitOfWork : IUnitOfWork
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly ILinkRepository _linkRepository;

        public ThrowingUnitOfWork(ICollectionRepository collectionRepository, ILinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public TRepository GetRepository<TRepository>() where TRepository : class
            => typeof(TRepository) == typeof(ICollectionRepository)
                ? (TRepository)(object)_collectionRepository
                : (TRepository)(object)_linkRepository;

        public Task Begin(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Commit(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Rollback(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }

    private sealed class InMemoryCollectionRepository : ICollectionRepository
    {
        public List<Collection> Items { get; } = [];
        public int UpdateCalls { get; private set; }
        public long LastUserId { get; private set; }
        public string? LastQuery { get; private set; }
        public int LastPage { get; private set; }
        public int DeleteCalls { get; private set; }

        public Task<Collection?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Id == id && item.UserId == userId));

        public Task<Collection?> GetBySlug(long userId, string slug, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.UserId == userId && item.Slug == slug));

        public Task<IReadOnlyCollection<Collection>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Collection>>(Items.Where(item => item.UserId == userId).ToList());

        public Task<IReadOnlyCollection<CollectionDto>> GetPageByUser(long userId, string? query, int page, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            LastQuery = query;
            LastPage = page;
            return Task.FromResult<IReadOnlyCollection<CollectionDto>>(
                Items
                    .Where(item => item.UserId == userId)
                    .Where(item => string.IsNullOrWhiteSpace(query) || item.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Skip((page - 1) * 10)
                    .Take(10)
                    .Select(item => new CollectionDto(item.Id, item.Name, item.Slug, item.Description, item.IsPublic, item.Links.Count, item.CreatedAt))
                    .ToList());
        }

        public Task<int> GetPageCountByUser(long userId, string? query, CancellationToken cancellationToken = default)
            => Task.FromResult(
                (int)Math.Ceiling((double)Items.Count(item => item.UserId == userId && (string.IsNullOrWhiteSpace(query) || item.Name.Contains(query, StringComparison.OrdinalIgnoreCase))) / 10));

        public Task Insert(Collection collection, CancellationToken cancellationToken = default)
        {
            if (collection.Id == 0)
            {
                long id = Items.Count == 0 ? 1 : Items.Max(item => item.Id) + 1;
                typeof(Collection).GetProperty(nameof(Collection.Id))!.SetValue(collection, id);
            }

            Items.Add(collection);
            return Task.CompletedTask;
        }

        public Task Update(Collection collection, CancellationToken cancellationToken = default)
        {
            UpdateCalls++;
            int index = Items.FindIndex(item => item.Id == collection.Id && item.UserId == collection.UserId);
            if (index >= 0)
            {
                Items[index] = collection;
            }

            return Task.CompletedTask;
        }

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
        {
            DeleteCalls++;
            Collection? target = Items.FirstOrDefault(item => item.Id == id && item.UserId == userId);
            if (target is not null)
            {
                Items.Remove(target);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryLinkRepository : ILinkRepository
    {
        public List<Link> Links { get; } = [];
        public List<Link> UpdatedLinks { get; } = [];
        public int UpdateCalls { get; private set; }

        public Task<Link?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<Link?>(null);

        public Task<Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
            => Task.FromResult<Link?>(null);

        public Task<IReadOnlyCollection<Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Link>>(Array.Empty<Link>());

        public Task<IReadOnlyCollection<Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Link>>(
                Links
                    .Where(item => item.UserId == userId && item.CollectionId == collectionId)
                    .ToList());

        public Task<IReadOnlyCollection<LinkDto>> GetPageByUser(long userId, string? query, long? collectionId, int page, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<LinkDto>>(Array.Empty<LinkDto>());

        public Task<int> GetPageCountByUser(long userId, string? query, long? collectionId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task Insert(Link link, CancellationToken cancellationToken = default)
        {
            Links.Add(link);
            return Task.CompletedTask;
        }

        public Task Update(Link link, CancellationToken cancellationToken = default)
        {
            UpdateCalls++;
            UpdatedLinks.Add(link);
            return Task.CompletedTask;
        }

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class ThrowIfCalledOutsideTransactionCollectionRepository : ICollectionRepository
    {
        public int GetBySlugCalls { get; private set; }

        public Task<Collection?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<Collection?>(null);

        public Task<Collection?> GetBySlug(long userId, string slug, CancellationToken cancellationToken = default)
        {
            GetBySlugCalls++;
            throw new InvalidOperationException("GetBySlug was called outside the transaction.");
        }

        public Task<IReadOnlyCollection<Collection>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Collection>>(Array.Empty<Collection>());

        public Task<IReadOnlyCollection<CollectionDto>> GetPageByUser(long userId, string? query, int page, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CollectionDto>>(Array.Empty<CollectionDto>());

        public Task<int> GetPageCountByUser(long userId, string? query, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task Insert(Collection collection, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Update(Collection collection, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class TransactionalUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly InMemoryCollectionRepository _collectionRepository;
        private readonly InMemoryLinkRepository _linkRepository;

        public TransactionalUnitOfWorkFactory(InMemoryCollectionRepository collectionRepository, InMemoryLinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public IUnitOfWork Get(string contextName)
            => new TransactionalUnitOfWork(_collectionRepository, _linkRepository);
    }

    private sealed class TransactionalUnitOfWork : IUnitOfWork
    {
        private readonly InMemoryCollectionRepository _collectionRepository;
        private readonly InMemoryLinkRepository _linkRepository;

        public TransactionalUnitOfWork(InMemoryCollectionRepository collectionRepository, InMemoryLinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public TRepository GetRepository<TRepository>() where TRepository : class
            => typeof(TRepository) == typeof(ICollectionRepository)
                ? (TRepository)(object)_collectionRepository
                : (TRepository)(object)_linkRepository;

        public Task Begin(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Commit(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Rollback(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }

    private static void SetId(Collection collection, long id)
        => typeof(Collection).GetProperty(nameof(Collection.Id))!.SetValue(collection, id);

    private static void SetId(Link link, long id)
        => typeof(Link).GetProperty(nameof(Link.Id))!.SetValue(link, id);

    private sealed class FakeUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly InMemoryCollectionRepository _collectionRepository;
        private readonly InMemoryLinkRepository _linkRepository;

        public FakeUnitOfWorkFactory(InMemoryCollectionRepository collectionRepository, InMemoryLinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public FakeUnitOfWork? LastUnitOfWork { get; private set; }

        public IUnitOfWork Get(string contextName)
        {
            LastUnitOfWork = new FakeUnitOfWork(_collectionRepository, _linkRepository);
            return LastUnitOfWork;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        private readonly InMemoryCollectionRepository _collectionRepository;
        private readonly InMemoryLinkRepository _linkRepository;

        public FakeUnitOfWork(InMemoryCollectionRepository collectionRepository, InMemoryLinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }
        public int BeginCount { get; private set; }

        public TRepository GetRepository<TRepository>() where TRepository : class
            => typeof(TRepository) == typeof(ICollectionRepository)
                ? (TRepository)(object)_collectionRepository
                : (TRepository)(object)_linkRepository;

        public Task Begin(CancellationToken cancellationToken = default)
        {
            BeginCount++;
            return Task.CompletedTask;
        }

        public Task Commit(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.CompletedTask;
        }

        public Task Rollback(CancellationToken cancellationToken = default)
        {
            RollbackCount++;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            RollbackIfTransactionActive();
        }

        public ValueTask DisposeAsync()
        {
            RollbackIfTransactionActive();
            return ValueTask.CompletedTask;
        }

        private void RollbackIfTransactionActive()
        {
            if (BeginCount > CommitCount + RollbackCount)
            {
                RollbackCount++;
            }
        }
    }
}
