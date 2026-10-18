using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Contracts;

public interface ILinkService
{
    Task<Result<LinkDto>> CreateLink(LinkCreateRequest request, CancellationToken cancellationToken = default);

    Task<Result<LinkDto>> EditLink(LinkEditRequest request, CancellationToken cancellationToken = default);

    Task<Result<LinkDto>> MoveLink(LinkMoveRequest request, CancellationToken cancellationToken = default);

    Task<Result<LinkDto>> ChangeLinkVisibility(LinkChangeVisibilityRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteLink(LinkDeleteRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LinkDto>>> ListLinks(string? query = null, long? collectionId = null, int page = 1, CancellationToken cancellationToken = default);
}
