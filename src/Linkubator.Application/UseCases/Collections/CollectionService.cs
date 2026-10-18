using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Logging;
using Linkubator.Application.Services;
using Linkubator.Application.Validation.Collections;
using DomainEntities = Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.UseCases.Collections;

public sealed class CollectionService : ICollectionService
{
    private readonly ICurrentUser _currentUser;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IValidator<CollectionCreateRequest> _createCollectionValidator;
    private readonly IValidator<CollectionRenameRequest> _renameCollectionValidator;
    private readonly IValidator<CollectionChangeVisibilityRequest> _changeCollectionVisibilityValidator;
    private readonly IValidator<CollectionDeleteRequest> _deleteCollectionValidator;
    private readonly ILogger<CollectionService> _logger;

    public CollectionService(
        ILogger<CollectionService> logger,
        ICurrentUser currentUser,
        ICollectionRepository collectionRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        CollectionServiceValidators? validators = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(collectionRepository);
        ArgumentNullException.ThrowIfNull(unitOfWorkFactory);

        CollectionServiceValidators resolvedValidators = validators ?? new CollectionServiceValidators();

        _logger = logger;
        _currentUser = currentUser;
        _collectionRepository = collectionRepository;
        _unitOfWorkFactory = unitOfWorkFactory;
        _createCollectionValidator = resolvedValidators.CollectionCreateValidator;
        _renameCollectionValidator = resolvedValidators.CollectionRenameValidator;
        _changeCollectionVisibilityValidator = resolvedValidators.CollectionChangeVisibilityValidator;
        _deleteCollectionValidator = resolvedValidators.CollectionDeleteValidator;
    }

    public async Task<Result<CollectionDto>> CreateCollection(CollectionCreateRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _createCollectionValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection creation validation failed for user;", new { UserId = _currentUser.UserId, ErrorCount = validation.Errors.Count }, request);
            return Result<CollectionDto>.Failure(validation.Errors);
        }
        if (!CurrentUserGuard.EnsureAuthenticated(_currentUser, _logger, CollectionErrors.CreateNotAuthenticated, "Collection creation rejected for user; not authenticated", out Result<CollectionDto> authenticationFailure))
        {
            return authenticationFailure;
        }
        try
        {
            long userId = _currentUser.UserId;
            DomainEntities.Collection newCollection = DomainEntities.Collection.Create(userId, request.Name, request.Description, request.IsPublic);

            Result<CollectionDto> result = await _unitOfWorkFactory.ExecuteInTransaction(
                UnitOfWorkContexts.Linkubator,
                async (unitOfWork, token) =>
                {
                    ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                    DomainEntities.Collection? existing = await repository.GetBySlug(userId, newCollection.Slug, token);
                    if (existing is not null)
                    {
                        SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection creation rejected for user; name already in use", new { UserId = _currentUser.UserId }, request);
                        return Result<CollectionDto>.Failure(CollectionErrors.NameInUse(existing.Name));
                    }

                    await repository.Insert(newCollection, token);
                    return Result<CollectionDto>.Success(Map(newCollection));
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                SafeLogger.Log(_logger, LogLevel.Information, null, "Collection created for user", new { UserId = userId });
            }

            return result;
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Collection creation failed for user", new { UserId = _currentUser.UserId }, request);
            return exception.FailDomain<CollectionDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Collection creation failed for user", new { UserId = _currentUser.UserId }, request);
            throw;
        }
    }

    public async Task<Result<CollectionDto>> RenameCollection(CollectionRenameRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _renameCollectionValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection rename validation failed for user", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId, Name = request.Name });
            return Result<CollectionDto>.Failure(validation.Errors);
        }
        if (!CurrentUserGuard.EnsureAuthenticated(_currentUser, _logger, CollectionErrors.RenameNotAuthenticated, "Collection rename rejected for user; not authenticated", out Result<CollectionDto> authenticationFailure))
        {
            return authenticationFailure;
        }
        try
        {
            Result<CollectionDto> result = await _unitOfWorkFactory.ExecuteInTransaction(
                UnitOfWorkContexts.Linkubator,
                async (unitOfWork, token) =>
                {
                    ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                    (DomainEntities.Collection? collection, Result? collectionFailure) = await GetCollectionForCurrentUserOrFailure(
                        request.CollectionId,
                        CollectionErrors.RenameNotAccessible,
                        "Collection rename rejected for user; collection not accessible",
                        token,
                        repository);
                    if (collectionFailure is not null)
                    {
                        return Result<CollectionDto>.Failure(collectionFailure.Errors);
                    }

                    string newName = request.Name?.Trim() ?? string.Empty;
                    DomainEntities.Collection ownedCollection = collection!;
                    string candidateSlug = DomainPolicies.SlugPolicy.Generate(newName);
                    DomainEntities.Collection? duplicate = await repository.GetBySlug(_currentUser.UserId, candidateSlug, token);
                    if (duplicate is not null && duplicate.Id != ownedCollection.Id)
                    {
                        SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection rename rejected for user; name already in use", new { UserId = _currentUser.UserId });
                        return Result<CollectionDto>.Failure(CollectionErrors.NameInUse(duplicate.Name));
                    }

                    ownedCollection.Rename(newName);
                    await repository.Update(ownedCollection, token);
                    return Result<CollectionDto>.Success(Map(ownedCollection));
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                SafeLogger.Log(_logger, LogLevel.Information, null, "Collection renamed for user", new { UserId = _currentUser.UserId });
            }

            return result;
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Collection rename failed for user", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId, Name = request.Name });
            return exception.FailDomain<CollectionDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Collection rename failed for user", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId, Name = request.Name });
            throw;
        }
    }

    public async Task<Result<CollectionDto>> ChangeCollectionVisibility(CollectionChangeVisibilityRequest request, CancellationToken cancellationToken = default)
    {
        Result visibilityValidation = _changeCollectionVisibilityValidator.Validate(request);
        if (!visibilityValidation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection change visibility validation failed for user", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId, IsPublic = request.IsPublic });
            return Result<CollectionDto>.Failure(visibilityValidation.Errors);
        }
        if (!CurrentUserGuard.EnsureAuthenticated(_currentUser, _logger, CollectionErrors.VisibilityNotAuthenticated, "Collection visibility change rejected for user; not authenticated", out Result<CollectionDto> authenticationFailure))
        {
            return authenticationFailure;
        }

        try
        {
            Result<CollectionDto> result = await _unitOfWorkFactory.ExecuteInTransaction(
                UnitOfWorkContexts.Linkubator,
                async (unitOfWork, token) =>
                {
                    ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                    ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();

                    (DomainEntities.Collection? collection, Result? collectionFailure) = await GetCollectionForCurrentUserOrFailure(
                        request.CollectionId,
                        CollectionErrors.VisibilityNotAccessible,
                        "Collection visibility change rejected for user; collection not accessible",
                        token,
                        repository);
                    if (collectionFailure is not null)
                    {
                        return Result<CollectionDto>.Failure(collectionFailure.Errors);
                    }

                    DomainEntities.Collection ownedCollection = collection!;
                    IReadOnlyCollection<DomainEntities.Link> links = await linkRepository.GetAllByCollection(_currentUser.UserId, ownedCollection.Id, token);
                    ownedCollection.RehydrateLinks(links);

                    DomainEntities.Link[] linksToMakePrivate = !request.IsPublic
                        ? links.Where(link => link.IsPublic).ToArray()
                        : [];

                    ownedCollection.ChangePrivacy(request.IsPublic, links);
                    await repository.Update(ownedCollection, token);
                    foreach (DomainEntities.Link link in linksToMakePrivate)
                    {
                        await linkRepository.Update(link, token);
                    }

                    return Result<CollectionDto>.Success(Map(ownedCollection));
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                SafeLogger.Log(_logger, LogLevel.Information, null, "Collection visibility changed for user", new { UserId = _currentUser.UserId });
            }

            return result;
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Collection visibility change failed", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId, IsPublic = request.IsPublic });
            return exception.FailDomain<CollectionDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Collection visibility change failed", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId, IsPublic = request.IsPublic });
            throw;
        }
    }

    public async Task<Result> DeleteCollection(CollectionDeleteRequest request, CancellationToken cancellationToken = default)
    {
        Result deleteValidation = _deleteCollectionValidator.Validate(request);
        if (!deleteValidation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection deletion validation failed for user", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId });
            return Result.Failure(deleteValidation.Errors);
        }
        if (!CurrentUserGuard.EnsureAuthenticated(_currentUser, _logger, CollectionErrors.DeleteNotAuthenticated, "Collection deletion rejected for user; not authenticated", out Result authenticationFailure))
        {
            return authenticationFailure;
        }

        try
        {
            Result result = await _unitOfWorkFactory.ExecuteInTransaction(
                UnitOfWorkContexts.Linkubator,
                async (unitOfWork, token) =>
                {
                    ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                    ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();

                    (DomainEntities.Collection? collection, Result? collectionFailure) = await GetCollectionForCurrentUserOrFailure(
                        request.CollectionId,
                        CollectionErrors.DeleteNotAccessible,
                        "Collection deletion rejected for user; collection not accessible",
                        token,
                        repository);
                    if (collectionFailure is not null)
                    {
                        return collectionFailure;
                    }

                    DomainEntities.Collection ownedCollection = collection!;
                    IReadOnlyCollection<DomainEntities.Link> links = await linkRepository.GetAllByCollection(_currentUser.UserId, ownedCollection.Id, token);
                    if (links.Count > 0)
                    {
                        SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection deletion rejected for user; collection still has links", new { UserId = _currentUser.UserId });
                        return Result.Failure(CollectionErrors.HasLinks());
                    }

                    IReadOnlyCollection<DomainEntities.Collection> allCollections = await repository.GetAllByUser(_currentUser.UserId, token);
                    if (allCollections.Count == 1)
                    {
                        SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection deletion rejected for user; last collection cannot be deleted", new { UserId = _currentUser.UserId });
                        return Result.Failure(CollectionErrors.LastCollectionCannotBeDeleted());
                    }

                    await repository.Delete(ownedCollection.Id, _currentUser.UserId, token);
                    return Result.Success();
                },
                cancellationToken);

            if (result.IsSuccess)
            {
                SafeLogger.Log(_logger, LogLevel.Information, null, "Collection deleted for user", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId });
            }

            return result;
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Collection deletion failed", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId });
            return exception.FailDomain();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Collection deletion failed", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId });
            throw;
        }
    }

    public async Task<Result<IReadOnlyList<CollectionDto>>> ListCollections(string? query = null, int page = 1, CancellationToken cancellationToken = default)
    {
        if (!CurrentUserGuard.EnsureAuthenticated(_currentUser, _logger, CollectionErrors.ListNotAuthenticated, "Collection listing rejected for user; not authenticated", out Result<IReadOnlyList<CollectionDto>> authenticationFailure))
        {
            return authenticationFailure;
        }
        if (page < 1)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection listing validation failed for user; invalid page", new { UserId = _currentUser.UserId }, new { Page = page });
            return Result<IReadOnlyList<CollectionDto>>.Failure(CollectionErrors.PageInvalid());
        }
        string? trimmedQuery = query?.Trim();
        IReadOnlyCollection<CollectionDto> items;
        try
        {
            items = await _collectionRepository.GetPageByUser(_currentUser.UserId, trimmedQuery, page, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Collection listing failed", new { UserId = _currentUser.UserId }, new { Page = page });
            throw;
        }
        SafeLogger.Log(_logger, LogLevel.Information, null, "Collections listed for user", new { UserId = _currentUser.UserId });
        return Result<IReadOnlyList<CollectionDto>>.Success(items.ToList());
    }

    private async Task<(DomainEntities.Collection? Collection, Result? Failure)> GetCollectionForCurrentUserOrFailure(
        long collectionId,
        Func<ApplicationError> notAccessibleFactory,
        string logMessage,
        CancellationToken cancellationToken,
        ICollectionRepository repository)
    {
        DomainEntities.Collection? collection = await repository.GetById(collectionId, _currentUser.UserId, cancellationToken);
        if (collection is null)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, logMessage, new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
            return (null, Result.Failure(notAccessibleFactory()));
        }

        return (collection, null);
    }

    private static CollectionDto Map(DomainEntities.Collection collection)
        => new(
            collection.Id,
            collection.Name,
            collection.Slug,
            collection.Description,
            collection.IsPublic,
            collection.Links.Count,
            collection.CreatedAt);
}
