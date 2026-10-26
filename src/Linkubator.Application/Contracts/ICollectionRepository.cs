using Linkubator.Application.Dtos.Models;
using Entities = Linkubator.Domain.Entities;

namespace Linkubator.Application.Contracts;

public interface ICollectionRepository
{
    Task<Entities.Collection?> GetById(long id, long userId, CancellationToken cancellationToken = default);

    Task<Entities.Collection?> GetBySlug(long userId, string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Entities.Collection>> GetAllByUser(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CollectionDto>> GetAllByUserWithLinkCounts(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CollectionDto>> GetPageByUser(
        long userId,
        string? query,
        int page,
        CancellationToken cancellationToken = default);

    Task<int> GetPageCountByUser(long userId, string? query, CancellationToken cancellationToken = default);

    Task Insert(Entities.Collection collection, CancellationToken cancellationToken = default);

    Task Update(Entities.Collection collection, CancellationToken cancellationToken = default);

    Task Delete(long id, long userId, CancellationToken cancellationToken = default);
}
