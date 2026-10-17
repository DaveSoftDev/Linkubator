namespace Linkubator.Application;

public interface IPublicReadService
{
    Task<Result<PublicUserDto>> GetUserPage(string alias, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PublicCollectionSummaryDto>>> GetCollectionsPage(string alias, CancellationToken cancellationToken = default);

    Task<Result<PublicCollectionDto>> GetCollectionPage(string alias, string slug, int page = 1, CancellationToken cancellationToken = default);

    Task<Result<int>> GetCollectionPageCount(string alias, string slug, CancellationToken cancellationToken = default);
}
