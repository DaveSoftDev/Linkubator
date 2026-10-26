namespace Linkubator.Web.Models.AppSidebar;

public sealed class AppSidebarSelection
{
    public AppSidebarSelectionKind Kind { get; init; }
    public string? ItemId { get; init; }
    public long? CollectionId { get; init; }
}
