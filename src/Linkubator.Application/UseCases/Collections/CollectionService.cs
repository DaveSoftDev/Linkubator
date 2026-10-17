using DomainEntities = Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Linkubator.Application.Constants;
using Linkubator.Domain.Constants;
namespace Linkubator.Application;
public sealed class CollectionService : ICollectionService
{
    private readonly ICurrentUser _currentUser;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IValidator<CreateCollectionRequest> _createCollectionValidator;
    public CollectionService(
        ICurrentUser currentUser,
        ICollectionRepository collectionRepository,
        ILinkRepository linkRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        IValidator<CreateCollectionRequest>? createCollectionValidator = null)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        ArgumentNullException.ThrowIfNull(linkRepository);
        _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        _createCollectionValidator = createCollectionValidator ?? new CollectionCreateValidator();
    }
    public async Task<Result<CollectionDto>> CreateCollection(CreateCollectionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<CollectionDto>.Failure(CollectionErrors.CreateNotAuthenticated());
        }
        if (request is null)
        {
            return Result<CollectionDto>.Failure(CollectionErrors.RequestRequired());
        }
        Result validation = _createCollectionValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            return Result<CollectionDto>.Failure(validation.Errors);
        }
        try
        {
            long userId = _currentUser.UserId;
            DomainEntities.Collection created = DomainEntities.Collection.Create(userId, request.Name, request.Description, request.IsPublic);
            DomainEntities.Collection? existing = await _collectionRepository.GetBySlug(userId, created.Slug, cancellationToken);
            if (existing is not null)
            {
                return Result<CollectionDto>.Failure(CollectionErrors.NameInUse(existing.Name));
            }
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
            await repository.Insert(created, cancellationToken);
            await unitOfWork.Commit(cancellationToken);
            return Result<CollectionDto>.Success(Map(created));
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            return Result<CollectionDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                CollectionErrors.NameField));
        }
    }
    public async Task<Result<CollectionDto>> RenameCollection(long collectionId, string name, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<CollectionDto>.Failure(CollectionErrors.RenameNotAuthenticated());
        }
        string trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return Result<CollectionDto>.Failure(CollectionErrors.NameRequired());
        }
        if (DomainPolicies.UserTextPolicy.CountCodePoints(trimmedName) > LengthLimitsConstants.CollectionName)
        {
            return Result<CollectionDto>.Failure(CollectionErrors.NameTooLong());
        }
        try
        {
            DomainEntities.Collection? collection = await _collectionRepository.GetById(collectionId, _currentUser.UserId, cancellationToken);
            if (collection is null)
            {
                return Result<CollectionDto>.Failure(CollectionErrors.RenameNotAccessible());
            }
            string candidateSlug = DomainPolicies.SlugPolicy.Generate(trimmedName);
            DomainEntities.Collection? duplicate = await _collectionRepository.GetBySlug(_currentUser.UserId, candidateSlug, cancellationToken);
            if (duplicate is not null && duplicate.Id != collection.Id)
            {
                return Result<CollectionDto>.Failure(CollectionErrors.NameInUse(duplicate.Name));
            }
            collection.Rename(trimmedName);
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
            await repository.Update(collection, cancellationToken);
            await unitOfWork.Commit(cancellationToken);
            return Result<CollectionDto>.Success(Map(collection));
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            return Result<CollectionDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                CollectionErrors.NameField));
        }
    }
    public async Task<Result<CollectionDto>> ChangeCollectionVisibility(long collectionId, bool isPublic, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<CollectionDto>.Failure(CollectionErrors.VisibilityNotAuthenticated());
        }
        try
        {
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
            ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
            bool committed = false;
            try
            {
                DomainEntities.Collection? collection = await repository.GetById(collectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    return Result<CollectionDto>.Failure(CollectionErrors.VisibilityNotAccessible());
                }

                IReadOnlyCollection<DomainEntities.Link> links = await linkRepository.GetAllByCollection(_currentUser.UserId, collection.Id, cancellationToken);
                DomainEntities.Link[] linksToMakePrivate = !isPublic
                    ? links.Where(link => link.IsPublic).ToArray()
                    : [];

                collection.ChangePrivacy(isPublic, links);
                await repository.Update(collection, cancellationToken);
                foreach (DomainEntities.Link link in linksToMakePrivate)
                {
                    await linkRepository.Update(link, cancellationToken);
                }

                await unitOfWork.Commit(cancellationToken);
                committed = true;
                return Result<CollectionDto>.Success(Map(collection));
            }
            finally
            {
                if (!committed)
                {
                    await unitOfWork.Rollback(cancellationToken);
                }
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            return Result<CollectionDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                CollectionErrors.CollectionIdField));
        }
    }
    public async Task<Result> DeleteCollection(long collectionId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(CollectionErrors.DeleteNotAuthenticated());
        }
        try
        {
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
            ILinkRepository linkRepo = unitOfWork.GetRepository<ILinkRepository>();
            bool committed = false;
            try
            {
                DomainEntities.Collection? collection = await repository.GetById(collectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    return Result.Failure(CollectionErrors.DeleteNotAccessible());
                }

                IReadOnlyCollection<DomainEntities.Link> links = await linkRepo.GetAllByCollection(_currentUser.UserId, collection.Id, cancellationToken);
                if (links.Count > 0)
                {
                    return Result.Failure(CollectionErrors.HasLinks());
                }

                IReadOnlyCollection<DomainEntities.Collection> allCollections = await repository.GetAllByUser(_currentUser.UserId, cancellationToken);
                if (allCollections.Count == 1)
                {
                    return Result.Failure(CollectionErrors.LastCollectionCannotBeDeleted());
                }

                await repository.Delete(collectionId, _currentUser.UserId, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;
                return Result.Success();
            }
            finally
            {
                if (!committed)
                {
                    await unitOfWork.Rollback(cancellationToken);
                }
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            return Result.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                CollectionErrors.CollectionIdField));
        }
    }
    public async Task<Result<IReadOnlyList<CollectionDto>>> ListCollections(string? query = null, int page = 1, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<IReadOnlyList<CollectionDto>>.Failure(CollectionErrors.ListNotAuthenticated());
        }
        if (page < 1)
        {
            return Result<IReadOnlyList<CollectionDto>>.Failure(CollectionErrors.PageInvalid());
        }
        string? trimmedQuery = query is null ? null : query.Trim();
        IReadOnlyCollection<CollectionDto> items = await _collectionRepository.GetPageByUser(_currentUser.UserId, trimmedQuery, page, cancellationToken);
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
