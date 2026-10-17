using DomainEntities = Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Linkubator.Application.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Application;

public sealed class LinkService : ILinkService
{
    private readonly ICurrentUser _currentUser;
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILinkRepository _linkRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IValidator<CreateLinkRequest> _createLinkValidator;
    private readonly ILogger<LinkService> _logger;

    public LinkService(
        ICurrentUser currentUser,
        ICollectionRepository collectionRepository,
        ILinkRepository linkRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        IValidator<CreateLinkRequest>? createLinkValidator = null,
        ILogger<LinkService>? logger = null)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        _linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));
        _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        _createLinkValidator = createLinkValidator ?? new LinkCreateValidator();
        _logger = logger ?? NullLogger<LinkService>.Instance;
    }

    public async Task<Result<LinkDto>> CreateLink(CreateLinkRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Link creation rejected for user {UserId}: not authenticated", _currentUser.UserId);
            return Result<LinkDto>.Failure(LinkErrors.CreateNotAuthenticated());
        }

        if (request is null)
        {
            _logger.LogWarning("Link creation rejected for user {UserId}: request was null", _currentUser.UserId);
            return Result<LinkDto>.Failure(LinkErrors.RequestRequired());
        }

        Result validation = _createLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            _logger.LogWarning("Link creation validation failed for user {UserId}. ErrorCount {ErrorCount}", _currentUser.UserId, validation.Errors.Count);
            return Result<LinkDto>.Failure(validation.Errors);
        }

        try
        {
            DomainEntities.Collection? collection = await _collectionRepository.GetById(request.CollectionId, _currentUser.UserId, cancellationToken);
            if (collection is null)
            {
                _logger.LogWarning("Link creation rejected for user {UserId}: collection not accessible", _currentUser.UserId);
                return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
            }

            if (request.IsPublic && !collection.IsPublic)
            {
                _logger.LogWarning("Link creation rejected for user {UserId}: public link in private collection", _currentUser.UserId);
                return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
            }

            string normalizedUrl = DomainPolicies.UrlNormalizationPolicy.Normalize(
                DomainPolicies.LinkUrlPolicy.Adjust(request.Url));

            DomainEntities.Link? duplicate = await _linkRepository.GetByNormalizedUrl(_currentUser.UserId, normalizedUrl, cancellationToken);
            if (duplicate is not null)
            {
                _logger.LogWarning("Link creation rejected for user {UserId}: duplicate URL in collection {CollectionId}", _currentUser.UserId, duplicate.CollectionId);
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
            _logger.LogInformation("Link created for user {UserId}. LinkId {LinkId} CollectionId {CollectionId} IsPublic {IsPublic}", _currentUser.UserId, created.Id, created.CollectionId, created.IsPublic);
            return Result<LinkDto>.Success(Map(created));
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            _logger.LogWarning("Link creation failed for user {UserId}: domain validation issue", _currentUser.UserId);
            return Result<LinkDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.CollectionIdField));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Link creation failed for user {UserId}. CollectionId {CollectionId}", _currentUser.UserId, request.CollectionId);
            throw;
        }
    }

    public async Task<Result<LinkDto>> EditLink(long linkId, string? title = null, string? description = null, string? image = null, bool? isPublic = null, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Link edit rejected for user {UserId}: not authenticated", _currentUser.UserId);
            return Result<LinkDto>.Failure(LinkErrors.EditNotAuthenticated());
        }

        if (linkId <= 0)
        {
            _logger.LogWarning("Link edit validation failed for user {UserId}: invalid link id {LinkId}", _currentUser.UserId, linkId);
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
                    _logger.LogWarning("Link edit rejected for user {UserId}: link not accessible", _currentUser.UserId);
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    _logger.LogWarning("Link edit rejected for user {UserId}: collection not accessible", _currentUser.UserId);
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (isPublic == true && !collection.IsPublic)
                {
                    _logger.LogWarning("Link edit rejected for user {UserId}: public link in private collection", _currentUser.UserId);
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
                _logger.LogInformation("Link edited for user {UserId}. LinkId {LinkId} CollectionId {CollectionId} IsPublic {IsPublic}", _currentUser.UserId, link.Id, link.CollectionId, link.IsPublic);
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
            _logger.LogWarning("Link edit failed for user {UserId}: domain validation issue", _currentUser.UserId);
            return Result<LinkDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.LinkIdField));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Link edit failed for user {UserId}. LinkId {LinkId}", _currentUser.UserId, linkId);
            throw;
        }
    }

    public async Task<Result<LinkDto>> MoveLink(long linkId, long destinationCollectionId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Link move rejected for user {UserId}: not authenticated", _currentUser.UserId);
            return Result<LinkDto>.Failure(LinkErrors.MoveNotAuthenticated());
        }

        if (linkId <= 0)
        {
            _logger.LogWarning("Link move validation failed for user {UserId}: invalid link id {LinkId}", _currentUser.UserId, linkId);
            return Result<LinkDto>.Failure(LinkErrors.LinkIdInvalid());
        }

        if (destinationCollectionId <= 0)
        {
            _logger.LogWarning("Link move validation failed for user {UserId}: invalid destination collection id {CollectionId}", _currentUser.UserId, destinationCollectionId);
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
                    _logger.LogWarning("Link move rejected for user {UserId}: link not accessible", _currentUser.UserId);
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? destinationCollection = await collectionRepository.GetById(destinationCollectionId, _currentUser.UserId, cancellationToken);
                if (destinationCollection is null)
                {
                    _logger.LogWarning("Link move rejected for user {UserId}: destination collection not accessible", _currentUser.UserId);
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                link.MoveToCollection(destinationCollection);
                await linkRepository.Update(link, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;
                _logger.LogInformation("Link moved for user {UserId}. LinkId {LinkId} CollectionId {CollectionId}", _currentUser.UserId, link.Id, link.CollectionId);
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
            _logger.LogWarning("Link move failed for user {UserId}: domain validation issue", _currentUser.UserId);
            return Result<LinkDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.LinkIdField));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Link move failed for user {UserId}. LinkId {LinkId} CollectionId {CollectionId}", _currentUser.UserId, linkId, destinationCollectionId);
            throw;
        }
    }

    public async Task<Result<LinkDto>> ChangeLinkVisibility(long linkId, bool isPublic, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Link visibility change rejected for user {UserId}: not authenticated", _currentUser.UserId);
            return Result<LinkDto>.Failure(LinkErrors.VisibilityNotAuthenticated());
        }

        if (linkId <= 0)
        {
            _logger.LogWarning("Link visibility change validation failed for user {UserId}: invalid link id {LinkId}", _currentUser.UserId, linkId);
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
                    _logger.LogWarning("Link visibility change rejected for user {UserId}: link not accessible", _currentUser.UserId);
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    _logger.LogWarning("Link visibility change rejected for user {UserId}: collection not accessible", _currentUser.UserId);
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (isPublic && !collection.IsPublic)
                {
                    _logger.LogWarning("Link visibility change rejected for user {UserId}: public link in private collection", _currentUser.UserId);
                    return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
                }

                link.ChangePrivacy(isPublic, collection);
                await linkRepository.Update(link, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;
                _logger.LogInformation("Link visibility changed for user {UserId}. LinkId {LinkId} CollectionId {CollectionId} IsPublic {IsPublic}", _currentUser.UserId, link.Id, link.CollectionId, isPublic);
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
            _logger.LogWarning("Link visibility change failed for user {UserId}: domain validation issue", _currentUser.UserId);
            return Result<LinkDto>.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.LinkIdField));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Link visibility change failed for user {UserId}. LinkId {LinkId}", _currentUser.UserId, linkId);
            throw;
        }
    }

    public async Task<Result> DeleteLink(long linkId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Link deletion rejected for user {UserId}: not authenticated", _currentUser.UserId);
            return Result.Failure(LinkErrors.DeleteNotAuthenticated());
        }

        if (linkId <= 0)
        {
            _logger.LogWarning("Link deletion validation failed for user {UserId}: invalid link id {LinkId}", _currentUser.UserId, linkId);
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
                    _logger.LogWarning("Link deletion rejected for user {UserId}: link not accessible", _currentUser.UserId);
                    return Result.Failure(LinkErrors.DeleteNotAccessible());
                }

                await repository.Delete(linkId, _currentUser.UserId, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;
                _logger.LogInformation("Link deleted for user {UserId}. LinkId {LinkId} CollectionId {CollectionId}", _currentUser.UserId, linkId, link.CollectionId);
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
            _logger.LogWarning("Link deletion failed for user {UserId}: domain validation issue", _currentUser.UserId);
            return Result.Failure(ApplicationError.FromDomainException(
                (DomainExceptions.DomainException)exception,
                LinkErrors.LinkIdField));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Link deletion failed for user {UserId}. LinkId {LinkId}", _currentUser.UserId, linkId);
            throw;
        }
    }

    public async Task<Result<IReadOnlyList<LinkDto>>> ListLinks(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Link listing rejected for user {UserId}: not authenticated", _currentUser.UserId);
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.ListNotAuthenticated());
        }

        if (page < 1)
        {
            _logger.LogWarning("Link listing validation failed for user {UserId}: invalid page {Page}", _currentUser.UserId, page);
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.PageInvalid());
        }

        if (collectionId is not null && collectionId <= 0)
        {
            _logger.LogWarning("Link listing validation failed for user {UserId}: invalid collection id {CollectionId}", _currentUser.UserId, collectionId);
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.CollectionIdInvalid());
        }

        if (collectionId is not null)
        {
            DomainEntities.Collection? collection;
            try
            {
                collection = await _collectionRepository.GetById(collectionId.Value, _currentUser.UserId, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError("Link listing collection check failed for user {UserId}. CollectionId {CollectionId}", _currentUser.UserId, collectionId);
                throw;
            }

            if (collection is null)
            {
                _logger.LogWarning("Link listing rejected for user {UserId}: collection not accessible", _currentUser.UserId);
                return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.CollectionNotAccessible());
            }
        }

        string? trimmedQuery = query is null ? null : query.Trim();
        IReadOnlyCollection<LinkDto> items;
        try
        {
            items = await _linkRepository.GetPageByUser(_currentUser.UserId, trimmedQuery, collectionId, page, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Link listing failed for user {UserId}. Page {Page} CollectionId {CollectionId}", _currentUser.UserId, page, collectionId);
            throw;
        }
        _logger.LogInformation("Links listed for user {UserId}. Page {Page} ResultCount {ResultCount}", _currentUser.UserId, page, items.Count);
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
