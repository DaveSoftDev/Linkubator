using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Logging;
using Linkubator.Application.Services;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.UseCases.PublicReads;

public sealed class PublicReadService : ApplicationServiceBase, IPublicReadService
{
    private readonly IPublicReadRepository _publicReadRepository;

    private static string TrimOrEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    public PublicReadService(ILogger<PublicReadService> logger, IPublicReadRepository publicReadRepository)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(publicReadRepository);
        _publicReadRepository = publicReadRepository;
    }

    public async Task<Result<PublicUserDto>> GetUserPage(string alias, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Public user page lookup rejected; alias missing", UserContext, new { Alias = alias });
            return Result<PublicUserDto>.Failure(PublicReadErrors.AliasInvalid());
        }

        Result<PublicUserDto> result = await Execute(
            new { Alias = alias, TrimmedAlias = trimmedAlias },
            cancellationToken,
            async (request, token) =>
            {
                PublicUserDto? user = await _publicReadRepository.GetUserPage(request.TrimmedAlias, token);
                if (user is null)
                {
                    return Failure<PublicUserDto>(PublicReadErrors.UserNotAccessible(), "Public user page lookup rejected; alias not accessible", new { request.Alias });
                }

                return Result<PublicUserDto>.Success(user);
            },
            "Public user page lookup failed",
            logContext: new { Alias = alias, TrimmedAlias = trimmedAlias });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Public user page loaded", UserContext);
        }

        return result;
    }

    public async Task<Result<IReadOnlyList<PublicCollectionSummaryDto>>> GetCollectionsPage(string alias, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Public collections lookup rejected; alias missing", UserContext, new { Alias = alias });
            return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Failure(PublicReadErrors.AliasInvalid());
        }

        Result<IReadOnlyList<PublicCollectionSummaryDto>> result = await Execute(
            new { Alias = alias, TrimmedAlias = trimmedAlias },
            cancellationToken,
            async (request, token) =>
            {
                IReadOnlyCollection<PublicCollectionSummaryDto>? collections = await _publicReadRepository.GetCollectionsPage(request.TrimmedAlias, token);
                if (collections is null)
                {
                    return Failure<IReadOnlyList<PublicCollectionSummaryDto>>(PublicReadErrors.CollectionsNotAccessible(), "Public collections lookup rejected; alias not accessible", new { request.Alias });
                }

                return Result<IReadOnlyList<PublicCollectionSummaryDto>>.Success(collections.ToList());
            },
            "Public collections lookup failed",
            logContext: new { Alias = alias, TrimmedAlias = trimmedAlias });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Public collections page loaded", UserContext, new { Alias = alias });
        }

        return result;
    }

    public async Task<Result<PublicCollectionDto>> GetCollectionPage(string alias, string slug, int page = 1, CancellationToken cancellationToken = default)
    {
        string trimmedAlias = TrimOrEmpty(alias);
        string trimmedSlug = TrimOrEmpty(slug);
        if (string.IsNullOrWhiteSpace(trimmedAlias))
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Public collection page lookup rejected; alias missing", UserContext, new { Alias = alias });
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.AliasInvalid());
        }

        if (string.IsNullOrWhiteSpace(trimmedSlug))
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Public collection page lookup rejected; slug missing", UserContext, new { Alias = alias, Slug = slug });
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.SlugInvalid());
        }

        if (page < 1)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Public collection page lookup rejected; invalid page", UserContext, new { Alias = alias, Page = page });
            return Result<PublicCollectionDto>.Failure(PublicReadErrors.PageInvalid());
        }

        Result<PublicCollectionDto> result = await Execute(
            new { Alias = alias, Slug = slug, TrimmedAlias = trimmedAlias, TrimmedSlug = trimmedSlug, Page = page },
            cancellationToken,
            async (request, token) =>
            {
                PublicCollectionDto? collection = await _publicReadRepository.GetCollectionPage(request.TrimmedAlias, request.TrimmedSlug, request.Page, token);
                if (collection is null)
                {
                    return Failure<PublicCollectionDto>(PublicReadErrors.CollectionNotAccessible(), "Public collection page lookup rejected; collection not accessible", new { request.Alias, request.Slug, request.Page });
                }

                return Result<PublicCollectionDto>.Success(collection);
            },
            "Public collection page lookup failed",
            logContext: new { Alias = alias, Slug = slug, TrimmedAlias = trimmedAlias, TrimmedSlug = trimmedSlug, Page = page });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Public collection page loaded", UserContext, new { Alias = alias, Slug = slug, Page = page });
        }

        return result;
    }

}
