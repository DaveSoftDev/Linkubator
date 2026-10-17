using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Logging;
using Linkubator.Application.Validation;
using DomainEntities = Linkubator.Domain.Entities;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Application.UseCases.Links;

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
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.CreateNotAuthenticated());
        }

        if (request is null)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; request is null", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.RequestRequired());
        }

        Result validation = _createLinkValidator.Validate(request);
        if (!validation.IsSuccess)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation validation failed for user", new { UserId = _currentUser.UserId }, request);
            return Result<LinkDto>.Failure(validation.Errors);
        }

        try
        {
            DomainEntities.Collection? collection = await _collectionRepository.GetById(request.CollectionId, _currentUser.UserId, cancellationToken);
            if (collection is null)
            {
                SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new {CollectionId = request.CollectionId });
                return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
            }

            if (request.IsPublic && !collection.IsPublic)
            {
                SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; public link in private collection", new { UserId = _currentUser.UserId }, request);
                return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
            }

            string normalizedUrl = DomainPolicies.UrlNormalizationPolicy.Normalize(
                DomainPolicies.LinkUrlPolicy.Adjust(request.Url));

            DomainEntities.Link? duplicate = await _linkRepository.GetByNormalizedUrl(_currentUser.UserId, normalizedUrl, cancellationToken);
            if (duplicate is not null)
            {
                SafeLogger.Log(_logger, LogLevel.Warning, null, "Link creation rejected for user; duplicate URL in collection", new { UserId = _currentUser.UserId }, request);
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
            SafeLogger.Log(_logger, LogLevel.Information, null, "Link created", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Success(Map(created));
        }
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
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

    public async Task<Result<LinkDto>> EditLink(long linkId, string? title = null, string? description = null, string? image = null, bool? isPublic = null, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.EditNotAuthenticated());
        }

        if (linkId <= 0)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit validation failed for user; invalid link ID", new { UserId = _currentUser.UserId }, new { LinkId = linkId });
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
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit rejected for user; link not accessible", new { UserId = _currentUser.UserId }, new { LinkId = linkId });
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { LinkId = linkId, CollectionId = link.CollectionId });
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (isPublic == true && !collection.IsPublic)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link edit rejected for user; public link in private collection", new { UserId = _currentUser.UserId }, new { LinkId = linkId, CollectionId = link.CollectionId });
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
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link edit failed for user; domain validation issue", new { UserId = _currentUser.UserId }, new { LinkId = linkId, Title = title, Description = description, Image = image, IsPublic = isPublic });
            return exception.FailDomain<LinkDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link edit failed for user", new { UserId = _currentUser.UserId }, new { LinkId = linkId, Title = title, Description = description, Image = image, IsPublic = isPublic });
            throw;
        }
    }

    public async Task<Result<LinkDto>> MoveLink(long linkId, long destinationCollectionId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.MoveNotAuthenticated());
        }

        if (linkId <= 0)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move validation failed for user; invalid link id", new { UserId = _currentUser.UserId}, new { LinkId = linkId });
            return Result<LinkDto>.Failure(LinkErrors.LinkIdInvalid());
        }

        if (destinationCollectionId <= 0)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move validation failed for user; invalid destination collection id", new { UserId = _currentUser.UserId }, new { CollectionId = destinationCollectionId });
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
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move rejected for user; linkin not accessible", new { UserId = _currentUser.UserId }, new { LinkId = linkId });
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? destinationCollection = await collectionRepository.GetById(destinationCollectionId, _currentUser.UserId, cancellationToken);
                if (destinationCollection is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link move rejected for user; destination collection not accessible", new { UserId = _currentUser.UserId }, new { LinkId = linkId, CollectionId = destinationCollectionId });
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
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link move failed for user; domain validation issue", new { UserId = _currentUser.UserId }, new { LinkId = linkId, CollectionId = destinationCollectionId });
            return exception.FailDomain<LinkDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link move failed for user", new { UserId = _currentUser.UserId }, new { LinkId = linkId, CollectionId = destinationCollectionId });
            throw;
        }
    }

    public async Task<Result<LinkDto>> ChangeLinkVisibility(long linkId, bool isPublic, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<LinkDto>.Failure(LinkErrors.VisibilityNotAuthenticated());
        }

        if (linkId <= 0)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change validation failed for user", new { UserId = _currentUser.UserId }, new { LinkId = linkId });
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
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change rejected for user; link not accessible", new { UserId = _currentUser.UserId }, new { LinkId = linkId });
                    return Result<LinkDto>.Failure(LinkErrors.LinkNotAccessible());
                }

                DomainEntities.Collection? collection = await collectionRepository.GetById(link.CollectionId, _currentUser.UserId, cancellationToken);
                if (collection is null)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change rejected for user; collection not accessible", new { UserId = _currentUser.UserId }, new { LinkId = linkId, CollectionId = link.CollectionId });
                    return Result<LinkDto>.Failure(LinkErrors.CollectionNotAccessible());
                }

                if (isPublic && !collection.IsPublic)
                {
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link visibility change rejected for user; public link in private collection", new { UserId = _currentUser.UserId }, new { LinkId = linkId, IsPublic = isPublic });
                    return Result<LinkDto>.Failure(LinkErrors.PublicLinkInPrivateCollection());
                }

                link.ChangePrivacy(isPublic, collection);
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
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link visibility change failed for user: domain validation issue", new { UserId = _currentUser.UserId }, new { LinkId = linkId, IsPublic = isPublic });
            return exception.FailDomain<LinkDto>();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link visibility change failed for user", new { UserId = _currentUser.UserId }, new { LinkId = linkId, IsPublic = isPublic });
            throw;
        }
    }

    public async Task<Result> DeleteLink(long linkId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link deletion rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result.Failure(LinkErrors.DeleteNotAuthenticated());
        }

        if (linkId <= 0)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Link deletion validation failed for user; invalid link ID", new { UserId = _currentUser.UserId }, new { LinkId = linkId });
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
                    SafeLogger.Log(_logger, LogLevel.Warning, null, "Link deletion rejected for user; link not accessible", new { UserId = _currentUser.UserId, LinkId = linkId }, null);
                    return Result.Failure(LinkErrors.DeleteNotAccessible());
                }

                await repository.Delete(linkId, _currentUser.UserId, cancellationToken);
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
        catch (Exception exception) when (exception is not OperationCanceledException && exception is DomainExceptions.DomainException)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Link deletion failed for user; domain validation issue", new { UserId = _currentUser.UserId }, new { LinkId = linkId });
            return exception.FailDomain();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link deletion failed for user; Link ID", new { UserId = _currentUser.UserId }, new { LinkId = linkId });
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
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Link listing failed for user; collection ID", new { UserId = _currentUser.UserId }, new { Page = page , CollectionId = collectionId });
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
