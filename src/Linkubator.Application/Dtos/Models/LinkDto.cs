namespace Linkubator.Application;

public sealed record LinkDto(
    long Id,
    long CollectionId,
    string UrlOriginal,
    string UrlNormalized,
    string? Title,
    string? Description,
    string? Image,
    bool IsPublic,
    DateTime CreatedAt);
