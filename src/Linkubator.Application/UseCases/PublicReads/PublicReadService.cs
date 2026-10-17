using Linkubator.Application.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Application;

public sealed class PublicReadService : IPublicReadService
{
    private readonly IPublicReadRepository _publicReadRepository;
    private readonly ILogger<PublicReadService> _logger;

    public PublicReadService(IPublicReadRepository publicReadRepository, ILogger<PublicReadService>? logger = null)
    {
        _publicReadRepository = publicReadRepository ?? throw new ArgumentNullException(nameof(publicReadRepository));
        _logger = logger ?? NullLogger<PublicReadService>.Instance;
    }

    public async Task<Result<PublicUserDto>> GetUserPage(string alias, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            _logger.LogWarning("Public user page lookup rejected: alias missing");
            return Result<PublicUserDto>.Failure(PublicReadErrors.AliasInvalid());
        }

        PublicUserDto? result;
        try
        {
            result = await _publicReadRepository.GetUserPage(trimmedAlias, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Public user page lookup failed. AliasLength {AliasLength}", trimmedAlias.Length);
            throw;
        }
        if (result is null)
        {
            _logger.LogWarning("Public user page lookup failed: alias not accessible");
            return Result<PublicUserDto>.Failure(PublicReadErrors.UserNotAccessible());
        }

        _logger.LogInformation("Public user page loaded. AliasLength {AliasLength}", trimmedAlias.Length);
        return Result<PublicUserDto>.Success(result);
    }

    public async Task<Result<IReadOnlyList<PublicCollectionSummaryDto>>> GetCollectionsPage(string alias, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            _logger.LogWarning("Public collections lookup rejected: alias missing");
            return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Failure(PublicReadErrors.AliasInvalid());
        }

        IReadOnlyCollection<PublicCollectionSummaryDto>? result;
        try
        {
            result = await _publicReadRepository.GetCollectionsPage(trimmedAlias, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Public collections lookup failed. AliasLength {AliasLength}", trimmedAlias.Length);
            throw;
        }
        if (result is null)
        {
            _logger.LogWarning("Public collections lookup failed: alias not accessible");
            return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Failure(PublicReadErrors.CollectionsNotAccessible());
        }

        _logger.LogInformation("Public collections page loaded. AliasLength {AliasLength} ResultCount {ResultCount}", trimmedAlias.Length, result.Count);
        return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Success(result.ToList());
    }

    public async Task<Result<PublicCollectionDto>> GetCollectionPage(string alias, string slug, int page = 1, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        string trimmedSlug = TrimOrEmpty(slug);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            _logger.LogWarning("Public collection page lookup rejected: alias missing");
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.AliasInvalid());
        }

        if (string.IsNullOrWhiteSpace(trimmedSlug))
        {
            _logger.LogWarning("Public collection page lookup rejected: slug missing");
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.SlugInvalid());
        }

        if (page < 1)
        {
            _logger.LogWarning("Public collection page lookup rejected: invalid page {Page}", page);
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.PageInvalid());
        }

        PublicCollectionDto? result;
        try
        {
            result = await _publicReadRepository.GetCollectionPage(trimmedAlias, trimmedSlug, page, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Public collection page lookup failed. AliasLength {AliasLength} Page {Page}", trimmedAlias.Length, page);
            throw;
        }
        if (result is null)
        {
            _logger.LogWarning("Public collection page lookup failed: collection not accessible");
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.CollectionNotAccessible());
        }

        _logger.LogInformation("Public collection page loaded. AliasLength {AliasLength} Page {Page}", trimmedAlias.Length, page);
        return Result<PublicCollectionDto>.Success(result);
    }

    public async Task<Result<int>> GetCollectionPageCount(string alias, string slug, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        string trimmedSlug = TrimOrEmpty(slug);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            _logger.LogWarning("Public collection count lookup rejected: alias missing");
            return Result<int>.Failure(PublicReadErrors.AliasInvalid());
        }

        if (string.IsNullOrWhiteSpace(trimmedSlug))
        {
            _logger.LogWarning("Public collection count lookup rejected: slug missing");
            return Result<int>.Failure(PublicReadErrors.SlugInvalid());
        }

        PublicCollectionDto? collection;
        int count;
        try
        {
            collection = await _publicReadRepository.GetCollectionPage(trimmedAlias, trimmedSlug, 1, cancellationToken);
            if (collection is not null)
            {
                count = await _publicReadRepository.GetCollectionPageCount(trimmedAlias, trimmedSlug, cancellationToken);
            }
            else
            {
                count = 0;
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Public collection count lookup failed. AliasLength {AliasLength}", trimmedAlias.Length);
            throw;
        }
        if (collection is null)
        {
            _logger.LogWarning("Public collection count lookup failed: collection not accessible");
            return Result<int>.Failure(PublicReadErrors.CollectionNotAccessible());
        }

        _logger.LogInformation("Public collection count loaded. AliasLength {AliasLength} ResultCount {ResultCount}", trimmedAlias.Length, count);
        return Result<int>.Success(count);
    }

    private static string TrimOrEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
}
