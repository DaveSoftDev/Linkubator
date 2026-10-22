using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Contracts;

public interface ICollectionService
{
    Task<Result<CollectionDto>> GetCollection(CollectionGetRequest request, CancellationToken cancellationToken = default);

    Task<Result<CollectionDto>> CreateCollection(CollectionCreateRequest request, CancellationToken cancellationToken = default);

    Task<Result<CollectionDto>> RenameCollection(CollectionRenameRequest request, CancellationToken cancellationToken = default);

    Task<Result<CollectionDto>> ChangeCollectionVisibility(CollectionChangeVisibilityRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteCollection(CollectionDeleteRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CollectionDto>>> ListCollections(string? query = null, int page = 1, CancellationToken cancellationToken = default);
}
