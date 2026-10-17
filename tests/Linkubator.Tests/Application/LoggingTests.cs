using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Logging;
using Linkubator.Application.Models;
using Linkubator.Application.Results;
using Linkubator.Application.UseCases.Collections;
using Linkubator.Application.UseCases.Dashboard;
using Linkubator.Application.UseCases.Links;
using Linkubator.Application.UseCases.PublicReads;
using Linkubator.Domain.Entities;
using Linkubator.Domain.Exceptions;
using Linkubator.Tests.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Linkubator.Tests.Application;

public sealed class LoggingTests
{
    [Fact]
    public async Task LinkService_CreateLink_LogsOnlySafeOperationMetadata()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "My Collection", "desc");
        EntityTestFactory.SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        TestLogger<LinkService> logger = new();
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory, logger: logger);

        Result<LinkDto> result = await service.CreateLink(new CreateLinkRequest(10, "https://example.com/path?token=super-secret-value"));

        Assert.True(result.IsSuccess);
        LogEntry entry = Assert.Single(logger.Entries, item => item.Level == LogLevel.Information);
        Assert.Equal("LinkService", entry.Category);
        Assert.Contains("Link created", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("super-secret-value", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("super-secret-value", string.Join(" ", entry.Values.Values), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", string.Join(" ", entry.Values.Values), StringComparison.OrdinalIgnoreCase);
        Assert.Null(entry.Exception);
        Assert.True(entry.Values.ContainsKey("UserId"));
    }

    [Fact]
    public async Task LinkService_ListLinks_LogsRepositoryFailureWithoutQueryOrException()
    {
        const string searchSentinel = "private-search-sentinel";
        InvalidOperationException repositoryFailure = new($"Failure containing {searchSentinel}");
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new() { PageFailure = repositoryFailure };
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        TestLogger<LinkService> logger = new();
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory, logger: logger);

        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ListLinks(searchSentinel));

        Assert.Same(repositoryFailure, thrown);
        LogEntry entry = Assert.Single(logger.Entries, item => item.Level == LogLevel.Error);
        Assert.Contains("Link listing failed", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Same(repositoryFailure, entry.Exception);
        Assert.DoesNotContain(searchSentinel, entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(searchSentinel, string.Join(" ", entry.Values.Values), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CollectionService_ChangeVisibility_LogsSafeVisibilityMetadata()
    {
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        Collection collection = Collection.Create(7, "My Collection", "desc", isPublic: true);
        EntityTestFactory.SetId(collection, 10);
        collectionRepository.Items.Add(collection);
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        TestLogger<CollectionService> logger = new();
        CollectionService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory, logger: logger);

        Result<CollectionDto> result = await service.ChangeCollectionVisibility(10, false);

        Assert.True(result.IsSuccess);
        LogEntry entry = Assert.Single(logger.Entries, item => item.Level == LogLevel.Information);
        Assert.Contains("Collection visibility changed", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public async Task LinkService_ListLinks_LogsValidationWithoutSearchText()
    {
        const string searchSentinel = "private-search-sentinel";
        CurrentUser currentUser = new(7);
        InMemoryCollectionRepository collectionRepository = new();
        InMemoryLinkRepository linkRepository = new();
        FakeUnitOfWorkFactory unitOfWorkFactory = new(collectionRepository, linkRepository);
        TestLogger<LinkService> logger = new();
        LinkService service = new(currentUser, collectionRepository, linkRepository, unitOfWorkFactory, logger: logger);

        Result<IReadOnlyList<LinkDto>> result = await service.ListLinks(searchSentinel, page: 0);

        Assert.False(result.IsSuccess);
        LogEntry entry = Assert.Single(logger.Entries, item => item.Level == LogLevel.Warning);
        Assert.Contains("validation failed", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(searchSentinel, entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(searchSentinel, string.Join(" ", entry.Values.Values), StringComparison.Ordinal);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public void TestLogger_CapturesStructuredStateAndAttachedException()
    {
        TestLogger<LinkService> logger = new();
        InvalidOperationException exception = new("diagnostic exception");

        logger.LogError(exception, "Repository failed for {UserId}", 7L);

        LogEntry entry = Assert.Single(logger.Entries);
        Assert.Equal(7L, entry.Values["UserId"]);
        Assert.Same(exception, entry.Exception);
        Assert.Contains("Repository failed", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SafeLogger_Log_PreservesDomainExceptionTraceability()
    {
        TestLogger<LinkService> logger = new();
        NotValidInputException exception = new();

        SafeLogger.Log(logger, LogLevel.Warning, exception, "Domain validation failed", new { UserId = 7L });

        LogEntry entry = Assert.Single(logger.Entries, item => item.Level == LogLevel.Warning);
        Assert.Equal("Domain validation failed", entry.Message);
        Assert.Equal("NotValidInput", entry.Values["DomainErrorCode"]);
        Assert.Equal(nameof(NotValidInputException), entry.Values["ExceptionType"]);
        Assert.False(entry.Values.ContainsKey("ExceptionMessage"));
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public async Task DashboardService_GetDashboard_LogsSuccessfulRead()
    {
        TestLogger<DashboardService> logger = new();
        DashboardService service = new(new CurrentUser(7), new FakeDashboardRepository(), logger);

        Result<DashboardDto> result = await service.GetDashboard();

        Assert.True(result.IsSuccess);
        LogEntry entry = Assert.Single(logger.Entries, item => item.Level == LogLevel.Information);
        Assert.Contains("Dashboard loaded", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(7L, entry.Values["UserId"]);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public async Task PublicReadService_GetUserPage_LogsNoRawAlias()
    {
        const string alias = "public-alias-sentinel";
        TestLogger<PublicReadService> logger = new();
        PublicReadService service = new(new FakePublicReadRepository(), logger);

        Result<PublicUserDto> result = await service.GetUserPage(alias);

        Assert.True(result.IsSuccess);
        LogEntry entry = Assert.Single(logger.Entries, item => item.Level == LogLevel.Information);
        Assert.Contains("Public user page loaded", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(alias, entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(alias, string.Join(" ", entry.Values.Values), StringComparison.Ordinal);
        Assert.Null(entry.Exception);
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Dictionary<string, object?> values = [];
            if (state is IEnumerable<KeyValuePair<string, object?>> keyValuePairs)
            {
                foreach (KeyValuePair<string, object?> pair in keyValuePairs)
                {
                    values[pair.Key] = pair.Value;
                }
            }

            Entries.Add(new LogEntry(typeof(T).Name, logLevel, formatter(state, exception), values, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    private sealed record LogEntry(string Category, LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Values, Exception? Exception);

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
        public Exception? PageFailure { get; init; }

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
            if (PageFailure is not null)
            {
                throw PageFailure;
            }

            return Task.FromResult<IReadOnlyCollection<LinkDto>>(Items.Where(item => item.UserId == userId)
                .Select(link => new LinkDto(link.Id, link.CollectionId, link.UrlOriginal, link.UrlNormalized, link.Title, link.Description, link.Image, link.IsPublic, link.CreatedAt))
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
            => Task.CompletedTask;

        public Task Delete(long id, long userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
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

    private sealed class FakeDashboardRepository : IDashboardRepository
    {
        public Task<DashboardDto> GetByUser(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult(new DashboardDto(0, 0, [], []));
    }

    private sealed class FakePublicReadRepository : IPublicReadRepository
    {
        public Task<PublicUserDto?> GetUserPage(string alias, CancellationToken cancellationToken = default)
            => Task.FromResult<PublicUserDto?>(new PublicUserDto(alias, [], []));

        public Task<IReadOnlyCollection<PublicCollectionSummaryDto>?> GetCollectionsPage(string alias, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<PublicCollectionSummaryDto>?>(Array.Empty<PublicCollectionSummaryDto>());

        public Task<PublicCollectionDto?> GetCollectionPage(string alias, string slug, int page, CancellationToken cancellationToken = default)
            => Task.FromResult<PublicCollectionDto?>(null);

        public Task<int> GetCollectionPageCount(string alias, string slug, CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
