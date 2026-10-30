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
    private readonly IValidator<LinkUpdateRequest> _updateLinkValidator;
    private readonly IValidator<LinkMoveRequest> _moveLinkValidator;
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
        _updateLinkValidator = resolvedValidators.UpdateLinkValidator;
        _moveLinkValidator = resolvedValidators.MoveLinkValidator;
        _deleteLinkValidator = resolvedValidators.DeleteLinkValidator;
    }

    public async Task<Result<LinkDto>> GetLink(LinkGetRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _getLinkValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link lookup validation failed for user", UserContext, new { LinkId = request?.LinkId });
            return validation.IsSuccess ? Result<LinkDto>.Failure(CommonErrors.RequestRequired()) : Result<LinkDto>.Failure(validation.Errors);
        }

        LinkGetRequest validRequest = request;
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        return await Execute(
            validRequest,
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
            "Link lookup failed for user",
            logContext: new { validRequest.LinkId });
    }

    public async Task<Result<LinkDto>> CreateLink(LinkCreateRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _createLinkValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link creation validation failed for user", UserContext, request);
            return validation.IsSuccess ? Result<LinkDto>.Failure(CommonErrors.RequestRequired()) : Result<LinkDto>.Failure(validation.Errors);
        }

        LinkCreateRequest validRequest = request;
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        Result<LinkDto> result = await ExecuteInTransaction(
            validRequest,
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
            "Link creation failed for user",
            logContext: new { validRequest.CollectionId, validRequest.Url, validRequest.Title, validRequest.Description, validRequest.Image, validRequest.IsPublic });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link created", UserContext);
        }

        return result;
    }

    public async Task<Result<LinkDto>> UpdateLink(LinkUpdateRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _updateLinkValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link update validation failed for user", UserContext, request);
            return validation.IsSuccess ? Result<LinkDto>.Failure(CommonErrors.RequestRequired()) : Result<LinkDto>.Failure(validation.Errors);
        }

        LinkUpdateRequest validRequest = request;
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        Result<LinkDto> result = await ExecuteInTransaction(
            validRequest,
            cancellationToken,
            async (updateRequest, unitOfWork, token) =>
            {
                ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
                ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();

                DomainEntities.Link? link = await linkRepository.GetById(updateRequest.LinkId, CurrentUser.UserId, token);
                if (link is null)
                {
                    return Failure<LinkDto>(LinkErrors.LinkNotAccessible(), "Link update rejected for user; link not accessible", new { LinkId = updateRequest.LinkId });
                }

                bool collectionChanged = updateRequest.CollectionId != link.CollectionId;
                Result<DomainEntities.Collection> destinationCollection = await GetDestinationCollection(updateRequest, collectionRepository, link, collectionChanged, token);
                if (!destinationCollection.IsSuccess)
                {
                    return Result<LinkDto>.Failure(destinationCollection.Errors);
                }

                return await ApplyUpdateLink(updateRequest, linkRepository, link, collectionChanged, destinationCollection.Value!, token);
            },
            "Link update failed for user",
            logContext: new { validRequest.LinkId, validRequest.CollectionId, validRequest.Title, validRequest.Description, validRequest.Image, validRequest.IsPublic });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link updated", UserContext);
        }

        return result;
    }

    private async Task<Result<DomainEntities.Collection>> GetDestinationCollection(LinkUpdateRequest updateRequest, ICollectionRepository collectionRepository, DomainEntities.Link link, bool collectionChanged, CancellationToken token)
    {
        DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, CurrentUser.UserId, token);
        if (collection is null)
        {
            return Failure<DomainEntities.Collection>(LinkErrors.CollectionNotAccessible(), "Link update rejected for user; collection not accessible", new { LinkId = updateRequest.LinkId, CollectionId = link.CollectionId });
        }

        DomainEntities.Collection? destinationCollection = collectionChanged
            ? await collectionRepository.GetById(updateRequest.CollectionId, CurrentUser.UserId, token)
            : collection;
        if (destinationCollection is null)
        {
            return Failure<DomainEntities.Collection>(
                LinkErrors.CollectionSelectionInvalid(),
                "Link update rejected for user; selected collection is not accessible",
                new { LinkId = updateRequest.LinkId, CollectionId = updateRequest.CollectionId });
        }

        return Result<DomainEntities.Collection>.Success(destinationCollection);
    }

    private async Task<Result<LinkDto>> ApplyUpdateLink(LinkUpdateRequest updateRequest, ILinkRepository linkRepository, DomainEntities.Link link, bool collectionChanged, DomainEntities.Collection destinationCollection, CancellationToken token)
    {
        if (!collectionChanged && updateRequest.IsPublic == true && !destinationCollection.IsPublic)
        {
            return Failure<LinkDto>(LinkErrors.PublicLinkInPrivateCollection(), "Link update rejected for user; public link in private collection", updateRequest);
        }

        Result metadataValidation = LinkMetadataValidator.Validate(new LinkMetadataValidationRequest(
            updateRequest.Title,
            updateRequest.Description,
            updateRequest.Image,
            link.UrlOriginal));
        if (!metadataValidation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link update content validation failed for user", UserContext, new { LinkId = updateRequest.LinkId });
            return Result<LinkDto>.Failure(metadataValidation.Errors);
        }

        link.Edit(updateRequest.Title, updateRequest.Description, updateRequest.Image);
        if (collectionChanged)
        {
            link.MoveToCollection(destinationCollection);
        }
        else if (updateRequest.IsPublic is bool requestedVisibility)
        {
            link.ChangePrivacy(requestedVisibility, destinationCollection);
        }

        await linkRepository.Update(link, token);
        return Result<LinkDto>.Success(Map(link));
    }

    public async Task<Result<LinkDto>> MoveLink(LinkMoveRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _moveLinkValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link move validation failed for user", UserContext, request);
            return validation.IsSuccess ? Result<LinkDto>.Failure(CommonErrors.RequestRequired()) : Result<LinkDto>.Failure(validation.Errors);
        }

        LinkMoveRequest validRequest = request;
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<LinkDto>.Failure(authentication.Errors);
        }

        Result<LinkDto> result = await ExecuteInTransaction(
            validRequest,
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
            "Link move failed for user",
            logContext: new { validRequest.LinkId, validRequest.DestinationCollectionId });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link moved", UserContext);
        }

        return result;
    }

    public async Task<Result> DeleteLink(LinkDeleteRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _deleteLinkValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link deletion validation failed for user", UserContext, request);
            return validation.IsSuccess ? Result.Failure(CommonErrors.RequestRequired()) : validation;
        }

        LinkDeleteRequest validRequest = request;
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return authentication;
        }

        Result result = await ExecuteInTransaction(
            validRequest,
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
            "Link deletion failed for user",
            logContext: new { validRequest.LinkId });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Link deleted", UserContext);
        }

        return result;
    }

    public async Task<Result<LinkPageDto>> ListLinksPage(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyList<LinkDto>> links = await ListLinks(query, collectionId, page, cancellationToken);
        if (!links.IsSuccess)
        {
            return Result<LinkPageDto>.Failure(links.Errors);
        }

        return await Execute(
            new { Query = query?.Trim(), CollectionId = collectionId, Page = page },
            cancellationToken,
            async (request, token) =>
            {
                int pageCount = await _linkRepository.GetPageCountByUser(CurrentUser.UserId, request.Query, request.CollectionId, token);
                int currentPage = Math.Min(request.Page, Math.Max(1, pageCount));
                Result<IReadOnlyList<LinkDto>> currentLinks = links;
                if (currentPage != request.Page)
                {
                    currentLinks = await ListLinks(request.Query, request.CollectionId, currentPage, token);
                    if (!currentLinks.IsSuccess)
                    {
                        return Result<LinkPageDto>.Failure(currentLinks.Errors);
                    }
                }

                IReadOnlyList<LinkDto> items = currentLinks.Value
                    ?? throw new InvalidOperationException("A successful link listing must contain its items.");
                return Result<LinkPageDto>.Success(new LinkPageDto(items, currentPage, pageCount));
            },
            "Link page count failed for user",
            handleDomainExceptions: false,
            logContext: new { Query = query, Page = page, CollectionId = collectionId });
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
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link listing validation failed for user; invalid page", UserContext, new { Query = query, Page = page, CollectionId = collectionId });
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.PageInvalid());
        }
        if (collectionId is not null && collectionId <= 0)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Link listing validation failed for user; invalid collection ID", UserContext, new { Query = query, CollectionId = collectionId, Page = page });
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
                        return Failure<IReadOnlyList<LinkDto>>(LinkErrors.CollectionNotAccessible(), "Link listing rejected for user; collection not accessible", new { request.Query, request.CollectionId, request.Page });
                    }
                }

                string? trimmedQuery = request.Query is null ? null : request.Query.Trim();
                IReadOnlyCollection<LinkDto> items = await _linkRepository.GetPageByUser(CurrentUser.UserId, trimmedQuery, request.CollectionId, request.Page, token);
                return Result<IReadOnlyList<LinkDto>>.Success(items.ToList());
            },
            "Link listing failed for user",
            handleDomainExceptions: false,
            logContext: new { Query = query, Page = page, CollectionId = collectionId });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Links listed for user", UserContext);
        }

        return result;
    }
}
