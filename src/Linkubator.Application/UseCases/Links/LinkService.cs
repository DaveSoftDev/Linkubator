using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Logging;
using Linkubator.Application.Validation.Links;
using DomainEntities = Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.UseCases.Links;

public sealed class LinkService : ILinkService
{
    private readonly ICurrentUser _currentUser;
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILinkRepository _linkRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IValidator<LinkCreateRequest> _createLinkValidator;
    private readonly IValidator<LinkEditRequest> _editLinkValidator;
    private readonly IValidator<LinkMoveRequest> _moveLinkValidator;
    private readonly IValidator<LinkChangeVisibilityRequest> _changeLinkVisibilityValidator;
    private readonly IValidator<LinkDeleteRequest> _deleteLinkValidator;
    private readonly ILogger<LinkService> _logger;

    public LinkService(
        ILogger<LinkService> logger,
        ICurrentUser currentUser,
        ICollectionRepository collectionRepository,
        ILinkRepository linkRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        LinkServiceValidators? validators = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(collectionRepository);
        ArgumentNullException.ThrowIfNull(linkRepository);
        ArgumentNullException.ThrowIfNull(unitOfWorkFactory);

        LinkServiceValidators resolvedValidators = validators ?? new LinkServiceValidators();

        _logger = logger;
        _currentUser = currentUser;
        _collectionRepository = collectionRepository;
        _linkRepository = linkRepository;
        _unitOfWorkFactory = unitOfWorkFactory;
        _createLinkValidator = resolvedValidators.CreateLinkValidator;
        _editLinkValidator = resolvedValidators.EditLinkValidator;
        _moveLinkValidator = resolvedValidators.MoveLinkValidator;
        _changeLinkVisibilityValidator = resolvedValidators.ChangeLinkVisibilityValidator;
        _deleteLinkValidator = resolvedValidators.DeleteLinkValidator;
    }

    public async Task<Result<LinkDto>> CreateLink(LinkCreateRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _createLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation validation failed for user", new { UserId = _currentUser.UserId }, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.CreateNotAuthenticated());
        }

        try
        {
            string normalizedUrl = DomainPolicies.UrlNormalizationPolicy.Normalize(DomainPolicies.LinkUrlPolicy.Adjust(request.Url));
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
            ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();
            bool committed = false;
            try
            {
                DomainEntities.Collection? collection = await collectionRepository.GetById(request.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { CollectionId = request.CollectionId });
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (request.IsPublic && !collection.IsPublic)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; public link in private collection", new { UserId = _currentUser.UserId }, request);
                    return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
                }

                DomainEntities.Link? duplicate = await linkRepository.GetByNormalizedUrl(_currentUser.UserId, normalizedUrl, cancellationToken);
                if (duplicate is not null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; duplicate URL in collection", new { UserId = _currentUser.UserId }, request);
                    DomainEntities.Collection? duplicateCollection = await collectionRepository.GetById(duplicate.CollectionId, _currentUser.UserId, cancellationToken);
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

                await linkRepository.Insert(created, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;

                SafeLogger.Log(_logger, LogLevel.Information, null, "Link created", new { UserId = _currentUser.UserId });
                return Result<LinkDto>.Success(Map(created));
            }
            finally
            {
                if (!committed)
                {
                    await unitOfWork.Rollback(cancellationToken);
                }
            }

        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link creation failed for user; domain validation issue", new { UserId = _currentUser.UserId }, request);
            return exception.FailDomain<LinkDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link creation failed for user", new { UserId = _currentUser.UserId }, request);
            throw;
        }
    }

    public async Task<Result<LinkDto>> EditLink(LinkEditRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _editLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit validation failed for user", new { UserId = _currentUser.UserId }, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.EditNotAuthenticated());
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
                DomainEntities.Link? link = await linkRepository.GetById(request.LinkId, _currentUser.UserId, cancellationToken);
                if (link is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit rejected for user; link not accessible", new { UserId = _currentUser.UserId }, new { LinkId = request.LinkId });
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { LinkId = request.LinkId, CollectionId = link.CollectionId });
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (request.IsPublic == true && !collection.IsPublic)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit rejected for user; public link in private collection", new { UserId = _currentUser.UserId }, request);
                    return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
                }

                link.Edit(request.Title, request.Description, request.Image);
                if (request.IsPublic is bool requestedVisibility)
                {
                    link.ChangePrivacy(requestedVisibility, collection);
                }

                await linkRepository.Update(link, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;

                SafeLogger.Log(_logger, LogLevel.Information, null, "Link edited", new { UserId = _currentUser.UserId });
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
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link edit failed for user; domain validation issue", new { UserId = _currentUser.UserId }, request);
            return exception.FailDomain<LinkDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link edit failed for user", new { UserId = _currentUser.UserId }, request);
            throw;
        }
    }

    public async Task<Result<LinkDto>> MoveLink(LinkMoveRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _moveLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move validation failed for user", new { UserId = _currentUser.UserId }, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.MoveNotAuthenticated());
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
                DomainEntities.Link? link = await linkRepository.GetById(request.LinkId, _currentUser.UserId, cancellationToken);
                if (link is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move rejected for user; link not accessible", new { UserId = _currentUser.UserId }, new { LinkId = request.LinkId });
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? destinationCollection = await collectionRepository.GetById(request.DestinationCollectionId, _currentUser.UserId, cancellationToken);
                if (destinationCollection is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move rejected for user; destination collection not accessible", new { UserId = _currentUser.UserId }, new { LinkId = request.LinkId, CollectionId = request.DestinationCollectionId });
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                link.MoveToCollection(destinationCollection);
                await linkRepository.Update(link, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;

                SafeLogger.Log(_logger, LogLevel.Information, null, "Link moved", new { UserId = _currentUser.UserId });
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
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link move failed for user; domain validation issue", new { UserId = _currentUser.UserId }, request);
            return exception.FailDomain<LinkDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link move failed for user", new { UserId = _currentUser.UserId }, request);
            throw;
        }
    }

    public async Task<Result<LinkDto>> ChangeLinkVisibility(LinkChangeVisibilityRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _changeLinkVisibilityValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change validation failed for user", new { UserId = _currentUser.UserId }, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.VisibilityNotAuthenticated());
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
                DomainEntities.Link? link = await linkRepository.GetById(request.LinkId, _currentUser.UserId, cancellationToken);
                if (link is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change rejected for user; link not accessible", new { UserId = _currentUser.UserId }, new { LinkId = request.LinkId });
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { LinkId = request.LinkId, CollectionId = link.CollectionId });
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (request.IsPublic && !collection.IsPublic)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change rejected for user; public link in private collection", new { UserId = _currentUser.UserId }, request);
                    return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
                }

                link.ChangePrivacy(request.IsPublic, collection);
                await linkRepository.Update(link, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;

                SafeLogger.Log(_logger, LogLevel.Information, null, "Link visibility changed", new { UserId = _currentUser.UserId });
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
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link visibility change failed for user; domain validation issue", new { UserId = _currentUser.UserId }, request);
            return exception.FailDomain<LinkDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link visibility change failed for user", new { UserId = _currentUser.UserId }, request);
            throw;
        }
    }

    public async Task<Result> DeleteLink(LinkDeleteRequest request, CancellationToken cancellationToken = default)
    {
        Result validation = _deleteLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link deletion validation failed for user", new { UserId = _currentUser.UserId }, request);
            return Result.Failure(validation.Errors);
        }
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link deletion rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result.Failure(LinkErrors.DeleteNotAuthenticated());
        }

        try
        {
            await using IUnitOfWork unitOfWork = _unitOfWorkFactory.Get(UnitOfWorkContexts.Linkubator);
            await unitOfWork.Begin(cancellationToken);
            ILinkRepository repository = unitOfWork.GetRepository<ILinkRepository>();
            bool committed = false;
            try
            {
                DomainEntities.Link? link = await repository.GetById(request.LinkId, _currentUser.UserId, cancellationToken);
                if (link is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link deletion rejected for user; link not accessible", new { UserId = _currentUser.UserId, LinkId = request.LinkId }, null);
                    return Result.Failure(LinkErrors.DeleteNotAccessible());
                }

                await repository.Delete(request.LinkId, _currentUser.UserId, cancellationToken);
                await unitOfWork.Commit(cancellationToken);
                committed = true;

                SafeLogger.Log(_logger, LogLevel.Information, null, "Link deleted", new { UserId = _currentUser.UserId });
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
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link deletion failed for user; domain validation issue", new { UserId = _currentUser.UserId }, request);
            return exception.FailDomain();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link deletion failed for user", new { UserId = _currentUser.UserId }, request);
            throw;
        }
    }

    public async Task<Result<IReadOnlyList<LinkDto>>> ListLinks(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link listing rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.ListNotAuthenticated());
        }

        if (page < 1)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link listing validation failed for user; invalid page", new { UserId = _currentUser.UserId }, new { Page = page });
            return Result<IReadOnlyList<LinkDto>>.Failure(LinkErrors.PageInvalid());
        }

        if (collectionId is not null && collectionId <= 0)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link listing validation failed for user; invalid collection ID", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
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
                SafeLogger.Log(_logger, LogLevel.Error, exception, "Link listing collection check failed for user; collection ID", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
                throw;
            }

            if (collection is null)
            {
                SafeLogger.Log(_logger, LogLevel.Warning, null, "Link listing rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { CollectionId = collectionId });
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
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link listing failed for user; collection ID", new { UserId = _currentUser.UserId }, new { Page = page, CollectionId = collectionId });
            throw;
        }

        SafeLogger.Log(_logger, LogLevel.Information, null, "Links listed for user", new { UserId = _currentUser.UserId });
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
