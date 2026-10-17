namespace Linkubator.Application.Dtos.Requests;

public sealed record CreateLinkRequest(
    long CollectionId,
    string Url,
    string? Title = null,
    string? Description = null,
    string? Image = null,
    bool IsPublic = false);
