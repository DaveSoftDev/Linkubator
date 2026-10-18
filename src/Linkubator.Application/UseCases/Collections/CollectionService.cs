using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Logging;
using Linkubator.Application.Services;
using Linkubator.Application.Validation;
using Linkubator.Domain.Constants;
using DomainEntities = Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Application.UseCases.Collections;
public sealed class CollectionService : ICollectionService
{
    private readonly ICurrentUser _currentUser;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IValidator<CreateCollectionRequest> _createCollectionValidator;
    private readonly ILogger<CollectionService> _logger;

    public CollectionService(
        ICurrentUser currentUser,
        ICollectionRepository collectionRepository,
        ILinkRepository linkRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        IValidator<CreateCollectionRequest>? createCollectionValidator = null,
        ILogger<CollectionService>? logger = null)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        ArgumentNullException.ThrowIfNull(linkRepository);
        _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        _createCollectionValidator = createCollectionValidator ?? new CollectionCreateValidator();
        _logger = logger ?? NullLogger<CollectionService>.Instance;
    }
    public async Task<Result<CollectionDto>> CreateCollection(CreateCollectionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection creation rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<CollectionDto>.Failure(CollectionErrors.CreateNotAuthenticated());
        }
        if (request is null)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection creation rejected for user; request was null", new { UserId = _currentUser.UserId });
            return Result<CollectionDto>.Failure(CollectionErrors.RequestRequired());
        }
        Result validation = _createCollectionValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection creation validation failed for user;", new { UserId = _currentUser.UserId, ErrorCount = validation.Errors.Count }, request);
            return Result<CollectionDto>.Failure(validation.Errors);
        }
        try
        {
            long userId = _currentUser.UserId;
            DomainEntities.Collection created = DomainEntities.Collection.Create(userId, request.Name, request.Description, request.IsPublic);
            DomainEntities.Collection? existing = await _collectionRepository.GetBySlug(userId, created.Slug, cancellationToken);
            if (existing is not null)
            {
                SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection creation rejected for user; name already in use", new { UserId = _currentUser.UserId }, request);
                return Result<CollectionDto>.Failure(CollectionErrors.NameInUse(existing.Name));
            }

            Result<CollectionDto> result = await _unitOfWorkFactory.ExecuteInTransaction(
                UnitOfWorkContexts.Linkubator,
                async (unitOfWork, token) =>
                {
                    ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                    await repository.Insert(created, token);
                    return Result<CollectionDto>.Success(Map(created));
                },
                cancellationToken);

            SafeLogger.Log(_logger, LogLevel.Information, null, "Collection created for user", new { UserId = userId });
            return result;
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
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
    public async Task<Result<CollectionDto>> RenameCollection(long collectionId, string name, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection rename rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<CollectionDto>.Failure(CollectionErrors.RenameNotAuthenticated());
        }
        string trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection rename validation failed for user; missing name", new { UserId = _currentUser.UserId });
            return Result<CollectionDto>.Failure(CollectionErrors.NameRequired());
        }
        if (DomainPolicies.UserTextPolicy.CountCodePoints(trimmedName) > LengthLimitsConstants.CollectionName)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection rename validation failed for user", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId, Name = name });
            return Result<CollectionDto>.Failure(CollectionErrors.NameTooLong());
        }
        try
        {
            DomainEntities.Collection? collection = await _collectionRepository.GetById(collectionId, _currentUser.UserId, cancellationToken);
            if (collection is null)
            {
                SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection rename rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
                return Result<CollectionDto>.Failure(CollectionErrors.RenameNotAccessible());
            }
            string candidateSlug = DomainPolicies.SlugPolicy.Generate(trimmedName);
            DomainEntities.Collection? duplicate = await _collectionRepository.GetBySlug(_currentUser.UserId, candidateSlug, cancellationToken);
            if (duplicate is not null && duplicate.Id != collection.Id)
            {
                SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection rename rejected for user; name already in use", new { UserId = _currentUser.UserId });
                return Result<CollectionDto>.Failure(CollectionErrors.NameInUse(duplicate.Name));
            }
            collection.Rename(trimmedName);

            Result<CollectionDto> result = await _unitOfWorkFactory.ExecuteInTransaction(
                UnitOfWorkContexts.Linkubator,
                async (unitOfWork, token) =>
                {
                    ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                    await repository.Update(collection, token);
                    return Result<CollectionDto>.Success(Map(collection));
                },
                cancellationToken);

            SafeLogger.Log(_logger, LogLevel.Information, null, "Collection renamed for user", new { UserId = _currentUser.UserId });
            return result;
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Collection rename failed for user", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId , Name = name });
            return exception.FailDomain<CollectionDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log( _logger, LogLevel.Error, exception, "Collection rename failed for user", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId, Name = name });
            throw;
        }
    }
    public async Task<Result<CollectionDto>> ChangeCollectionVisibility(long collectionId, bool isPublic, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection visibility change rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<CollectionDto>.Failure(CollectionErrors.VisibilityNotAuthenticated());
        }
        try
        {
            Result<CollectionDto> result = await _unitOfWorkFactory.ExecuteInTransaction(
                UnitOfWorkContexts.Linkubator,
                async (unitOfWork, token) =>
                {
                    ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                    ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();

                    DomainEntities.Collection? collection = await repository.GetById(collectionId, _currentUser.UserId, token);
                    if (collection is null)
                    {
                        SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection visibility change rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
                        return Result<CollectionDto>.Failure(CollectionErrors.VisibilityNotAccessible());
                    }

                    IReadOnlyCollection<DomainEntities.Link> links = await linkRepository.GetAllByCollection(_currentUser.UserId, collection.Id, token);
                    DomainEntities.Link[] linksToMakePrivate = !isPublic
                        ? links.Where(link => link.IsPublic).ToArray()
                        : [];

                    collection.ChangePrivacy(isPublic, links);
                    await repository.Update(collection, token);
                    foreach (DomainEntities.Link link in linksToMakePrivate)
                    {
                        await linkRepository.Update(link, token);
                    }

                    return Result<CollectionDto>.Success(Map(collection));
                },
                cancellationToken);

            SafeLogger.Log(_logger, LogLevel.Information, null, "Collection visibility changed for user", new { UserId = _currentUser.UserId });
            return result;
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Collection visibility change failed", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId, IsPublic = isPublic });
            return exception.FailDomain<CollectionDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Collection visibility change failed", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId, IsPublic = isPublic });
            throw;
        }
    }
    public async Task<Result> DeleteCollection(long collectionId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection deletion rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result.Failure(CollectionErrors.DeleteNotAuthenticated());
        }
        try
        {
            Result result = await _unitOfWorkFactory.ExecuteInTransaction(
                UnitOfWorkContexts.Linkubator,
                async (unitOfWork, token) =>
                {
                    ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
                    ILinkRepository linkRepo = unitOfWork.GetRepository<ILinkRepository>();

                    DomainEntities.Collection? collection = await repository.GetById(collectionId, _currentUser.UserId, token);
                    if (collection is null)
                    {
                        SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection deletion rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
                        return Result.Failure(CollectionErrors.DeleteNotAccessible());
                    }

                    IReadOnlyCollection<DomainEntities.Link> links = await linkRepo.GetAllByCollection(_currentUser.UserId, collection.Id, token);
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

                    await repository.Delete(collectionId, _currentUser.UserId, token);
                    return Result.Success();
                },
                cancellationToken);

            SafeLogger.Log(_logger, LogLevel.Information, null, "Collection deleted for user", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
            return result;
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Collection deletion failed", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
            return exception.FailDomain();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Collection deletion failed", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
            throw;
        }
    }
    public async Task<Result<IReadOnlyList<CollectionDto>>> ListCollections(string? query = null, int page = 1, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection listing rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<IReadOnlyList<CollectionDto>>.Failure(CollectionErrors.ListNotAuthenticated());
        }
        if (page < 1)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Collection listing validation failed for user; invalid page", new { UserId = _currentUser.UserId }, new { Page = page });
            return Result<IReadOnlyList<CollectionDto>>.Failure(CollectionErrors.PageInvalid());
        }
        string? trimmedQuery = query is null ? null : query.Trim();
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
