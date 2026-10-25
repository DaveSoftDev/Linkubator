namespace Linkubator.Web.Models.AppSidebar;

public sealed class AppSidebarCollectionItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int LinkCount { get; init; }
    public bool IsPublic { get; init; }
    public string Route { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
