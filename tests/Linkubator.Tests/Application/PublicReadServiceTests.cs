using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Enums;
using Linkubator.Application.Results;
using Linkubator.Application.UseCases.PublicReads;
using Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Tests.Application;

public class PublicReadServiceTests
{
    [Fact]
    public void Constructor_WhenDependenciesAreNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PublicReadService(null!, new FakePublicReadRepository()));
        Assert.Throws<ArgumentNullException>(() => new PublicReadService(NullLogger<PublicReadService>.Instance, null!));
    }

    [Fact]
    public async Task GetUserPage_WhenAliasIsBlank_ReturnsValidationError()
    {
        PublicReadService service = CreateService(new FakePublicReadRepository());

        Result<PublicUserDto> result = await service.GetUserPage("   ");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("PUBLIC_READ_ALIAS_INVALID", result.Errors[0].Code);
        Assert.Equal(PublicReadErrors.AliasField, result.Errors[0].Field);
    }

    [Fact]
    public async Task GetUserPage_WhenUserExists_ReturnsPublicPage()
    {
        FakePublicReadRepository repository = new();
        repository.UserPage = new PublicUserDto(
            "alice",
            new[]
            {
                new PublicCollectionSummaryDto("Tech", "tech", "Warm collection"),
            },
            new[]
            {
                new PublicLinkDto("https://example.com/post", "Post", "Preview", "https://example.com/icon.png"),
            });
        PublicReadService service = CreateService(repository);

        Result<PublicUserDto> result = await service.GetUserPage(" alice ");

        Assert.True(result.IsSuccess);
        Assert.Equal("alice", repository.LastAlias);
        Assert.Equal("alice", result.Value!.Alias);
        Assert.Single(result.Value.PublicCollections);
        Assert.Single(result.Value.RecentLinks);
        Assert.Equal(repository.UserPage, result.Value);
    }

    [Fact]
    public async Task GetUserPage_WhenUserExistsButHasNoPublicContent_ReturnsEmptyCollectionsAndLinks()
    {
        FakePublicReadRepository repository = new();
        repository.UserPage = new PublicUserDto("alice", Array.Empty<PublicCollectionSummaryDto>(), Array.Empty<PublicLinkDto>());
        PublicReadService service = CreateService(repository);

        Result<PublicUserDto> result = await service.GetUserPage("alice");

        Assert.True(result.IsSuccess);
        Assert.Equal("alice", result.Value!.Alias);
        Assert.Empty(result.Value.PublicCollections);
        Assert.Empty(result.Value.RecentLinks);
    }

    [Fact]
    public async Task GetUserPage_WhenRepositoryThrowsDomainException_ReturnsDomainFailure()
    {
        FakePublicReadRepository repository = new()
        {
            UserPageThrow = new NotValidInputException()
        };
        PublicReadService service = CreateService(repository);

        Result<PublicUserDto> result = await service.GetUserPage("alice");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task GetUserPage_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        FakePublicReadRepository repository = new()
        {
            UserPageThrow = new InvalidOperationException("boom")
        };
        PublicReadService service = CreateService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUserPage("alice"));
    }

    [Fact]
    public async Task GetUserPage_WhenUserIsNotAccessible_ReturnsResourceNotAccessible()
    {
        PublicReadService service = CreateService(new FakePublicReadRepository { UserPage = null });

        Result<PublicUserDto> result = await service.GetUserPage("alice");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("PUBLIC_READ_USER_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollectionsPage_WhenAliasIsBlank_ReturnsValidationError()
    {
        PublicReadService service = CreateService(new FakePublicReadRepository());

        Result<IReadOnlyList<PublicCollectionSummaryDto>> result = await service.GetCollectionsPage(" ");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal("PUBLIC_READ_ALIAS_INVALID", result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollectionsPage_WhenCollectionsExist_ReturnsSummaries()
    {
        FakePublicReadRepository repository = new();
        repository.CollectionsPage = new[]
        {
            new PublicCollectionSummaryDto("Tech", "tech", "Preview"),
            new PublicCollectionSummaryDto("Life", "life", null),
        };
        PublicReadService service = CreateService(repository);

        Result<IReadOnlyList<PublicCollectionSummaryDto>> result = await service.GetCollectionsPage("alice");

        Assert.True(result.IsSuccess);
        Assert.Equal(repository.CollectionsPage, result.Value);
        Assert.Equal("alice", repository.LastCollectionsAlias);
    }

    [Fact]
    public async Task GetCollectionsPage_WhenCollectionsAreEmpty_ReturnsEmptyList()
    {
        FakePublicReadRepository repository = new();
        repository.CollectionsPage = Array.Empty<PublicCollectionSummaryDto>();
        PublicReadService service = CreateService(repository);

        Result<IReadOnlyList<PublicCollectionSummaryDto>> result = await service.GetCollectionsPage("alice");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
        Assert.Equal("alice", repository.LastCollectionsAlias);
    }

    [Fact]
    public async Task GetCollectionsPage_WhenCollectionsAreNotAccessible_ReturnsResourceNotAccessible()
    {
        PublicReadService service = CreateService(new FakePublicReadRepository { CollectionsPage = null });

        Result<IReadOnlyList<PublicCollectionSummaryDto>> result = await service.GetCollectionsPage("alice");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("PUBLIC_READ_COLLECTIONS_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollectionsPage_WhenRepositoryThrowsDomainException_ReturnsDomainFailure()
    {
        FakePublicReadRepository repository = new()
        {
            CollectionsPageThrow = new NotValidInputException()
        };
        PublicReadService service = CreateService(repository);

        Result<IReadOnlyList<PublicCollectionSummaryDto>> result = await service.GetCollectionsPage("alice");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollectionsPage_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        FakePublicReadRepository repository = new()
        {
            CollectionsPageThrow = new InvalidOperationException("boom")
        };
        PublicReadService service = CreateService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetCollectionsPage("alice"));
    }

    [Fact]
    public async Task GetCollectionPage_WhenAliasOrSlugAreInvalid_ReturnsValidationError()
    {
        PublicReadService service = CreateService(new FakePublicReadRepository());

        Result<PublicCollectionDto> aliasResult = await service.GetCollectionPage(" ", "tech");
        Result<PublicCollectionDto> slugResult = await service.GetCollectionPage("alice", " ");

        Assert.False(aliasResult.IsSuccess);
        Assert.Equal("PUBLIC_READ_ALIAS_INVALID", aliasResult.Errors[0].Code);
        Assert.False(slugResult.IsSuccess);
        Assert.Equal("PUBLIC_READ_COLLECTION_SLUG_INVALID", slugResult.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollectionPage_WhenPageIsBelowOne_ReturnsValidationError()
    {
        PublicReadService service = CreateService(new FakePublicReadRepository());

        Result<PublicCollectionDto> result = await service.GetCollectionPage("alice", "tech", page: 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.Validation, result.Errors[0].Category);
        Assert.Equal(CommonErrors.PageInvalidCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollectionPage_WhenCollectionExists_ReturnsKnownPage()
    {
        FakePublicReadRepository repository = new();
        repository.CollectionPage = new PublicCollectionDto(
            "alice",
            "Tech",
            "tech",
            new[]
            {
                new PublicLinkDto("https://example.com/post", "Post", "Preview", "https://example.com/icon.png"),
            },
            2,
            3);
        PublicReadService service = CreateService(repository);

        Result<PublicCollectionDto> result = await service.GetCollectionPage(" alice ", " tech ", 2);

        Assert.True(result.IsSuccess);
        Assert.Equal("alice", repository.LastCollectionAlias);
        Assert.Equal("tech", repository.LastCollectionSlug);
        Assert.Equal(2, repository.LastCollectionPage);
        Assert.Equal("Tech", result.Value!.CollectionName);
        Assert.Single(result.Value.Links);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(3, result.Value.PageCount);
        Assert.Equal(1, repository.CollectionPageCalls);
        Assert.Equal(repository.CollectionPage, result.Value);
    }

    [Fact]
    public async Task GetCollectionPage_WhenCollectionExistsButHasNoPublicLinks_ReturnsEmptyPage()
    {
        FakePublicReadRepository repository = new();
        repository.CollectionPage = new PublicCollectionDto("alice", "Tech", "tech", Array.Empty<PublicLinkDto>(), 1, 0);
        PublicReadService service = CreateService(repository);

        Result<PublicCollectionDto> result = await service.GetCollectionPage("alice", "tech", 1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Tech", result.Value!.CollectionName);
        Assert.Empty(result.Value.Links);
    }

    [Fact]
    public async Task GetCollectionPage_WhenCollectionIsNotAccessible_ReturnsResourceNotAccessible()
    {
        PublicReadService service = CreateService(new FakePublicReadRepository { CollectionPage = null });

        Result<PublicCollectionDto> result = await service.GetCollectionPage("alice", "tech");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("PUBLIC_READ_COLLECTION_NOT_ACCESSIBLE", result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollectionPage_WhenRepositoryThrowsDomainException_ReturnsDomainFailure()
    {
        FakePublicReadRepository repository = new()
        {
            CollectionPageThrow = new NotValidInputException()
        };
        PublicReadService service = CreateService(repository);

        Result<PublicCollectionDto> result = await service.GetCollectionPage("alice", "tech");

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    [Fact]
    public async Task GetCollectionPage_WhenRepositoryThrowsUnexpectedException_ReThrows()
    {
        FakePublicReadRepository repository = new()
        {
            CollectionPageThrow = new InvalidOperationException("boom")
        };
        PublicReadService service = CreateService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetCollectionPage("alice", "tech"));
    }

    private static PublicReadService CreateService(FakePublicReadRepository repository)
        => new(NullLogger<PublicReadService>.Instance, repository);

    private sealed class FakePublicReadRepository : IPublicReadRepository
    {
        public PublicUserDto? UserPage { get; set; }

        public Exception? UserPageThrow { get; set; }

        public IReadOnlyCollection<PublicCollectionSummaryDto>? CollectionsPage { get; set; } = Array.Empty<PublicCollectionSummaryDto>();

        public Exception? CollectionsPageThrow { get; set; }

        public PublicCollectionDto? CollectionPage { get; set; }

        public Exception? CollectionPageThrow { get; set; }

        public int CollectionPageCalls { get; private set; }

        public string LastAlias { get; private set; } = string.Empty;

        public string LastCollectionsAlias { get; private set; } = string.Empty;

        public string LastCollectionAlias { get; private set; } = string.Empty;

        public string LastCollectionSlug { get; private set; } = string.Empty;

        public int LastCollectionPage { get; private set; }

        public Task<PublicUserDto?> GetUserPage(string alias, CancellationToken cancellationToken = default)
        {
            LastAlias = alias;
            if (UserPageThrow is not null)
            {
                return Task.FromException<PublicUserDto?>(UserPageThrow);
            }

            return Task.FromResult(UserPage);
        }

        public Task<IReadOnlyCollection<PublicCollectionSummaryDto>?> GetCollectionsPage(string alias, CancellationToken cancellationToken = default)
        {
            LastCollectionsAlias = alias;
            if (CollectionsPageThrow is not null)
            {
                return Task.FromException<IReadOnlyCollection<PublicCollectionSummaryDto>?>(CollectionsPageThrow);
            }

            return Task.FromResult(CollectionsPage);
        }

        public Task<PublicCollectionDto?> GetCollectionPage(string alias, string slug, int page, CancellationToken cancellationToken = default)
        {
            CollectionPageCalls++;
            LastCollectionAlias = alias;
            LastCollectionSlug = slug;
            LastCollectionPage = page;
            if (CollectionPageThrow is not null)
            {
                return Task.FromException<PublicCollectionDto?>(CollectionPageThrow);
            }

            return Task.FromResult(CollectionPage);
        }


    }
}
