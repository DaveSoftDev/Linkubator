using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Enums;
using Linkubator.Application.Models;
using Linkubator.Application.Results;
using Linkubator.Application.UseCases.Links;
using Linkubator.Domain.Entities;

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

        Assert.Throws<ArgumentNullException>(() => new LinkService(null!, collectionRepository, linkRepository, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new LinkService(currentUser, null!, linkRepository, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new LinkService(currentUser, collectionRepository, null!, unitOfWorkFactory));
        Assert.Throws<ArgumentNullException>(() => new LinkService(currentUser, collectionRepository, linkRepository, null!));
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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new CreateLinkRequest(10, "https://example.com/path?x=1"));

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.CollectionId);
        Assert.Equal("https://example.com/path?x=1", result.Value.UrlOriginal);
        Assert.Single(linkRepository.Items);
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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new CreateLinkRequest(10, "https://example.com/"));

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_DUPLICATE_URL", result.Errors[0].Code);
        Assert.Contains("Existing collection", result.Errors[0].Message);
        Assert.Single(linkRepository.Items);
    }

    [Fact]
    public async Task CreateLink_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        CurrentUser currentUser = new(0);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new CreateLinkRequest(10, "https://example.com"));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_CREATE_NOT_AUTHENTICATED", result.Errors[0].Code);
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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(20, "New title", "New desc", "https://example.com/new.png");

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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.EditLink(20, "Changed", "Changed description", "file:///invalid.png", true);

        Assert.False(result.IsSuccess);
        Assert.Equal("LINK_PUBLIC_IN_PRIVATE_COLLECTION", result.Errors[0].Code);
        Assert.Equal("Original", link.Title);
        Assert.Equal("Original description", link.Description);
        Assert.False(link.IsPublic);
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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(collectionId: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.All(result.Value, item => Assert.Equal(10, item.CollectionId));
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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(20);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(linkRepository.Items, item => item.Id == 20);
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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new CreateLinkRequest(10, "https://example.com/path", Image: image));

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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new CreateLinkRequest(10, "https://example.com/path", IsPublic: true));

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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new CreateLinkRequest(10, "https://example.com/public", IsPublic: true));

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
        Link link = Link.Create(7, original, "https://example.com/path", null, null, null, false);
        SetId(link, 20);
        linkRepository.Items.Add(link);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.MoveLink(20, 11);

        Assert.True(result.IsSuccess);
        Assert.Equal(11, result.Value!.CollectionId);
        Assert.Equal(11, link.CollectionId);
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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.MoveLink(20, 11);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
        Assert.Equal(10, link.CollectionId);
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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.CreateLink(new CreateLinkRequest(10, "https://example.com/"));

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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<LinkDto> result = await service.ChangeLinkVisibility(20, true);

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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

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
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(collectionId: 42);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task DeleteLink_WhenIdIsInvalid_ReturnsCommonResourceNotAccessibleError()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory);

        Result result = await service.DeleteLink(0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("LINK_DELETE_NOT_ACCESSIBLE", result.Errors[0].Code);
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

        public Task<Link?> GetById(long id, long userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.Id == id && item.UserId == userId));

        public Task<Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(item => item.UserId == userId && item.UrlNormalized == normalizedUrl));

        public Task<IReadOnlyCollection<Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Link>>(Items.Where(item => item.UserId == userId).ToList());

        public Task<IReadOnlyCollection<Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Link>>(Items.Where(item => item.UserId == userId && item.CollectionId == collectionId).ToList());

        public Task<IReadOnlyCollection<LinkDto>> GetPageByUser(long userId, string? query, long? collectionId, int page, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<LinkDto>>(Items.Where(item => item.UserId == userId)
                .Select(Map)
                .ToList());

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
