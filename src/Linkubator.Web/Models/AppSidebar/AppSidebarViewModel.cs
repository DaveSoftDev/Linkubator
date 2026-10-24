using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using SearchFormViewModel = Linkubator.Web.Models.Presentation.SearchFormViewModel;

namespace Linkubator.Web.Models.AppSidebar;

public sealed class AppSidebarViewModel
{
    public int TotalLinks => Collections.Sum(item => item.LinkCount);

    public AppSidebarUserViewModel User { get; init; } = new();

    public SearchFormViewModel SidebarSearchForm { get; init; } = new()
    {
        Action = AppRoutes.Links,
        Label = "Buscar enlaces",
        Placeholder = "Buscar enlaces...",
        ButtonText = "Buscar",
        QueryInputId = "sidebar-search-links",
    };

    public IReadOnlyList<AppSidebarNavItem> Navigation { get; init; } = [];
    public IReadOnlyList<AppSidebarCollectionItem> Collections { get; init; } = [];
    public AppSidebarSelection Selection { get; init; } = new();
    public bool IsCollapsed { get; init; }
    public bool IsMobileOpen { get; init; }
    public int TotalCollections => Collections.Count;
}
