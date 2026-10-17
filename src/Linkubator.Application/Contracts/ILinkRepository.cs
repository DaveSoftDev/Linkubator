using Entities = Linkubator.Domain.Entities;

namespace Linkubator.Application;

public interface ILinkRepository
{
    Task<Entities.Link?> GetById(long id, long userId, CancellationToken cancellationToken = default);

    Task<Entities.Link?> GetByNormalizedUrl(long userId, string normalizedUrl, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Entities.Link>> GetAllByUser(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Entities.Link>> GetAllByCollection(long userId, long collectionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LinkDto>> GetPageByUser(
        long userId,
        string? query,
        long? collectionId,
        int page,
        CancellationToken cancellationToken = default);

    Task<int> GetPageCountByUser(
        long userId,
        string? query,
        long? collectionId,
        CancellationToken cancellationToken = default);

    Task Insert(Entities.Link link, CancellationToken cancellationToken = default);

    Task Update(Entities.Link link, CancellationToken cancellationToken = default);

    Task Delete(long id, long userId, CancellationToken cancellationToken = default);
}
