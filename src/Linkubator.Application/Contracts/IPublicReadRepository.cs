using Linkubator.Application.Dtos.Models;

namespace Linkubator.Application.Contracts;

public interface IPublicReadRepository
{
    Task<PublicUserDto?> GetUserPage(string alias, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PublicCollectionSummaryDto>?> GetCollectionsPage(
        string alias,
        CancellationToken cancellationToken = default);

    Task<PublicCollectionDto?> GetCollectionPage(
        string alias,
        string slug,
        int page,
        CancellationToken cancellationToken = default);

}
