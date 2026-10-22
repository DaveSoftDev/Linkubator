using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Enums;
using Linkubator.Application.Models;
using Linkubator.Application.Results;
using Linkubator.Application.UseCases.Links;
using Linkubator.Application.Validation.Links;
using Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Tests.Application;

public class LinkServiceTests
{
    [Fact]
    public void Constructor_WhenDependenciesAreNull_ThrowsArgumentNullException()
    {
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        CurrentUser currentUser = new(7);

        Assert.Throws<ArgumentNullException>(() => new LinkService(null!, currentUser, collectionRepository, linkRepository, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new LinkService(NullLogger<LinkService>.Instance, null!, collectionRepository, linkRepository, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new LinkService(NullLogger<LinkService>.Instance, currentUser, null!, linkRepository, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new LinkService(NullLogger<LinkService>.Instance, currentUser, collectionRepository, null!, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new LinkService(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, null!));
    }

    [Fact]
    public async Task GetLink_WhenUserIsUnauthenticated_ReturnsNotAuthenticated()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        LinkService service = CreateService(currentUser, collectionRepository, linkRepository);

        Result<LinkDto> result = await service.GetLink(new LinkGetRequest(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(CommonErrors.NotAuthenticated().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task GetLink_WhenLinkIdIsInvalid_ReturnsValidationError()
    {
        LinkService service = CreateService(new CurrentUser(7), new InMemoryCollectionRepository(), new InMemoryLinkRepository());

        Result<LinkDto> result = await service.GetLink(new LinkGetRequest(0));

        Assert.False(result.IsSuccess);
        Assert.Equal(LinkErrors.LinkIdInvalid().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task GetLink_WhenLinkBelongsToCurrentUser_ReturnsLink()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "My Collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.test/link", "Title", "Description", null, true);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        LinkService service = CreateService(currentUser, collectionRepository, linkRepository);

        Result<LinkDto> result = await service.GetLink(new LinkGetRequest(link.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(link.Id, result.Value!.Id);
        Assert.Equal(link.Title, result.Value.Title);
        Assert.True(result.Value.IsPublic);
    }

    [Fact]
    public async Task GetLink_WhenLinkBelongsToAnotherUser_ReturnsNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(8, "Other Collection", null);
        SetId(collection, 10);
        Link link = Link.Create(8, collection, "https://example.test/link", null, null, null);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        LinkService service = CreateService(currentUser, collectionRepository, linkRepository);

        Result<LinkDto> result = await service.GetLink(new LinkGetRequest(link.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(LinkErrors.LinkNotAccessible().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateLink_WhenRequestIsValid_ReturnsCreatedLink()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "My Collection", "desc");
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com/path?x=1"));

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.CollectionId);
        Assert.Equal("https://example.com/path?x=1", result.Value.UrlOriginal);
        Assert.False(result.Value.IsPublic);
        Assert.Single(linkRepository.Items);
    }

    [Fact]
    public async Task CreateLink_WhenUrlIsInvalid_DoesNotInsertLink()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "My Collection", null);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "javascript:alert(1)"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("UrlSchemeNotAllowed", result.Errors[0].Code);
        Assert.Empty(linkRepository.Items);
    }

    [Fact]
    public async Task CreateLink_WhenImageIsInvalid_DoesNotInsertLink()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "My Collection", null);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com", Image: "file:///invalid.png"));

        Assert.False(result.IsSuccess);
        Assert.Empty(linkRepository.Items);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("UrlSchemeNotAllowed", result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateLink_WhenUrlAlreadyExists_ReportsExistingCollection()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Existing collection", null);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        linkRepository.Items.Add(Link.Create(7, collection, "https://example.com", null, null, null));
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com/"));

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_DUPLICATE_URL", result.Errors[0].Code);
        Assert.Contains("Existing collection", result.Errors[0].Message);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Single(linkRepository.Items);
    }

    [Fact]
    public async Task CreateLink_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticated().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateLink_WhenRequestIsNull_ReturnsRequestRequiredBeforeAuthentication()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", result.Errors[0].Code);
    }

    [Fact]
    public async Task CreateLink_WhenCollectionIsNotAccessible_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(9, "Other collection", null);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.Empty(linkRepository.Items);
    }

    [Fact]
    public async Task CreateLink_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new(7);
        UnexpectedCollectionRepository collectionRepository = new();
        UnexpectedLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateLink(new LinkCreateRequest(10, "https://example.com")));
    }

    [Fact]
    public async Task CreateLink_WhenInsertThrowsAfterAwait_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        AsyncThrowingLinkRepository linkRepository = new() { ThrowOnInsert = true };
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateLink(new LinkCreateRequest(10, "https://example.com/path")));
    }

    [Fact]
    public async Task CreateLink_WhenDomainExceptionIsThrown_ReturnsDomainFailure()
    {
        CurrentUser currentUser = new(7);
        ThrowingCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task EditLink_WhenRequestIsNull_ReturnsRequestRequired()
    {
        LinkService service = CreateService(new(7), new(), new());

        Result<LinkDto> result = await service.EditLink(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", result.Errors[0].Code);
    }

    [Fact]
    public async Task EditLink_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 10, "Title"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticated().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task EditLink_WhenLinkIdIsInvalid_ReturnsValidationError()
    {
        LinkService service = CreateService(new(7), new(), new());

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(0, 0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("LINK_ID_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task EditLink_WhenLinkDoesNotExist_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(99, 10, "Title"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task EditLink_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        UnexpectedLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EditLink(new LinkEditRequest(20, 10, "Title")));
    }

    [Fact]
    public async Task EditLink_WhenUpdateThrowsAfterAwait_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        AsyncThrowingLinkRepository linkRepository = new() { ThrowOnUpdate = true };
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EditLink(new LinkEditRequest(20, 10, "Updated title")));
    }

    [Fact]
    public async Task EditLink_WhenDomainExceptionIsThrown_ReturnsDomainFailure()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        ThrowingLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 10, "Title"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task MoveLink_WhenLinkIdIsInvalid_ReturnsValidationError()
    {
        LinkService service = CreateService(new(7), new(), new());

        Result<LinkDto> result = await service.MoveLink(new LinkMoveRequest(0, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("LINK_ID_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenLinkIdIsInvalid_ReturnsValidationError()
    {
        LinkService service = CreateService(new(7), new(), new());

        Result<LinkDto> result = await service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(0, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("LINK_ID_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task MoveLink_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.MoveLink(new LinkMoveRequest(20, 10));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticated().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task MoveLink_WhenRequestIsNull_ReturnsRequestRequired()
    {
        LinkService service = CreateService(new(7), new(), new());

        Result<LinkDto> result = await service.MoveLink(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", result.Errors[0].Code);
    }

    [Fact]
    public async Task MoveLink_WhenLinkDoesNotExist_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection destination = Collection.Create(7, "Destination", null, true);
        SetId(destination, 11);
        collectionRepository.Items.Add(destination);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.MoveLink(new LinkMoveRequest(99, 11));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task MoveLink_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        UnexpectedLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.MoveLink(new LinkMoveRequest(20, 10)));
    }

    [Fact]
    public async Task MoveLink_WhenUpdateThrowsAfterAwait_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        Collection original = Collection.Create(7, "Original", null, true);
        Collection destination = Collection.Create(7, "Destination", null, true);
        SetId(original, 10);
        SetId(destination, 11);
        collectionRepository.Items.Add(original);
        collectionRepository.Items.Add(destination);
        AsyncThrowingLinkRepository linkRepository = new() { ThrowOnUpdate = true };
        Link link = Link.Create(7, original, "https://example.com/path", "Original title", "Original description", "https://example.com/image.png", true);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.MoveLink(new LinkMoveRequest(20, 11)));
    }

    [Fact]
    public async Task MoveLink_WhenDomainExceptionIsThrown_ReturnsDomainFailure()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        ThrowingLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.MoveLink(new LinkMoveRequest(20, 10));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticated().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenRequestIsNull_ReturnsRequestRequired()
    {
        LinkService service = CreateService(new(7), new(), new());

        Result<LinkDto> result = await service.ChangeLinkVisibility(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", result.Errors[0].Code);
    }

    [Fact]
    public async Task MoveLink_WhenLinkDoesNotBelongToUser_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection originalCollection = Collection.Create(9, "Other user's collection", null, true);
        Collection destinationCollection = Collection.Create(7, "Destination", null, true);
        SetId(originalCollection, 10);
        SetId(destinationCollection, 11);
        collectionRepository.Items.Add(originalCollection);
        collectionRepository.Items.Add(destinationCollection);
        Link link = Link.Create(9, originalCollection, "https://example.com/path", null, null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.MoveLink(new LinkMoveRequest(20, 11));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenLinkDoesNotBelongToUser_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(9, "Other user's collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(9, collection, "https://example.com/path", null, null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.False(link.IsPublic);
        Assert.Empty(linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        UnexpectedLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, true)));
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenUpdateThrowsAfterAwait_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        AsyncThrowingLinkRepository linkRepository = new() { ThrowOnUpdate = true };
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, true)));
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenDomainExceptionIsThrown_ReturnsDomainFailure()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        ThrowingLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteLink_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(new LinkDeleteRequest(20));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticated().Code, result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteLink_WhenRequestIsNull_ReturnsRequestRequired()
    {
        LinkService service = CreateService(new(7), new(), new());

        Result result = await service.DeleteLink(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", result.Errors[0].Code);
    }

    [Fact]
    public async Task EditLink_WhenOwnedAndValid_UpdatesFieldsWithoutChangingBaseUrl()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "My Collection", "desc", true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Old", "Old desc", "https://example.com/old.png", true);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 7, "New title", "New desc", "https://example.com/new.png"));

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.com/path", result.Value!.UrlOriginal);
        Assert.Equal("New title", result.Value.Title);
        Assert.Equal("New desc", result.Value.Description);
        Assert.Equal("https://example.com/new.png", result.Value.Image);
    }

    [Fact]
    public async Task EditLink_WhenPublishingInPrivateCollectionAndImageIsInvalid_LeavesLinkUnchanged()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Private collection", null, false);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Original", "Original description", null);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 7,"Changed", "Changed description", "file:///invalid.png", true));

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_PUBLIC_IN_PRIVATE_COLLECTION", result.Errors[0].Code);
        Assert.Equal("Original", link.Title);
        Assert.Equal("Original description", link.Description);
        Assert.False(link.IsPublic);
        Assert.Empty(linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task EditLink_WhenPublishingInPrivateCollectionWithOtherValidChanges_LeavesLinkUnchanged()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Private collection", null);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Original", "Original description", null);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 10, "Changed", "Changed description", IsPublic: true));

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_PUBLIC_IN_PRIVATE_COLLECTION", result.Errors[0].Code);
        Assert.Equal("Original", link.Title);
        Assert.Equal("Original description", link.Description);
        Assert.False(link.IsPublic);
        Assert.Empty(linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task EditLink_WhenLinkBelongsToAnotherUser_LeavesItUnchanged()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(9, "Other user's collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(9, collection, "https://example.com/path", "Original", "Original description", null);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 10, "Changed"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("Original", link.Title);
        Assert.Equal("Original description", link.Description);
        Assert.False(link.IsPublic);
        Assert.Empty(linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task EditLink_WhenImageIsInvalidInPublicCollection_LeavesLinkUnchanged()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Public collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Original", "Original description", "https://example.com/original.png", false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 10, "Changed", "Changed description", "file:///invalid.png"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Original", link.Title);
        Assert.Equal("Original description", link.Description);
        Assert.Equal("https://example.com/original.png", link.Image);
        Assert.Empty(linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task ListLinks_WhenCollectionIdBelongsToUser_ReturnsLinks()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link first = Link.Create(7, collection, "https://example.com/one", "One", null, null, false);
        Link second = Link.Create(7, collection, "https://example.com/two", "Two", null, null, true);
        SetId(first, 20);
        SetId(second, 21);
        linkRepository.Items.Add(first);
        linkRepository.Items.Add(second);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(collectionId: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.All(result.Value, item => Assert.Equal(10, item.CollectionId));
    }

    [Fact]
    public async Task ListLinks_WhenQueryIsProvided_UsesTrimmedQuery()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link first = Link.Create(7, collection, "https://example.com/one", "One", null, null, false);
        Link second = Link.Create(7, collection, "https://example.com/two", "Two", null, null, true);
        SetId(first, 20);
        SetId(second, 21);
        linkRepository.Items.Add(first);
        linkRepository.Items.Add(second);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(query: "  one  ", collectionId: 10, page: 2);

        Assert.True(result.IsSuccess);
        Assert.Equal("one", linkRepository.LastQuery);
        Assert.Equal(10, linkRepository.LastCollectionId);
        Assert.Equal(2, linkRepository.LastPage);
        Assert.Equal(currentUser.UserId, linkRepository.LastUserId);
    }

    [Fact]
    public async Task ListLinks_WhenCollectionDoesNotBelongToUser_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(9, "Other user collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(collectionId: 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteLink_WhenOwnedAndValid_RemovesLink()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", "Description", null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(new LinkDeleteRequest(20));

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(linkRepository.Items, item => item.Id == 20);
    }

    [Fact]
    public async Task DeleteLink_WhenLinkBelongsToAnotherUser_ReturnsSameNotAccessibleErrorWithoutMutation()
    {
        CurrentUser currentUser = new(8);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(9, "Other user's collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(9, collection, "https://example.com/path", "Title", null, null);
        SetId(link, 21);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(new LinkDeleteRequest(21));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_DELETE_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.Equal("LINK_NOT_ACCESSIBLE", LinkErrors.LinkNotAccessible().Code);
        Assert.Contains(link, linkRepository.Items);
        Assert.Single(linkRepository.Items);
        Assert.Equal(0, linkRepository.DeleteCalls);
    }

    [Fact]
    public async Task DeleteLink_WhenIdIsInvalid_ReturnsValidationWithoutDeleting()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(currentUser.UserId, "Collection", null);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(currentUser.UserId, collection, "https://example.com/keep", "Keep", null, null);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(new LinkDeleteRequest(0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("LINK_ID_INVALID", result.Errors[0].Code);
        Assert.Contains(link, linkRepository.Items);
        Assert.Equal(0, linkRepository.DeleteCalls);
    }

    [Theory]
    [InlineData("img/logo.png")]
    [InlineData("//cdn.example.com/img/logo.png")]
    public async Task CreateLink_WhenImageUsesRelativeReference_IsAccepted(string image)
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "My Collection", "desc", true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com/path", Image: image));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Image);
        if (image.StartsWith("//", StringComparison.Ordinal))
        {
            Assert.StartsWith("https://cdn.example.com", result.Value.Image);
        }
        else
        {
            Assert.StartsWith("https://example.com", result.Value.Image);
        }
        Assert.EndsWith("logo.png", result.Value.Image);
        Assert.Single(linkRepository.Items);
    }

    [Fact]
    public async Task CreateLink_WhenMakingPublicLinkInPrivateCollection_ReturnsStableDomainError()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Private collection", null, false);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com/path", IsPublic: true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("LINK_PUBLIC_IN_PRIVATE_COLLECTION", result.Errors[0].Code);
        Assert.Equal("IsPublic", result.Errors[0].Field);
        Assert.Empty(linkRepository.Items);
    }

    [Fact]
    public async Task CreateLink_WhenPublicLinkIsCreatedInPublicCollection_ReturnsCreatedLink()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Public collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com/public", IsPublic: true));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsPublic);
        Assert.Equal(10, result.Value.CollectionId);
        Assert.Single(linkRepository.Items);
    }

    [Fact]
    public async Task MoveLink_WhenOwnedAndDestinationIsValid_UpdatesCollection()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection original = Collection.Create(7, "Original", null, true);
        Collection destination = Collection.Create(7, "Destination", null, true);
        SetId(original, 10);
        SetId(destination, 11);
        collectionRepository.Items.Add(original);
        collectionRepository.Items.Add(destination);
        Link link = Link.Create(7, original, "https://example.com/path", "Original title", "Original description", "https://example.com/image.png", true);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.MoveLink(new LinkMoveRequest(20, 11));

        Assert.True(result.IsSuccess);
        Assert.Equal(11, result.Value!.CollectionId);
        Assert.Equal(11, link.CollectionId);
        Assert.Equal(link.Id, result.Value.Id);
        Assert.Equal("Original title", result.Value.Title);
        Assert.Equal("Original description", result.Value.Description);
        Assert.Equal("https://example.com/image.png", result.Value.Image);
        Assert.False(result.Value.IsPublic);
        Assert.Contains(link, linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task MoveLink_WhenDestinationCollectionDoesNotBelongToUser_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Mine", null, true);
        Collection other = Collection.Create(9, "Other", null, true);
        SetId(collection, 10);
        SetId(other, 11);
        collectionRepository.Items.Add(collection);
        collectionRepository.Items.Add(other);
        Link link = Link.Create(7, collection, "https://example.com/path", null, null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.MoveLink(new LinkMoveRequest(20, 11));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.Equal(10, link.CollectionId);
        Assert.False(link.IsPublic);
        Assert.Empty(linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task CreateLink_WhenUrlAlreadyExists_DoesNotMutateCollectionLinks()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Existing collection", null);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link existing = Link.Create(7, collection, "https://example.com", null, null, null);
        SetId(existing, 20);
        linkRepository.Items.Add(existing);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com/"));

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_DUPLICATE_URL", result.Errors[0].Code);
        Assert.Single(collection.Links);
        Assert.Contains(existing, collection.Links);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenMakingPublicLinkInPrivateCollection_ReturnsStableDomainError()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Private collection", null, false);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", null, null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, true));

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_PUBLIC_IN_PRIVATE_COLLECTION", result.Errors[0].Code);
        Assert.False(link.IsPublic);
        Assert.Empty(linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task ListLinks_WhenPageIsInvalid_ReturnsValidationError()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(page: 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("PAGE_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task ListLinks_WhenCollectionIdIsInvalid_ReturnsValidationError()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(collectionId: 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_ID_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task ListLinks_WhenCollectionIdDoesNotBelongToUser_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(9, "Other user's collection", null);
        SetId(collection, 42);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(collectionId: 42);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteLink_WhenIdIsInvalid_ReturnsValidationError()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(new LinkDeleteRequest(0));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("LINK_ID_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteLink_WhenLinkDoesNotExist_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(new LinkDeleteRequest(99));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_DELETE_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.Equal(0, linkRepository.DeleteCalls);
    }

    [Fact]
    public async Task DeleteLink_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        UnexpectedLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteLink(new LinkDeleteRequest(20)));
    }

    [Fact]
    public async Task DeleteLink_WhenDeleteThrowsAfterAwait_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        AsyncThrowingLinkRepository linkRepository = new() { ThrowOnDelete = true };
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteLink(new LinkDeleteRequest(20)));
    }

    [Fact]
    public async Task DeleteLink_WhenDomainExceptionIsThrown_ReturnsDomainFailure()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        ThrowingLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(new LinkDeleteRequest(20));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task ListLinks_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks();

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal(CommonErrors.NotAuthenticatedCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task ListLinks_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        UnexpectedLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ListLinks());
    }

    [Fact]
    public async Task ListLinks_WhenRepositoryThrowsDomainException_ReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        ThrowingLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<DomainExceptions.NotValidInputException>(() => service.ListLinks());
    }

    [Fact]
    public void LinkServiceValidators_UsesCustomAndDefaultValidators()
    {
        StubValidator<LinkGetRequest> getValidator = new();
        StubValidator<LinkCreateRequest> createValidator = new();
        StubValidator<LinkEditRequest> editValidator = new();
        StubValidator<LinkMoveRequest> moveValidator = new();
        StubValidator<LinkChangeVisibilityRequest> changeVisibilityValidator = new();
        StubValidator<LinkDeleteRequest> deleteValidator = new();

        LinkServiceValidators configured = new(
            getValidator,
            createValidator,
            editValidator,
            moveValidator,
            changeVisibilityValidator,
            deleteValidator);

        Assert.Same(getValidator, configured.GetLinkValidator);
        Assert.Same(createValidator, configured.CreateLinkValidator);
        Assert.Same(editValidator, configured.EditLinkValidator);
        Assert.Same(moveValidator, configured.MoveLinkValidator);
        Assert.Same(changeVisibilityValidator, configured.ChangeLinkVisibilityValidator);
        Assert.Same(deleteValidator, configured.DeleteLinkValidator);

        LinkServiceValidators defaults = new();
        Assert.NotNull(defaults.GetLinkValidator);
        Assert.NotNull(defaults.CreateLinkValidator);
        Assert.NotNull(defaults.EditLinkValidator);
        Assert.NotNull(defaults.MoveLinkValidator);
        Assert.NotNull(defaults.ChangeLinkVisibilityValidator);
        Assert.NotNull(defaults.DeleteLinkValidator);

        Result createResult = defaults.CreateLinkValidator.Validate(new LinkCreateRequest(1, "https://example.com"));
        Result editResult = defaults.EditLinkValidator.Validate(new LinkEditRequest(1, 1, "Title"));
        Result moveResult = defaults.MoveLinkValidator.Validate(new LinkMoveRequest(1, 1));
        Result visibilityResult = defaults.ChangeLinkVisibilityValidator.Validate(new LinkChangeVisibilityRequest(1, true));
        Result deleteResult = defaults.DeleteLinkValidator.Validate(new LinkDeleteRequest(1));

        Assert.True(createResult.IsSuccess);
        Assert.True(editResult.IsSuccess);
        Assert.True(moveResult.IsSuccess);
        Assert.True(visibilityResult.IsSuccess);
        Assert.True(deleteResult.IsSuccess);
    }

    [Fact]
    public async Task CreateLink_WhenDuplicateCollectionIsMissing_ReportsDuplicateUrlWithoutCollectionName()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Collection duplicateCollection = Collection.Create(7, "Duplicate collection", null, true);
        SetId(duplicateCollection, 99);
        Link duplicate = Link.Create(7, duplicateCollection, "https://example.com", "Title", null, null, false);
        SetId(duplicate, 100);
        linkRepository.Items.Add(duplicate);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new LinkCreateRequest(10, "https://example.com/"));

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_DUPLICATE_URL", result.Errors[0].Code);
        Assert.DoesNotContain("Duplicate collection", result.Errors[0].Message);
    }

    [Fact]
    public async Task EditLink_WhenVisibilityIsExplicitlySetOnPublicCollection_UpdatesPrivacy()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Public collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 10, "Title", null, null, true));

        Assert.True(result.IsSuccess);
        Assert.True(link.IsPublic);
        Assert.Contains(link, linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenMakingPrivateOnPublicCollection_ChangesVisibility()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Public collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, true);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, false));

        Assert.True(result.IsSuccess);
        Assert.False(link.IsPublic);
        Assert.Contains(link, linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenMakingPublicOnPublicCollection_ChangesVisibility()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Public collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, true));

        Assert.True(result.IsSuccess);
        Assert.True(link.IsPublic);
        Assert.Contains(link, linkRepository.UpdatedItems);
    }

    [Fact]
    public async Task DeleteLink_WhenRepositoryThrowsAfterLookup_RollsBackAndReThrows()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        ThrowingDeleteLinkRepository failingRepository = new(linkRepository);
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, failingRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteLink(new LinkDeleteRequest(20)));
    }

    [Fact]
    public async Task EditLink_WhenCollectionIsMissing_ReturnsCollectionNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(new LinkEditRequest(20, 10, "Updated title"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task ChangeLinkVisibility_WhenCollectionIsMissing_ReturnsCollectionNotAccessible()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "Owned collection", null, true);
        SetId(collection, 10);
        Link link = Link.Create(7, collection, "https://example.com/path", "Title", null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.ChangeLinkVisibility(new LinkChangeVisibilityRequest(20, true));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task ListLinks_WhenCollectionRepositoryThrowsUnexpectedException_ReThrows()
    {
        CurrentUser currentUser = new(7);
        UnexpectedCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        UnexpectedUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ListLinks(collectionId: 10));
    }

    [Fact]
    public void LinkServiceValidators_RecordHasExpectedValueSemantics()
    {
        LinkServiceValidators first = new();
        LinkServiceValidators second = new();
        LinkServiceValidators custom = new(
            new StubValidator<LinkGetRequest>(),
            new StubValidator<LinkCreateRequest>(),
            new StubValidator<LinkEditRequest>(),
            new StubValidator<LinkMoveRequest>(),
            new StubValidator<LinkChangeVisibilityRequest>(),
            new StubValidator<LinkDeleteRequest>());

        Assert.NotNull(first.GetLinkValidator);
        Assert.NotNull(first.CreateLinkValidator);
        Assert.NotNull(first.EditLinkValidator);
        Assert.NotNull(first.MoveLinkValidator);
        Assert.NotNull(first.ChangeLinkVisibilityValidator);
        Assert.NotNull(first.DeleteLinkValidator);
        Assert.NotNull(second.CreateLinkValidator);
        Assert.Same(first.CreateLinkValidator.GetType(), second.CreateLinkValidator.GetType());
        Assert.Same(first.EditLinkValidator.GetType(), second.EditLinkValidator.GetType());
        Assert.Same(first.DeleteLinkValidator.GetType(), second.DeleteLinkValidator.GetType());
        Assert.Same(custom.GetLinkValidator.GetType(), typeof(StubValidator<LinkGetRequest>));
        Assert.Same(custom.CreateLinkValidator.GetType(), typeof(StubValidator<LinkCreateRequest>));
        Assert.Same(custom.EditLinkValidator.GetType(), typeof(StubValidator<LinkEditRequest>));
        Assert.Same(custom.DeleteLinkValidator.GetType(), typeof(StubValidator<LinkDeleteRequest>));
        Assert.Contains("LinkServiceValidators", first.ToString());
    }

    [Fact]
    public void LinkErrors_AllFactoryMethods_ReturnExpectedMetadata()
    {
        Assert.Equal(CommonErrors.RequestRequiredCode, LinkErrors.RequestRequired().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, LinkErrors.RequestRequired().Category);
        Assert.Equal("request", LinkErrors.RequestRequired().Field);

        Assert.Equal(CommonErrors.NotAuthenticatedCode, CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal(CommonErrors.NotAuthenticatedCode, CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal(CommonErrors.NotAuthenticatedCode, CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal(CommonErrors.NotAuthenticatedCode, CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal(CommonErrors.NotAuthenticatedCode, CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal(CommonErrors.NotAuthenticatedCode, CommonErrors.NotAuthenticated().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, CommonErrors.NotAuthenticated().Category);
        Assert.Equal("UserId", CommonErrors.NotAuthenticated().Field);

        Assert.Equal("LINK_ID_INVALID", LinkErrors.LinkIdInvalid().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, LinkErrors.LinkIdInvalid().Category);
        Assert.Equal("LinkId", LinkErrors.LinkIdInvalid().Field);

        Assert.Equal("LINK_DESTINATION_COLLECTION_ID_INVALID", LinkErrors.DestinationCollectionIdInvalid().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, LinkErrors.DestinationCollectionIdInvalid().Category);
        Assert.Equal("DestinationCollectionId", LinkErrors.DestinationCollectionIdInvalid().Field);

        Assert.Equal("LINK_COLLECTION_ID_INVALID", LinkErrors.CollectionIdInvalid().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, LinkErrors.CollectionIdInvalid().Category);
        Assert.Equal("CollectionId", LinkErrors.CollectionIdInvalid().Field);

        Assert.Equal("URL_REQUIRED", LinkErrors.UrlRequired().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, LinkErrors.UrlRequired().Category);
        Assert.Equal("Url", LinkErrors.UrlRequired().Field);

        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", LinkErrors.CollectionNotAccessible().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, LinkErrors.CollectionNotAccessible().Category);
        Assert.Equal("CollectionId", LinkErrors.CollectionNotAccessible().Field);

        Assert.Equal("LINK_NOT_ACCESSIBLE", LinkErrors.LinkNotAccessible().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, LinkErrors.LinkNotAccessible().Category);
        Assert.Equal("LinkId", LinkErrors.LinkNotAccessible().Field);

        Assert.Equal("LINK_DELETE_NOT_ACCESSIBLE", LinkErrors.DeleteNotAccessible().Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, LinkErrors.DeleteNotAccessible().Category);
        Assert.Equal("LinkId", LinkErrors.DeleteNotAccessible().Field);

        Assert.Equal("LINK_PUBLIC_IN_PRIVATE_COLLECTION", LinkErrors.PublicLinkInPrivateCollection().Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, LinkErrors.PublicLinkInPrivateCollection().Category);
        Assert.Equal("IsPublic", LinkErrors.PublicLinkInPrivateCollection().Field);

        Assert.Equal("LINK_DUPLICATE_URL", LinkErrors.DuplicateUrl().Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, LinkErrors.DuplicateUrl().Category);
        Assert.Equal("Url", LinkErrors.DuplicateUrl().Field);

        ApplicationError withCollectionName = LinkErrors.DuplicateUrl("My Collection");
        Assert.Equal("LINK_DUPLICATE_URL", withCollectionName.Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, withCollectionName.Category);
        Assert.Equal("Url", withCollectionName.Field);
        Assert.Contains("My Collection", withCollectionName.Message);

        Assert.Equal("PAGE_INVALID", LinkErrors.PageInvalid().Code);
        Assert.Equal(ApplicationErrorCategory.Validation, LinkErrors.PageInvalid().Category);
        Assert.Equal("page", LinkErrors.PageInvalid().Field);
    }

    private static LinkService CreateService(CurrentUser currentUser, InMemoryCollectionRepository collectionRepository, InMemoryLinkRepository linkRepository)
    {
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        return new LinkService(NullLogger<LinkService>.Instance, currentUser, collectionRepository, linkRepository, unitOfWorkFactory);
    }

    private sealed class StubValidator<TRequest> : IValidator<TRequest>
    {
        public Result Validate(TRequest request)
            => Result.Success();
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

    private sealed class ThrowingLinkRepository : ILinkRepository
    {
        public Task<Link?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromException<Link?>(new DomainExceptions.NotValidInputException());

        public Task<Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
            => Task.FromException<Link?>(new DomainExceptions.NotValidInputException());

        public Task<IReadOnlyCollection<Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<Link>>(new DomainExceptions.NotValidInputException());

        public Task<IReadOnlyCollection<Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<Link>>(new DomainExceptions.NotValidInputException());

        public Task<IReadOnlyCollection<LinkDto>> GetPageByUser(long userId, string? query, long? collectionId, int page, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<LinkDto>>(new DomainExceptions.NotValidInputException());

        public Task<int> GetPageCountByUser(long userId, string? query, long? collectionId, CancellationToken cancellationToken = default)
            => Task.FromException<int>(new DomainExceptions.NotValidInputException());

        public Task Insert(Link link, CancellationToken cancellationToken = default)
            => Task.FromException(new DomainExceptions.NotValidInputException());

        public Task Update(Link link, CancellationToken cancellationToken = default)
            => Task.FromException(new DomainExceptions.NotValidInputException());

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromException(new DomainExceptions.NotValidInputException());
    }

    private sealed class ThrowingDeleteLinkRepository : ILinkRepository
    {
        private readonly ILinkRepository _inner;

        public ThrowingDeleteLinkRepository(ILinkRepository inner)
        {
            _inner = inner;
        }

        public Task<Link?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => _inner.GetById(id, userId, cancellationToken);

        public Task<Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
            => _inner.GetByNormalizedUrl(userId, normalizedUrl, cancellationToken);

        public Task<IReadOnlyCollection<Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => _inner.GetAllByUser(userId, cancellationToken);

        public Task<IReadOnlyCollection<Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
            => _inner.GetAllByCollection(userId, collectionId, cancellationToken);

        public Task<IReadOnlyCollection<LinkDto>> GetPageByUser(long userId, string? query, long? collectionId, int page, CancellationToken cancellationToken = default)
            => _inner.GetPageByUser(userId, query, collectionId, page, cancellationToken);

        public Task<int> GetPageCountByUser(long userId, string? query, long? collectionId, CancellationToken cancellationToken = default)
            => _inner.GetPageCountByUser(userId, query, collectionId, cancellationToken);

        public Task Insert(Link link, CancellationToken cancellationToken = default)
            => _inner.Insert(link, cancellationToken);

        public Task Update(Link link, CancellationToken cancellationToken = default)
            => _inner.Update(link, cancellationToken);

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("boom"));
    }

    private sealed class AsyncThrowingLinkRepository : ILinkRepository
    {
        public List<Link> Items { get; } = [];
        public bool ThrowOnInsert { get; set; }
        public bool ThrowOnUpdate { get; set; }
        public bool ThrowOnDelete { get; set; }

        public Task<Link?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Id == id && item.UserId == userId));

        public Task<Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.UserId == userId && item.UrlNormalized == normalizedUrl));

        public Task<IReadOnlyCollection<Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Link>>(Items.Where(item => item.UserId == userId).ToList());

        public Task<IReadOnlyCollection<Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Link>>(Items.Where(item => item.UserId == userId && item.CollectionId == collectionId).ToList());

        public Task<IReadOnlyCollection<LinkDto>> GetPageByUser(long userId, string? query, long? collectionId, int page, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<LinkDto>>(Items.Where(item => item.UserId == userId).Select(item => new LinkDto(item.Id, item.CollectionId, item.UrlOriginal, item.UrlNormalized, item.Title, item.Description, item.Image, item.IsPublic, item.CreatedAt)).ToList());

        public Task<int> GetPageCountByUser(long userId, string? query, long? collectionId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Count(item => item.UserId == userId));

        public async Task Insert(Link link, CancellationToken cancellationToken = default)
        {
            if (ThrowOnInsert)
            {
                await Task.Yield();
                throw new InvalidOperationException("boom");
            }

            Items.Add(link);
        }

        public async Task Update(Link link, CancellationToken cancellationToken = default)
        {
            if (ThrowOnUpdate)
            {
                await Task.Yield();
                throw new InvalidOperationException("boom");
            }
        }

        public async Task Delete(long id, long userId, CancellationToken cancellationToken = default)
        {
            if (ThrowOnDelete)
            {
                await Task.Yield();
                throw new InvalidOperationException("boom");
            }
        }
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

    private sealed class UnexpectedLinkRepository : ILinkRepository
    {
        public Task<Link?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromException<Link?>(new InvalidOperationException("boom"));

        public Task<Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
            => Task.FromException<Link?>(new InvalidOperationException("boom"));

        public Task<IReadOnlyCollection<Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<Link>>(new InvalidOperationException("boom"));

        public Task<IReadOnlyCollection<Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<Link>>(new InvalidOperationException("boom"));

        public Task<IReadOnlyCollection<LinkDto>> GetPageByUser(long userId, string? query, long? collectionId, int page, CancellationToken cancellationToken = default)
            => Task.FromException<IReadOnlyCollection<LinkDto>>(new InvalidOperationException("boom"));

        public Task<int> GetPageCountByUser(long userId, string? query, long? collectionId, CancellationToken cancellationToken = default)
            => Task.FromException<int>(new InvalidOperationException("boom"));

        public Task Insert(Link link, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("boom"));

        public Task Update(Link link, CancellationToken cancellationToken = default)
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

    private sealed class InMemoryCollectionRepository : ICollectionRepository
    {
        public List<Collection> Items { get; } = [];

        public Task<Collection?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Id == id && item.UserId == userId));

        public Task<Collection?> GetBySlug(long userId, string slug, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.UserId == userId && item.Slug == slug));

        public Task<IReadOnlyCollection<Collection>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Collection>>(Items.Where(item => item.UserId == userId).ToList());

        public Task<IReadOnlyCollection<CollectionDto>> GetPageByUser(long userId, string? query, int page, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CollectionDto>>(Array.Empty<CollectionDto>());

        public Task<int> GetPageCountByUser(long userId, string? query, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task Insert(Collection collection, CancellationToken cancellationToken = default)
        {
            Items.Add(collection);
            return Task.CompletedTask;
        }

        public Task Update(Collection collection, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryLinkRepository : ILinkRepository
    {
        public List<Link> Items { get; } = [];
        public List<Link> UpdatedItems { get; } = [];
        public int DeleteCalls { get; private set; }
        public long LastUserId { get; private set; }
        public string? LastQuery { get; private set; }
        public long? LastCollectionId { get; private set; }
        public int LastPage { get; private set; }

        public Task<Link?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Id == id && item.UserId == userId));

        public Task<Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.UserId == userId && item.UrlNormalized == normalizedUrl));

        public Task<IReadOnlyCollection<Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Link>>(Items.Where(item => item.UserId == userId).ToList());

        public Task<IReadOnlyCollection<Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Link>>(Items.Where(item => item.UserId == userId && item.CollectionId == collectionId).ToList());

        public Task<IReadOnlyCollection<LinkDto>> GetPageByUser(long userId, string? query, long? collectionId, int page, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            LastQuery = query;
            LastCollectionId = collectionId;
            LastPage = page;
            return Task.FromResult<IReadOnlyCollection<LinkDto>>(Items.Where(item => item.UserId == userId)
                .Select(Map)
                .ToList());
        }

        public Task<int> GetPageCountByUser(long userId, string? query, long? collectionId, CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task Insert(Link link, CancellationToken cancellationToken = default)
        {
            Items.Add(link);
            return Task.CompletedTask;
        }

        public Task Update(Link link, CancellationToken cancellationToken = default)
        {
            UpdatedItems.Add(link);
            return Task.CompletedTask;
        }

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
        {
            DeleteCalls++;
            Link? existing = Items.FirstOrDefault(item => item.Id == id && item.UserId == userId);
            if (existing is not null)
            {
                Items.Remove(existing);
            }

            return Task.CompletedTask;
        }

        private static LinkDto Map(Link link)
            => new(link.Id, link.CollectionId, link.UrlOriginal, link.UrlNormalized, link.Title, link.Description, link.Image, link.IsPublic, link.CreatedAt);
    }

    private sealed class FakeUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly InMemoryCollectionRepository _collectionRepository;
        private readonly InMemoryLinkRepository _linkRepository;

        public FakeUnitOfWorkFactory(InMemoryCollectionRepository collectionRepository, InMemoryLinkRepository linkRepository)
        {
            _collectionRepository = collectionRepository;
            _linkRepository = linkRepository;
        }

        public IUnitOfWork Get(string contextName)
            => new FakeUnitOfWork(_collectionRepository, _linkRepository);
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

    private static void SetId(Collection collection, long id)
        => typeof(Collection).GetProperty(nameof(Collection.Id))!.SetValue(collection, id);

    private static void SetId(Link link, long id)
        => typeof(Link).GetProperty(nameof(Link.Id))!.SetValue(link, id);
}
