using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Links;

public sealed record LinkServiceValidators(
    IValidator<LinkCreateRequest>? CreateLink = null,
    IValidator<LinkEditRequest>? EditLink = null,
    IValidator<LinkMoveRequest>? MoveLink = null,
    IValidator<LinkChangeVisibilityRequest>? ChangeLinkVisibility = null,
    IValidator<LinkDeleteRequest>? DeleteLink = null)
{
    public IValidator<LinkCreateRequest> CreateLinkValidator { get; } = CreateLink ?? new LinkCreateValidator();
    public IValidator<LinkEditRequest> EditLinkValidator { get; } = EditLink ?? new LinkEditValidator();
    public IValidator<LinkMoveRequest> MoveLinkValidator { get; } = MoveLink ?? new LinkMoveValidator();
    public IValidator<LinkChangeVisibilityRequest> ChangeLinkVisibilityValidator { get; } = ChangeLinkVisibility ?? new LinkChangeVisibilityValidator();
    public IValidator<LinkDeleteRequest> DeleteLinkValidator { get; } = DeleteLink ?? new LinkDeleteValidator();
}
