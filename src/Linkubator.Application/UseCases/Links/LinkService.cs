using DomainEntities = Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Linkubator.Application.Constants;

namespace Linkubator.Application;

public sealed class LinkService : ILinkService
{
    private readonly ICurrentUser _currentUser;
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILinkRepository _linkRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IValidator<CreateLinkRequest> _createLinkValidator;

    public LinkService(
        ICurrentUser currentUser,
        ICollectionRepository collectionRepository,
        ILinkRepository linkRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        IValidator<CreateLinkRequest>? createLinkValidator = null)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        _linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));
        _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        _createLinkValidator = createLinkValidator ?? new LinkCreateValidator();
    }

    public async Task<Result<LinkDto>> CreateLink(CreateLinkRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<LinkDto>.Failure(LinkErrors.CreateNotAuthenticated());
        }

        if (request is null)
        {
            return Result<LinkDto>.Failure(LinkErrors.RequestRequired());
        }

        Result validation = _createLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            return Result<LinkDto>.Failure(validation.Errors);
        }

        try
        {
            DomainEntities.Collection? collection = await _collectionRepository.GetById(request.CollectionId, _currentUser.UserId, cancellationToken);
            if (collection is null)
            {
                return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
            }

            if (request.IsPublic && !collection.IsPublic)
            {
                return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
            }

            string normalizedUrl = DomainPolicies.UrlNormalizationPolicy.Normalize(
                DomainPolicies.LinkUrlPolicy.Adjust(request.Url));

            DomainEntities.Link? duplicate = await _linkRepository.GetByNormalizedUrl(_currentUser.UserId, normalizedUrl, cancellationToken);
            if (duplicate is not null)
            {
                DomainEntities.Collection? duplicateCollection = await _collectionRepository.GetById(
                    duplicate.CollectionId,
                    _currentUser.UserId,
                    cancellationToken);
                return Result<LinkDto>.Failure(LinkErrors.DuplicateUrl(duplicateCollection?.Name));
            }

            DomainEntities.Link created = DomainEntities.Link.Create(
                _currentUser.UserId,
                collection,
                request.Url,
                request.Title,
                request.Description,
                request.Image,
                request.IsPublic);

            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ILinkRepository repository = unitOfWork.GetRepository<ILinkRepository>();
            await repository.Insert(created, cancellationToken);
            await unitOfWork.Commit(cancellationToken);
            return Result<LinkDto>.Success(Map(created));
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            return Result<LinkDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.CollectionIdField));
        }
    }

    public async Task<Result<LinkDto>> EditLink(long linkId, string? title = null, string? description = null, string? image = null, bool? isPublic = null, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<LinkDto>.Failure(LinkErrors.EditNotAuthenticated());
        }

        if (linkId <= 0)
        {
            return Result<LinkDto>.Failure(LinkErrors.LinkIdInvalid());
        }

        try
        {
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
            ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
            bool committed = false;
            try
            {
                DomainEntities.Link? link = await linkRepository.GetById(linkId, _currentUser.UserId, cancellationToken);
                if (link is null)
                {
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (isPublic == true && !collection.IsPublic)
                {
                    return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
                }

                link.Edit(title, description, image);
                if (isPublic is bool requestedVisibility)
                {
                    link.ChangePrivacy(requestedVisibility, collection);
                }
                await linkRepository.Update(link, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;
                return Result<LinkDto>.Success(Map(link));
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
            return Result<LinkDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.LinkIdField));
        }
    }

    public async Task<Result<LinkDto>> MoveLink(long linkId, long destinationCollectionId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<LinkDto>.Failure(LinkErrors.MoveNotAuthenticated());
        }

        if (linkId <= 0)
        {
            return Result<LinkDto>.Failure(LinkErrors.LinkIdInvalid());
        }

        if (destinationCollectionId <= 0)
        {
            return Result<LinkDto>.Failure(LinkErrors.DestinationCollectionIdInvalid());
        }

        try
        {
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
            ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
            bool committed = false;
            try
            {
                DomainEntities.Link? link = await linkRepository.GetById(linkId, _currentUser.UserId, cancellationToken);
                if (link is null)
                {
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? destinationCollection = await collectionRepository.GetById(destinationCollectionId, _currentUser.UserId, cancellationToken);
                if (destinationCollection is null)
                {
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                link.MoveToCollection(destinationCollection);
                await linkRepository.Update(link, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;
                return Result<LinkDto>.Success(Map(link));
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
            return Result<LinkDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.LinkIdField));
        }
    }

    public async Task<Result<LinkDto>> ChangeLinkVisibility(long linkId, bool isPublic, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<LinkDto>.Failure(LinkErrors.VisibilityNotAuthenticated());
        }

        if (linkId <= 0)
        {
            return Result<LinkDto>.Failure(LinkErrors.LinkIdInvalid());
        }

        try
        {
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
            ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
            bool committed = false;
            try
            {
                DomainEntities.Link? link = await linkRepository.GetById(linkId, _currentUser.UserId, cancellationToken);
                if (link is null)
                {
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (isPublic && !collection.IsPublic)
                {
                    return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
                }

                link.ChangePrivacy(isPublic, collection);
                await linkRepository.Update(link, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;
                return Result<LinkDto>.Success(Map(link));
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
            return Result<LinkDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.LinkIdField));
        }
    }

    public async Task<Result> DeleteLink(long linkId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(LinkErrors.DeleteNotAuthenticated());
        }

        if (linkId <= 0)
        {
            return Result.Failure(LinkErrors.DeleteNotAccessible());
        }

        try
        {
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ILinkRepository repository = unitOfWork.GetRepository<ILinkRepository>();
            bool committed = false;
            try
            {
                DomainEntities.Link? link = await repository.GetById(linkId, _currentUser.UserId, cancellationToken);
                if (link is null)
                {
                    return Result.Failure(LinkErrors.DeleteNotAccessible());
                }

                await repository.Delete(linkId, _currentUser.UserId, cancellationToken);
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
                LinkErrors.LinkIdField));
        }
    }

    public async Task<Result<IReadOnlyList<LinkDto>>> ListLinks(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.ListNotAuthenticated());
        }

        if (page < 1)
        {
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.PageInvalid());
        }

        if (collectionId is not null && collectionId <= 0)
        {
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.CollectionIdInvalid());
        }

        if (collectionId is not null)
        {
            DomainEntities.Collection? collection = await _collectionRepository.GetById(collectionId.Value, _currentUser.UserId, cancellationToken);
            if (collection is null)
            {
                return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.CollectionNotAccessible());
            }
        }

        string? trimmedQuery = query is null ? null : query.Trim();
        IReadOnlyCollection<LinkDto> items = await _linkRepository.GetPageByUser(_currentUser.UserId, trimmedQuery, collectionId, page, cancellationToken);
        return Result<IReadOnlyList<LinkDto>>.Success(items.ToList());
    }

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
}
