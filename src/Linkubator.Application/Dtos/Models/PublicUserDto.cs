namespace Linkubator.Application;

public sealed record PublicUserDto(
    string Alias,
    IReadOnlyList<CollectionDto> PublicCollections,
    IReadOnlyList<LinkDto> RecentLinks);
