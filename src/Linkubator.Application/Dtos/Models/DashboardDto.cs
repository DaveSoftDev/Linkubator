namespace Linkubator.Application;

public sealed record DashboardDto(
    int CollectionCount,
    int LinkCount,
    IReadOnlyList<CollectionDto> Collections,
    IReadOnlyList<LinkDto> RecentLinks);
