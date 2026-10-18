namespace Linkubator.Application.Dtos.Models;

public sealed record DashboardDto(int CollectionCount, int LinkCount, IReadOnlyList<CollectionDto> Collections, IReadOnlyList<LinkDto> RecentLinks);
