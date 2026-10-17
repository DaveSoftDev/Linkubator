namespace Linkubator.Application;

public sealed record PublicLinkDto(
    string UrlOriginal,
    string? Title,
    string? Description,
    string? Image);
