namespace Linkubator.Application.Dtos.Models;

public sealed record PublicUserDto(
    string Alias,
    IReadOnlyList<PublicCollectionSummaryDto> PublicCollections,
    IReadOnlyList<PublicLinkDto> RecentLinks);
