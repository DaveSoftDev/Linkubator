namespace Linkubator.Application;

public sealed record PublicUserDto(
    string Alias,
    IReadOnlyList<PublicCollectionSummaryDto> PublicCollections,
    IReadOnlyList<PublicLinkDto> RecentLinks);
