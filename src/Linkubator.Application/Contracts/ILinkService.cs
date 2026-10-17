using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Contracts;

public interface ILinkService
{
    Task<Result<LinkDto>> CreateLink(CreateLinkRequest request, CancellationToken cancellationToken = default);

    Task<Result<LinkDto>> EditLink(long linkId, string? title = null, string? description = null, string? image = null, bool? isPublic = null, CancellationToken cancellationToken = default);

    Task<Result<LinkDto>> MoveLink(long linkId, long destinationCollectionId, CancellationToken cancellationToken = default);

    Task<Result<LinkDto>> ChangeLinkVisibility(long linkId, bool isPublic, CancellationToken cancellationToken = default);

    Task<Result> DeleteLink(long linkId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LinkDto>>> ListLinks(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default);
}
