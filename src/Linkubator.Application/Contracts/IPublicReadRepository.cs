namespace Linkubator.Application;

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

    Task<int> GetCollectionPageCount(string alias, string slug, CancellationToken cancellationToken = default);
}
