using Linkubator.Application.Constants;

namespace Linkubator.Application;

public sealed class PublicReadService : IPublicReadService
{
    private readonly IPublicReadRepository _publicReadRepository;

    public PublicReadService(IPublicReadRepository publicReadRepository)
    {
        _publicReadRepository = publicReadRepository ?? throw new ArgumentNullException(nameof(publicReadRepository));
    }

    public async Task<Result<PublicUserDto>> GetUserPage(string alias, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            return Result<PublicUserDto>.Failure(PublicReadErrors.AliasInvalid());
        }

        PublicUserDto? result = await _publicReadRepository.GetUserPage(trimmedAlias, cancellationToken);
        if (result is null)
        {
            return Result<PublicUserDto>.Failure(PublicReadErrors.UserNotAccessible());
        }

        return Result<PublicUserDto>.Success(result);
    }

    public async Task<Result<IReadOnlyList<PublicCollectionSummaryDto>>> GetCollectionsPage(string alias, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Failure(PublicReadErrors.AliasInvalid());
        }

        IReadOnlyCollection<PublicCollectionSummaryDto>? result = await _publicReadRepository.GetCollectionsPage(trimmedAlias, cancellationToken);
        if (result is null)
        {
            return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Failure(PublicReadErrors.CollectionsNotAccessible());
        }

        return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Success(result.ToList());
    }

    public async Task<Result<PublicCollectionDto>> GetCollectionPage(string alias, string slug, int page = 1, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        string trimmedSlug = TrimOrEmpty(slug);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.AliasInvalid());
        }

        if (string.IsNullOrWhiteSpace(trimmedSlug))
        {
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.SlugInvalid());
        }

        if (page < 1)
        {
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.PageInvalid());
        }

        PublicCollectionDto? result = await _publicReadRepository.GetCollectionPage(trimmedAlias, trimmedSlug, page, cancellationToken);
        if (result is null)
        {
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.CollectionNotAccessible());
        }

        return Result<PublicCollectionDto>.Success(result);
    }

    public async Task<Result<int>> GetCollectionPageCount(string alias, string slug, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        string trimmedSlug = TrimOrEmpty(slug);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            return Result<int>.Failure(PublicReadErrors.AliasInvalid());
        }

        if (string.IsNullOrWhiteSpace(trimmedSlug))
        {
            return Result<int>.Failure(PublicReadErrors.SlugInvalid());
        }

        PublicCollectionDto? collection = await _publicReadRepository.GetCollectionPage(trimmedAlias, trimmedSlug, 1, cancellationToken);
        if (collection is null)
        {
            return Result<int>.Failure(PublicReadErrors.CollectionNotAccessible());
        }

        int count = await _publicReadRepository.GetCollectionPageCount(trimmedAlias, trimmedSlug, cancellationToken);
        return Result<int>.Success(count);
    }

    private static string TrimOrEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
}
