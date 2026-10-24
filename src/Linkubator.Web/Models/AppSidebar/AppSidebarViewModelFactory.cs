namespace Linkubator.Web.Models.AppSidebar;

public static class AppSidebarViewModelFactory
{
    public static AppSidebarViewModel CreateSidebar(
        AppSidebarSelectionKind selectionKind,
        Application.Dtos.Models.CollectionDto[] collections,
        AppSidebarUserViewModel? user = null)
    {
        AppSidebarUserViewModel sidebarUser = user ?? CreateNoUser();
        AppSidebarViewModel sidebarViewModel = new AppSidebarViewModel
        {
            User = sidebarUser,
            SidebarSearchForm = new()
            {
                Action = "/app/enlaces",
                Label = "Buscar enlaces",
                Placeholder = "Buscar enlaces...",
                ButtonText = "Buscar",
                QueryInputId = "sidebar-search-links",
            },
            Navigation =
            [
                new AppSidebarNavItem { Id = "home", Label = "Inicio", Route = "/app", IsActive = true, Count = null },
                new AppSidebarNavItem { Id = "links", Label = "Mis Enlaces", Route = "/app/enlaces", IsActive = false, Count = -1 },
                new AppSidebarNavItem { Id = "collections", Label = "Mis colecciones", Route = "/app/colecciones", IsActive = false, Count = collections.Count() },
            ],
            Collections = collections.Select(collection => new AppSidebarCollectionItem
            {
                Id = collection.Slug,
                Name = collection.Name,
                Route = $"/app/colecciones/{collection.Slug}",
                LinkCount = collection.LinkCount,
                IsActive = false,
            }).ToList(),
            Selection = new AppSidebarSelection { Kind = selectionKind, ItemId = null },
            IsCollapsed = false,
            IsMobileOpen = false,
        };

        AppSidebarNavItem? linksItem = sidebarViewModel.Navigation.FirstOrDefault(item => item.Id == "links");
        if (linksItem is not null)
        {
            linksItem.Count = sidebarViewModel.TotalLinks;
        }

        return sidebarViewModel;
    }

    public static AppSidebarViewModel CreateDefault(AppSidebarUserViewModel? user = null)
    {
        AppSidebarSelection selection = new()
        {
            Kind = AppSidebarSelectionKind.Home,
            ItemId = null,
        };

        AppSidebarUserViewModel sidebarUser = user ?? CreateNoUser();

        AppSidebarViewModel viewModel = new()
        {
            User = sidebarUser,
            SidebarSearchForm = new()
            {
                Action = "/app/enlaces",
                Label = "Buscar enlaces",
                Placeholder = "Buscar enlaces...",
                ButtonText = "Buscar",
                QueryInputId = "sidebar-search-links",
            },
            Navigation =
            [
                new AppSidebarNavItem { Id = "home", Label = "Inicio", Route = "/app", IsActive = true, Count = null },
                new AppSidebarNavItem { Id = "links", Label = "Mis Enlaces", Route = "/app/enlaces", IsActive = false, Count = 0 },
                new AppSidebarNavItem { Id = "collections", Label = "Mis colecciones", Route = "/app/colecciones", IsActive = false, Count = 0 },
            ],
            /*
            Collections =
            [
                new AppSidebarCollectionItem { Id = "resources-dev", Name = "Recursos Dev", Route = "/app/colecciones/recursos-dev", LinkCount = 24, IsActive = false },
                new AppSidebarCollectionItem { Id = "design-uiux", Name = "Diseño UI/UX", Route = "/app/colecciones/diseno-uiux", LinkCount = 18, IsActive = false },
            ],
            */
            Selection = selection,
            IsCollapsed = false,
            IsMobileOpen = false,
        };

        return viewModel;
    }

    private static AppSidebarUserViewModel CreateNoUser()
    {
        return new AppSidebarUserViewModel
        {
            Name = "No User",
            Alias = "no-user-exception",
        };
    }
}
