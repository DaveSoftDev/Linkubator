using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Contracts;

public interface ICollectionService
{
    Task<Result<CollectionDto>> CreateCollection(CreateCollectionRequest request, CancellationToken cancellationToken = default);

    Task<Result<CollectionDto>> RenameCollection(long collectionId, string name, CancellationToken cancellationToken = default);

    Task<Result<CollectionDto>> ChangeCollectionVisibility(long collectionId, bool isPublic, CancellationToken cancellationToken = default);

    Task<Result> DeleteCollection(long collectionId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CollectionDto>>> ListCollections(string? query = null, int page = 1, CancellationToken cancellationToken = default);
}
