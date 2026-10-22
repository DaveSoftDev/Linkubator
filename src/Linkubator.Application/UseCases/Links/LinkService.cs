using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Logging;
using Linkubator.Application.Services;
using Linkubator.Application.Validation.Links;
using DomainEntities = Linkubator.Domain.Entities;
using DomainPolicies = Linkubator.Domain.Policies;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.UseCases.Links;

public sealed class LinkService : ApplicationServiceBase, ILinkService
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILinkRepository _linkRepository;
    private readonly IValidator<LinkGetRequest> _getLinkValidator;
    private readonly IValidator<LinkCreateRequest> _createLinkValidator;
    private readonly IValidator<LinkEditRequest> _editLinkValidator;
    private readonly IValidator<LinkMoveRequest> _moveLinkValidator;
    private readonly IValidator<LinkChangeVisibilityRequest> _changeLinkVisibilityValidator;
    private readonly IValidator<LinkDeleteRequest> _deleteLinkValidator;

    private static LinkDto Map(DomainEntities.Link link)
        => new(
            link.Id,
            link.CollectionId,
            link.UrlOriginal,
            link.UrlNormalized,
            link.Title,
            link.Description,
            link.Image,
            link.IsPublic,
            link.CreatedAt);

    public LinkService(
        ILogger<LinkService> logger,
        ICurrentUser currentUser,
        ICollectionRepository collectionRepository,
        ILinkRepository linkRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        LinkServiceValidators? validators = null)
        : base(logger, currentUser, unitOfWorkFactory)
    {
        ArgumentNullException.ThrowIfNull(collectionRepository);
        ArgumentNullException.ThrowIfNull(linkRepository);

        LinkServiceValidators resolvedValidators = validators ?? new LinkServiceValidators();

        _collectionRepository = collectionRepository;
        _linkRepository = linkRepository;
        _getLinkValidator = resolvedValidators.GetLinkValidator;
        _createLinkValidator = resolvedValidators.CreateLinkValidator;
        _editLinkValidator = resolvedValidators.EditLinkValidator;
        _moveLinkValidator = resolvedValidators.MoveLinkValidator;
        _changeLinkVisibilityValidator = resolvedValidators.ChangeLinkVisibilityValidator;
        _deleteLinkValidator = resolvedValidators.DeleteLinkValidator;
    }

    public async Task<Result<LinkDto>> GetLink(LinkGetRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _getLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link lookup validation failed for user; invalid link ID", UserContext, new { LinkId = request.LinkId });
            return Result<LinkDto>.Failure(LinkErrors.LinkIdInvalid());
        }

        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        return await Execute(
            request,
            cancellationToken,
            async (getRequest, token) =>
            {
                DomainEntities.Link? link = await _linkRepository.GetById(getRequest.LinkId, CurrentUser.UserId, token);
                if (link is null)
                {
                    return Failure<LinkDto>(LinkErrors.LinkNotAccessible(), "Link lookup rejected for user; link not accessible", new { LinkId = getRequest.LinkId });
                }

                return Result<LinkDto>.Success(Map(link));
            },
            "Link lookup failed for user");
    }

    public async Task<Result<LinkDto>> CreateLink(LinkCreateRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _createLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link creation validation failed for user", UserContext, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }

        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        Result<LinkDto> result = await ExecuteInTransaction(
            request,
            cancellationToken,
            async (createRequest, unitOfWork, token) =>
            {
                string normalizedUrl = DomainPolicies.UrlNormalizationPolicy.Normalize(DomainPolicies.LinkUrlPolicy.Adjust(createRequest.Url));
                ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
                ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
                DomainEntities.Collection? collection = await collectionRepository.GetById(createRequest.CollectionId, CurrentUser.UserId, token);
                if (collection is null)
                {
                    return Failure<LinkDto>(LinkErrors.CollectionNotAccessible(), "Link creation rejected for user; collection not accessible", new { CollectionId = createRequest.CollectionId });
                }

                if (createRequest.IsPublic && !collection.IsPublic)
                {
                    return Failure<LinkDto>(LinkErrors.PublicLinkInPrivateCollection(), "Link creation rejected for user; public link in private collection", createRequest);
                }

                DomainEntities.Link? duplicate = await linkRepository.GetByNormalizedUrl(CurrentUser.UserId, normalizedUrl, token);
                if (duplicate is not null)
                {
                    DomainEntities.Collection? duplicateCollection = await collectionRepository.GetById(duplicate.CollectionId, CurrentUser.UserId, token);
                    return Failure<LinkDto>(LinkErrors.DuplicateUrl(duplicateCollection?.Name), "Link creation rejected for user; duplicate URL", createRequest);
                }

                DomainEntities.Link created = DomainEntities.Link.Create(CurrentUser.UserId, collection, createRequest.Url, createRequest.Title, createRequest.Description, createRequest.Image, createRequest.IsPublic);
                await linkRepository.Insert(created, token);
                return Result<LinkDto>.Success(Map(created));
            },
            "Link creation failed for user");
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link created", UserContext);
        }

        return result;
    }

    public async Task<Result<LinkDto>> EditLink(LinkEditRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _editLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link edit validation failed for user", UserContext, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }

        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        Result<LinkDto> result = await ExecuteInTransaction(
            request,
            cancellationToken,
            async (editRequest, unitOfWork, token) =>
            {
                ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
                ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
                DomainEntities.Link? link = await linkRepository.GetById(editRequest.LinkId, CurrentUser.UserId, token);
                if (link is null)
                {
                    return Failure<LinkDto>(LinkErrors.LinkNotAccessible(), "Link edit rejected for user; link not accessible", new { LinkId = editRequest.LinkId });
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, CurrentUser.UserId, token);
                if (collection is null)
                {
                    return Failure<LinkDto>(LinkErrors.CollectionNotAccessible(), "Link edit rejected for user; collection not accessible", new { LinkId = editRequest.LinkId, CollectionId = link.CollectionId });
                }

                if (editRequest.IsPublic == true && !collection.IsPublic)
                {
                    return Failure<LinkDto>(LinkErrors.PublicLinkInPrivateCollection(), "Link edit rejected for user; public link in private collection", editRequest);
                }

                link.Edit(editRequest.Title, editRequest.Description, editRequest.Image);
                if (editRequest.IsPublic is bool requestedVisibility)
                {
                    link.ChangePrivacy(requestedVisibility, collection);
                }

                await linkRepository.Update(link, token);
                return Result<LinkDto>.Success(Map(link));
            },
            "Link edit failed for user");
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link edited", UserContext);
        }

        return result;
    }

    public async Task<Result<LinkDto>> MoveLink(LinkMoveRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _moveLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link move validation failed for user", UserContext, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }

        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        Result<LinkDto> result = await ExecuteInTransaction(
            request,
            cancellationToken,
            async (moveRequest, unitOfWork, token) =>
            {
                ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
                ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
                DomainEntities.Link? link = await linkRepository.GetById(moveRequest.LinkId, CurrentUser.UserId, token);
                if (link is null)
                {
                    return Failure<LinkDto>(LinkErrors.LinkNotAccessible(), "Link move rejected for user; link not accessible", new { LinkId = moveRequest.LinkId });
                }

                DomainEntities.Collection? destinationCollection = await collectionRepository.GetById(moveRequest.DestinationCollectionId, CurrentUser.UserId, token);
                if (destinationCollection is null)
                {
                    return Failure<LinkDto>(LinkErrors.CollectionNotAccessible(), "Link move rejected for user; destination collection not accessible", new { LinkId = moveRequest.LinkId, CollectionId = moveRequest.DestinationCollectionId });
                }

                link.MoveToCollection(destinationCollection);
                await linkRepository.Update(link, token);
                return Result<LinkDto>.Success(Map(link));
            },
            "Link move failed for user");
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link moved", UserContext);
        }

        return result;
    }

    public async Task<Result<LinkDto>> ChangeLinkVisibility(LinkChangeVisibilityRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _changeLinkVisibilityValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link visibility change validation failed for user", UserContext, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }

        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        Result<LinkDto> result = await ExecuteInTransaction(
            request,
            cancellationToken,
            async (visibilityRequest, unitOfWork, token) =>
            {
                ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
                ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
                DomainEntities.Link? link = await linkRepository.GetById(visibilityRequest.LinkId, CurrentUser.UserId, token);
                if (link is null)
                {
                    return Failure<LinkDto>(LinkErrors.LinkNotAccessible(), "Link visibility change rejected for user; link not accessible", new { LinkId = visibilityRequest.LinkId });
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, CurrentUser.UserId, token);
                if (collection is null)
                {
                    return Failure<LinkDto>(LinkErrors.CollectionNotAccessible(), "Link visibility change rejected for user; collection not accessible", new { LinkId = visibilityRequest.LinkId, CollectionId = link.CollectionId });
                }

                if (visibilityRequest.IsPublic && !collection.IsPublic)
                {
                    return Failure<LinkDto>(LinkErrors.PublicLinkInPrivateCollection(), "Link visibility change rejected for user; public link in private collection", visibilityRequest);
                }

                link.ChangePrivacy(visibilityRequest.IsPublic, collection);
                await linkRepository.Update(link, token);
                return Result<LinkDto>.Success(Map(link));
            },
            "Link visibility change failed for user");
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link visibility changed", UserContext);
        }

        return result;
    }

    public async Task<Result> DeleteLink(LinkDeleteRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _deleteLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link deletion validation failed for user", UserContext, request);
            return Result.Failure(validation.Errors);
        }

        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return authentication;
        }

        Result result = await ExecuteInTransaction(
            request,
            cancellationToken,
            async (deleteRequest, unitOfWork, token) =>
            {
                ILinkRepository repository = unitOfWork.GetRepository<ILinkRepository>();
                DomainEntities.Link? link = await repository.GetById(deleteRequest.LinkId, CurrentUser.UserId, token);
                if (link is null)
                {
                    return Failure(LinkErrors.DeleteNotAccessible(), "Link deletion rejected for user; link not accessible", new { LinkId = deleteRequest.LinkId });
                }

                await repository.Delete(deleteRequest.LinkId, CurrentUser.UserId, token);
                return Result.Success();
            },
            "Link deletion failed for user");
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link deleted", UserContext);
        }

        return result;
    }

    public async Task<Result<IReadOnlyList<LinkDto>>> ListLinks(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default)
    {
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<IReadOnlyList<LinkDto>>.Failure(authentication.Errors);
        }
        if (page < 1)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link listing validation failed for user; invalid page", UserContext, new { Page = page });
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.PageInvalid());
        }
        if (collectionId is not null && collectionId <= 0)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link listing validation failed for user; invalid collection ID", UserContext, new { CollectionId = collectionId });
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.CollectionIdInvalid());
        }

        Result<IReadOnlyList<LinkDto>> result = await Execute(
            new { Query = query, CollectionId = collectionId, Page = page },
            cancellationToken,
            async (request, token) =>
            {
                if (request.CollectionId is not null)
                {
                    DomainEntities.Collection? collection = await _collectionRepository.GetById(request.CollectionId.Value, CurrentUser.UserId, token);
                    if (collection is null)
                    {
                        return Failure<IReadOnlyList<LinkDto>>(LinkErrors.CollectionNotAccessible(), "Link listing rejected for user; collection not accessible", new { CollectionId = request.CollectionId });
                    }
                }

                string? trimmedQuery = request.Query is null ? null : request.Query.Trim();
                IReadOnlyCollection<LinkDto> items = await _linkRepository.GetPageByUser(CurrentUser.UserId, trimmedQuery, request.CollectionId, request.Page, token);
                return Result<IReadOnlyList<LinkDto>>.Success(items.ToList());
            },
            "Link listing failed for user",
            handleDomainExceptions: false,
            logContext: new { Page = page, CollectionId = collectionId });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Links listed for user", UserContext);
        }

        return result;
    }
}
