namespace Linkubator.Web.Models.AppSidebar;

public sealed class AppSidebarNavItem
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public int? Count { get; internal set; }
    public bool IsActive { get; init; }
}
