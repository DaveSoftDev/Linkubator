using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Links;

public sealed record LinkServiceValidators(
    IValidator<LinkGetRequest>? GetLink = null,
    IValidator<LinkCreateRequest>? CreateLink = null,
    IValidator<LinkUpdateRequest>? UpdateLink = null,
    IValidator<LinkMoveRequest>? MoveLink = null,
    IValidator<LinkDeleteRequest>? DeleteLink = null)
{
    public IValidator<LinkGetRequest> GetLinkValidator { get; } = GetLink ?? new LinkGetValidator();
    public IValidator<LinkCreateRequest> CreateLinkValidator { get; } = CreateLink ?? new LinkCreateValidator();
    public IValidator<LinkUpdateRequest> UpdateLinkValidator { get; } = UpdateLink ?? new LinkUpdateValidator();
    public IValidator<LinkMoveRequest> MoveLinkValidator { get; } = MoveLink ?? new LinkMoveValidator();
    public IValidator<LinkDeleteRequest> DeleteLinkValidator { get; } = DeleteLink ?? new LinkDeleteValidator();
}
