namespace Linkubator.Web.Models.AppSidebar;

public sealed class AppSidebarViewModel
{
    public int TotalLinks => Collections.Sum(item => item.LinkCount);

    public AppSidebarUserViewModel User { get; init; } = new();
    public IReadOnlyList<AppSidebarNavItem> Navigation { get; init; } = [];
    public IReadOnlyList<AppSidebarCollectionItem> Collections { get; init; } = [];
    public AppSidebarSelection Selection { get; init; } = new();
    public bool IsCollapsed { get; init; }
    public bool IsMobileOpen { get; init; }
    public int TotalCollections => Collections.Count;
}
