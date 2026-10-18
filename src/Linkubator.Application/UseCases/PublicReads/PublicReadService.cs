using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Logging;
using DomainExceptions = Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.UseCases.PublicReads;

public sealed class PublicReadService : IPublicReadService
{
    private readonly IPublicReadRepository _publicReadRepository;
    private readonly ILogger<PublicReadService> _logger;

    public PublicReadService(ILogger<PublicReadService> logger, IPublicReadRepository publicReadRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(publicReadRepository);

        _logger = logger;
        _publicReadRepository = publicReadRepository;
    }

    public async Task<Result<PublicUserDto>> GetUserPage(string alias, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public user page lookup rejected; alias missing", new { }, new { Alias = alias });
            return Result<PublicUserDto>.Failure(PublicReadErrors.AliasInvalid());
        }

        PublicUserDto? result;
        try
        {
            result = await _publicReadRepository.GetUserPage(trimmedAlias, cancellationToken);
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Public user page lookup failed; domain validation issue", new { }, new { Alias = alias });
            return exception.FailDomain<PublicUserDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Public user page lookup failed; alias", new { }, new { Alias = alias });
            throw;
        }
        if (result is null)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public user page lookup failed; alias not accessible", new { }, new { Alias = alias });
            return Result<PublicUserDto>.Failure(PublicReadErrors.UserNotAccessible());
        }

        SafeLogger.Log(_logger, LogLevel.Information, null, "Public user page loaded", new { });
        return Result<PublicUserDto>.Success(result);
    }

    public async Task<Result<IReadOnlyList<PublicCollectionSummaryDto>>> GetCollectionsPage(string alias, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collections lookup rejected; alias missing", new { }, new { Alias = alias });
            return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Failure(PublicReadErrors.AliasInvalid());
        }

        IReadOnlyCollection<PublicCollectionSummaryDto>? result;
        try
        {
            result = await _publicReadRepository.GetCollectionsPage(trimmedAlias, cancellationToken);
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Public collections lookup failed; domain validation issue", new { }, new { Alias = alias });
            return exception.FailDomain<IReadOnlyList<PublicCollectionSummaryDto>>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Public collections lookup failed; alias", new { }, new { Alias = alias });
            throw;
        }
        if (result is null)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collections lookup failed; alias not accessible", new { }, new { Alias = alias });
            return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Failure(PublicReadErrors.CollectionsNotAccessible());
        }

        SafeLogger.Log(_logger, LogLevel.Information, null, "Public collections page loaded", new { }, new { Alias = alias });
        return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Success(result.ToList());
    }

    public async Task<Result<PublicCollectionDto>> GetCollectionPage(string alias, string slug, int page = 1, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        string trimmedSlug = TrimOrEmpty(slug);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collection page lookup rejected; alias missing", new { }, new { Alias = alias });
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.AliasInvalid());
        }

        if (string.IsNullOrWhiteSpace(trimmedSlug))
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collection page lookup rejected; slug missing", new { }, new { Alias = alias, Slug = slug });
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.SlugInvalid());
        }

        if (page < 1)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collection page lookup rejected; invalid page", new { }, new { Alias = alias, Page = page });
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.PageInvalid());
        }

        PublicCollectionDto? result;
        try
        {
            result = await _publicReadRepository.GetCollectionPage(trimmedAlias, trimmedSlug, page, cancellationToken);
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Public collection page lookup failed; domain validation issue", new { }, new { Alias = alias, Slug = slug, Page = page });
            return exception.FailDomain<PublicCollectionDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Public collection page lookup failed; alias and slug", new { }, new { Alias = alias, Slug = slug, Page = page });
            throw;
        }
        if (result is null)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collection page lookup failed; collection not accessible", new { }, new { Alias = alias, Slug = slug, Page = page });
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.CollectionNotAccessible());
        }

        SafeLogger.Log(_logger, LogLevel.Information, null, "Public collection page loaded", new { }, new { Alias = alias, Slug = slug, Page = page });
        return Result<PublicCollectionDto>.Success(result);
    }

    public async Task<Result<int>> GetCollectionPageCount(string alias, string slug, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        string trimmedSlug = TrimOrEmpty(slug);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collection count lookup rejected; alias missing", new { }, new { Alias = alias });
            return Result<int>.Failure(PublicReadErrors.AliasInvalid());
        }

        if (string.IsNullOrWhiteSpace(trimmedSlug))
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collection count lookup rejected; slug missing", new { }, new { Alias = alias, Slug = slug });
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
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Public collection count lookup failed; domain validation issue", new { }, new { Alias = alias, Slug = slug });
            return exception.FailDomain<int>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Public collection count lookup failed; alias and slug", new { }, new { Alias = alias, Slug = slug });
            throw;
        }
        if (collection is null)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Public collection count lookup failed; collection not accessible", new { }, new { Alias = alias, Slug = slug });
            return Result<int>.Failure(PublicReadErrors.CollectionNotAccessible());
        }

        SafeLogger.Log(_logger, LogLevel.Information, null, "Public collection count loaded", new { });
        return Result<int>.Success(count);
    }

    private static string TrimOrEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
}
