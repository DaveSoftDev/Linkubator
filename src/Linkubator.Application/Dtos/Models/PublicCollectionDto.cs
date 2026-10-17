namespace Linkubator.Application;

public sealed record PublicCollectionDto(
    string Alias,
    string CollectionName,
    string Slug,
    IReadOnlyList<PublicLinkDto> Links);
