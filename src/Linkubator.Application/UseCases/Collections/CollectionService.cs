using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Logging;
using Linkubator.Application.Services;
using Linkubator.Application.Validation.Collections;
using Microsoft.Extensions.Logging;
using DomainEntities = Linkubator.Domain.Entities;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Application.UseCases.Collections;

public sealed class CollectionService : ApplicationServiceBase, ICollectionService
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IUnitOfWorkFactory _collectionUnitOfWorkFactory;
    private readonly IValidator<CollectionGetRequest> _getCollectionValidator;
    private readonly IValidator<CollectionCreateRequest> _createCollectionValidator;
    private readonly IValidator<CollectionUpdateRequest> _collectionUpdateValidator;
    private readonly IValidator<CollectionDeleteRequest> _deleteCollectionValidator;

    public CollectionService(
        ILogger<CollectionService> logger,
        ICurrentUser currentUser,
        ICollectionRepository collectionRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        CollectionServiceValidators? validators = null)
        : base(logger, currentUser, unitOfWorkFactory)
    {
        ArgumentNullException.ThrowIfNull(collectionRepository);
        ArgumentNullException.ThrowIfNull(unitOfWorkFactory);
        CollectionServiceValidators resolvedValidators = validators ?? new CollectionServiceValidators();
        _collectionRepository = collectionRepository;
        _collectionUnitOfWorkFactory = unitOfWorkFactory;
        _getCollectionValidator = resolvedValidators.CollectionGetValidator;
        _createCollectionValidator = resolvedValidators.CollectionCreateValidator;
        _collectionUpdateValidator = resolvedValidators.CollectionUpdateValidator;
        _deleteCollectionValidator = resolvedValidators.CollectionDeleteValidator;
    }

    public async Task<Result<CollectionDto>> GetCollection(CollectionGetRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _getCollectionValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Collection lookup validation failed for user", UserContext, request);
            return Result<CollectionDto>.Failure(validation.Errors);
        }

        CollectionGetRequest validRequest = request;
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<CollectionDto>.Failure(authentication.Errors);
        }
        return await Execute(
            validRequest,
            cancellationToken,
            async (getRequest, token) =>
            {
                await using IUnitOfWork unitOfWork = _collectionUnitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
                ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
                ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
                DomainEntities.Collection? collection = await collectionRepository.GetById(getRequest.CollectionId, CurrentUser.UserId, token);
                if (collection is null)
                {
                    return Failure<CollectionDto>(CollectionErrors.GetNotAccessible(), "Collection lookup rejected for user; collection not accessible", getRequest);
                }

                int linkCount = await linkRepository.GetCountByCollection(CurrentUser.UserId, collection.Id, token);
                return Result<CollectionDto>.Success(Map(collection) with { LinkCount = linkCount });
            },
            "Collection lookup failed for user",
            logContext: new { validRequest.CollectionId });
    }

    public async Task<Result<CollectionDto>> CreateCollection(CollectionCreateRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _createCollectionValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Collection creation validation failed for user", UserContext, request);
            return Result<CollectionDto>.Failure(validation.Errors);
        }

        CollectionCreateRequest validRequest = request;
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<CollectionDto>.Failure(authentication.Errors);
        }
        Result<CollectionDto> result = await ExecuteInTransaction(
            validRequest,
            cancellationToken,
            async (createRequest, unitOfWork, token) =>
            {
                DomainEntities.Collection collection = DomainEntities.Collection.Create(CurrentUser.UserId, createRequest.Name, createRequest.Description, createRequest.IsPublic);
                ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                DomainEntities.Collection? existing = await repository.GetBySlug(CurrentUser.UserId, collection.Slug, token);
                if (existing is not null)
                {
                    return Failure<CollectionDto>(CollectionErrors.NameInUse(existing.Name), "Collection creation rejected for user; name already in use", createRequest);
                }

                await repository.Insert(collection, token);
                return Result<CollectionDto>.Success(Map(collection));
            },
            "Collection creation failed for user",
            logContext: new { validRequest.Name, validRequest.Description, validRequest.IsPublic });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Collection created for user", UserContext);
        }
        return result;
    }
    public async Task<Result<CollectionDto>> UpdateCollection(CollectionUpdateRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _collectionUpdateValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Collection update validation failed for user", UserContext, new { CollectionId = request?.CollectionId, Name = request?.Name });
            return Result<CollectionDto>.Failure(validation.Errors);
        }

        CollectionUpdateRequest validRequest = request;
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<CollectionDto>.Failure(authentication.Errors);
        }
        Result<CollectionDto> result = await ExecuteInTransaction(
            validRequest,
            cancellationToken,
            async (updateRequest, unitOfWork, token) =>
            {
                ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
                
                (DomainEntities.Collection? collection, Result? collectionFailure) = await GetCollectionForCurrentUserOrFailure(
                    updateRequest.CollectionId,
                    CollectionErrors.UpdateNotAccessible,
                    "Collection update rejected for user; collection not accessible",
                    token,
                    repository);
                if (collectionFailure is not null)
                {
                    return Result<CollectionDto>.Failure(collectionFailure.Errors);
                }

                string newName = updateRequest.Name?.Trim() ?? string.Empty;
                DomainEntities.Collection ownedCollection = collection!;
                string candidateSlug = DomainPolicies.SlugPolicy.Generate(newName);
                DomainEntities.Collection? duplicate = await repository.GetBySlug(CurrentUser.UserId, candidateSlug, token);
                if (duplicate is not null && duplicate.Id != ownedCollection.Id)
                {
                    return Failure<CollectionDto>(CollectionErrors.NameInUse(duplicate.Name), "Collection update rejected for user; name already in use", new { CollectionId = updateRequest.CollectionId });
                }

                int linkCount = await linkRepository.GetCountByCollection(CurrentUser.UserId, ownedCollection.Id, token);
                bool becomingPrivate = ownedCollection.IsPublic && !updateRequest.IsPublic;
                ownedCollection.Rename(newName);
                ownedCollection.ChangeDescription(updateRequest.Description);
                ownedCollection.ChangePrivacy(updateRequest.IsPublic);
                await repository.Update(ownedCollection, token);
                if (becomingPrivate)
                {
                    await linkRepository.UpdatePrivateByCollection(CurrentUser.UserId, ownedCollection.Id, token);
                }

                return Result<CollectionDto>.Success(Map(ownedCollection) with { LinkCount = linkCount });
            },
            "Collection update failed for user",
            logContext: new { validRequest?.CollectionId, validRequest?.Name, validRequest?.Description, validRequest?.IsPublic });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Collection updated for user", UserContext);
        }
        return result;
    }

    public async Task<Result> DeleteCollection(CollectionDeleteRequest? request, CancellationToken cancellationToken = default)
    {
        Result validation = _deleteCollectionValidator.Validate(request);
        if (request is null || !validation.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Warning, null, "Collection deletion validation failed for user", UserContext, new { CollectionId = request?.CollectionId });
            return Result.Failure(validation.Errors);
        }

        CollectionDeleteRequest? validRequest = request;
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
                ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
                ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
                (DomainEntities.Collection? collection, Result? collectionFailure) = await GetCollectionForCurrentUserOrFailure(
                    deleteRequest.CollectionId,
                    CollectionErrors.DeleteNotAccessible,
                    "Collection deletion rejected for user; collection not accessible",
                    token,
                    collectionRepository);
                if (collectionFailure is not null)
                {
                    return collectionFailure;
                }

                DomainEntities.Collection ownedCollection = collection!;
                IReadOnlyCollection<DomainEntities.Link> links = await linkRepository.GetAllByCollection(CurrentUser.UserId, ownedCollection.Id, token);
                if (links.Count > 0)
                {
                    return Failure(CollectionErrors.HasLinks(), "Collection deletion rejected for user; collection still has links", deleteRequest);
                }

                IReadOnlyCollection<DomainEntities.Collection> allCollections = await collectionRepository.GetAllByUser(CurrentUser.UserId, token);
                if (allCollections.Count == 1)
                {
                    return Failure(CollectionErrors.LastCollectionCannotBeDeleted(), "Collection deletion rejected for user; last collection cannot be deleted", deleteRequest);
                }

                await collectionRepository.Delete(ownedCollection.Id, CurrentUser.UserId, token);
                return Result.Success();
            },
            "Collection deletion failed",
            logContext: new { validRequest.CollectionId });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Collection deleted for user", UserContext, new { CollectionId = validRequest.CollectionId });
        }
        return result;
    }
    public async Task<Result<CollectionPageDto>> ListCollectionsPage(string? query = null, int page = 1, CancellationToken cancellationToken = default)
    {
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<CollectionPageDto>.Failure(authentication.Errors);
        }
        if (page < 1)
        {
            return Failure<CollectionPageDto>(CollectionErrors.PageInvalid(), "Collection listing validation failed for user; invalid page", new { Query = query, Page = page });
        }

        string? trimmedQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        Result<CollectionPageDto> result = await Execute(
            new CollectionListOperation(trimmedQuery, page),
            cancellationToken,
            async (operation, token) =>
            {
                int pageCount = await _collectionRepository.GetPageCountByUser(
                    CurrentUser.UserId,
                    operation.Query,
                    token);
                int currentPage = pageCount == 0 ? 1 : Math.Min(operation.Page, pageCount);
                IReadOnlyCollection<CollectionDto> items = await _collectionRepository.GetPageByUser(
                    CurrentUser.UserId,
                    operation.Query,
                    currentPage,
                    token);
                return Result<CollectionPageDto>.Success(new CollectionPageDto(items.ToList(), currentPage, pageCount));
            },
            "Collection listing failed",
            logContext: new { Query = query, Page = page });
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Collections listed for user", UserContext);
        }
        return result;
    }

    public async Task<Result<IReadOnlyCollection<CollectionDto>>> ListAllCollections(CancellationToken cancellationToken = default)
    {
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<IReadOnlyCollection<CollectionDto>>.Failure(authentication.Errors);
        }
        Result<IReadOnlyCollection<CollectionDto>> result = await Execute(
            new CollectionListOperation(string.Empty, 0),
            cancellationToken,
            async (operation, token) =>
            {
                IReadOnlyCollection<CollectionDto> items = await _collectionRepository.GetAllByUserWithLinkCounts(CurrentUser.UserId, token);
                return Result<IReadOnlyCollection<CollectionDto>>.Success(items.ToList());
            },
            "Collection listing failed");
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Collections listed for user", UserContext);
        }
        return result;
    }

    private async Task<(DomainEntities.Collection? Collection, Result? Failure)> GetCollectionForCurrentUserOrFailure(long collectionId, Func<ApplicationError> notAccessibleFactory, string logMessage, CancellationToken cancellationToken, ICollectionRepository repository)
    {
        DomainEntities.Collection? collection = await repository.GetById(collectionId, CurrentUser.UserId, cancellationToken);
        if (collection is null)
        {
            return (null, Failure(notAccessibleFactory(), logMessage, new { CollectionId = collectionId }));
        }
        return (collection, null);
    }

    private static CollectionDto Map(DomainEntities.Collection collection)
        => new(collection.Id, collection.Name, collection.Slug, collection.Description, collection.IsPublic, collection.Links.Count, collection.CreatedAt);

    private sealed record CollectionListOperation(string? Query, int Page);
}
