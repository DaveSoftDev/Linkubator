using CollectionDto = Linkubator.Application.Dtos.Models.CollectionDto;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using SearchFormViewModel = Linkubator.Web.Models.Presentation.SearchFormViewModel;

namespace Linkubator.Web.Models.AppSidebar;

public static class AppSidebarViewModelFactory
{
    public static AppSidebarViewModel CreateSidebar(
        AppSidebarSelectionKind selectionKind,
        IReadOnlyList<CollectionDto> collections,
        AppSidebarUserViewModel? user = null)
    {
        AppSidebarUserViewModel sidebarUser = user ?? CreateFallbackUser();
        return new AppSidebarViewModelBuilder(selectionKind, collections, sidebarUser).Build();
    }

    private sealed class AppSidebarViewModelBuilder
    {
        private readonly AppSidebarSelectionKind _selectionKind;
        private readonly IReadOnlyList<CollectionDto> _collections;
        private readonly AppSidebarUserViewModel _user;

        public AppSidebarViewModelBuilder(
            AppSidebarSelectionKind selectionKind,
            IReadOnlyList<CollectionDto> collections,
            AppSidebarUserViewModel user)
        {
            _selectionKind = selectionKind;
            _collections = collections;
            _user = user;
        }

        public AppSidebarViewModel Build()
        {
            int totalLinks = _collections.Sum(collection => collection.LinkCount);

            return new AppSidebarViewModel
            {
                User = _user,
                SidebarSearchForm = BuildSearchForm(),
                Navigation = BuildNavigationItems(totalLinks, _collections.Count),
                Collections = BuildCollectionItems(),
                Selection = BuildSelection(),
                IsCollapsed = false,
                IsMobileOpen = false,
            };
        }

        private SearchFormViewModel BuildSearchForm()
        {
            return new SearchFormViewModel
            {
                Action = AppRoutes.Links,
                Label = "Buscar enlaces",
                Placeholder = "Buscar enlaces...",
                ButtonText = "Buscar",
                QueryInputId = "sidebar-search-links",
            };
        }

        private IReadOnlyList<AppSidebarCollectionItem> BuildCollectionItems()
        {
            return _collections.Select(collection => new AppSidebarCollectionItem
            {
                Id = collection.Slug,
                Name = collection.Name,
                Route = $"{AppRoutes.Collections}/{collection.Slug}",
                LinkCount = collection.LinkCount,
                IsPublic = collection.IsPublic,
                IsActive = false,
            }).ToList();
        }

        private AppSidebarSelection BuildSelection()
        {
            return new AppSidebarSelection
            {
                Kind = _selectionKind,
                ItemId = null,
            };
        }

        private AppSidebarNavItem[] BuildNavigationItems(int totalLinks, int totalCollections)
        {
            return
            [
                BuildNavigationItem("home", "Inicio", AppRoutes.Root, null),
                BuildNavigationItem("links", "Mis Enlaces", AppRoutes.Links, totalLinks),
                BuildNavigationItem("collections", "Mis colecciones", AppRoutes.Collections, totalCollections),
            ];
        }

        private AppSidebarNavItem BuildNavigationItem(
            string id,
            string label,
            string route,
            int? count)
        {
            return new AppSidebarNavItem
            {
                Id = id,
                Label = label,
                Route = route,
                Count = count,
                IsActive = IsNavigationItemActive(id),
            };
        }

        private bool IsNavigationItemActive(string id)
        {
            return id switch
            {
                "home" => _selectionKind == AppSidebarSelectionKind.Home,
                "links" => _selectionKind == AppSidebarSelectionKind.Links,
                "collections" => _selectionKind == AppSidebarSelectionKind.Collections || _selectionKind == AppSidebarSelectionKind.Collection,
                _ => false,
            };
        }
    }

    private static AppSidebarUserViewModel CreateFallbackUser()
    {
        return new AppSidebarUserViewModel
        {
            Name = "No User",
            Alias = "no-user-exception",
        };
    }
}
