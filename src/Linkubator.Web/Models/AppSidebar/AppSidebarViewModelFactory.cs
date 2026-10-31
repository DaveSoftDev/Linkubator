using CollectionDto = Linkubator.Application.Dtos.Models.CollectionDto;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using SearchFormViewModel = Linkubator.Web.Models.Presentation.SearchFormViewModel;

namespace Linkubator.Web.Models.AppSidebar;

public static class AppSidebarViewModelFactory
{
    /// <summary>
    /// Construye la barra lateral con las colecciones y selección actuales.
    /// </summary>
    public static AppSidebarViewModel CreateSidebar(
        AppSidebarSelectionKind selectionKind,
        IReadOnlyList<CollectionDto> collections,
        AppSidebarUserViewModel? user = null,
        long? selectedCollectionId = null)
    {
        AppSidebarUserViewModel sidebarUser = user ?? CreateFallbackUser();
        return new AppSidebarViewModelBuilder(selectionKind, collections, sidebarUser, selectedCollectionId).Build();
    }

    private sealed class AppSidebarViewModelBuilder
    {
        private readonly AppSidebarSelectionKind _selectionKind;
        private readonly IReadOnlyList<CollectionDto> _collections;
        private readonly AppSidebarUserViewModel _user;
        private readonly long? _selectedCollectionId;

        /// <summary>
        /// Inicializa el constructor del modelo de barra lateral con el estado de navegación indicado.
        /// </summary>
        public AppSidebarViewModelBuilder(
            AppSidebarSelectionKind selectionKind,
            IReadOnlyList<CollectionDto> collections,
            AppSidebarUserViewModel user,
            long? selectedCollectionId)
        {
            _selectionKind = selectionKind;
            _collections = collections;
            _user = user;
            _selectedCollectionId = selectedCollectionId;
        }

        /// <summary>
        /// Construye el modelo completo de la barra lateral.
        /// </summary>
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

        /// <summary>
        /// Construye el formulario de búsqueda de enlaces de la barra lateral.
        /// </summary>
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

        /// <summary>
        /// Construye los elementos de navegación para las colecciones del usuario.
        /// </summary>
        private IReadOnlyList<AppSidebarCollectionItem> BuildCollectionItems()
        {
            return _collections.Select(collection => new AppSidebarCollectionItem
            {
                Id = collection.Slug,
                Name = collection.Name,
                Route = $"{AppRoutes.Links}?c={collection.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                LinkCount = collection.LinkCount,
                IsPublic = collection.IsPublic,
                IsActive = collection.Id == _selectedCollectionId,
            }).ToList();
        }

        /// <summary>
        /// Construye el estado de selección actual de la barra lateral.
        /// </summary>
        private AppSidebarSelection BuildSelection()
        {
            CollectionDto? selected = _selectionKind == AppSidebarSelectionKind.Collection
                ? _collections.FirstOrDefault(collection => collection.Id == _selectedCollectionId)
                : null;
            return new AppSidebarSelection
            {
                Kind = _selectionKind,
                ItemId = _collections.FirstOrDefault(collection => collection.Id == _selectedCollectionId)?.Slug,
                CollectionId = selected?.Id,
            };
        }

        /// <summary>
        /// Construye los elementos principales de navegación con sus recuentos.
        /// </summary>
        private AppSidebarNavItem[] BuildNavigationItems(int totalLinks, int totalCollections)
        {
            return
            [
                BuildNavigationItem("home", "Inicio", AppRoutes.Root, null),
                BuildNavigationItem("links", "Mis Enlaces", AppRoutes.Links, totalLinks),
                BuildNavigationItem("collections", "Mis colecciones", AppRoutes.Collections, totalCollections),
            ];
        }

        /// <summary>
        /// Construye un elemento de navegación con la etiqueta, ruta y estado indicados.
        /// </summary>
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

        /// <summary>
        /// Determina si el elemento de navegación indicado está seleccionado.
        /// </summary>
        private bool IsNavigationItemActive(string id)
        {
            return id switch
            {
                "home" => _selectionKind == AppSidebarSelectionKind.Home,
                "links" => _selectionKind == AppSidebarSelectionKind.Links,
                "collections" => _selectionKind == AppSidebarSelectionKind.Collections,
                _ => false,
            };
        }
    }

    /// <summary>
    /// Crea los datos de usuario de respaldo para la barra lateral.
    /// </summary>
    private static AppSidebarUserViewModel CreateFallbackUser()
    {
        return new AppSidebarUserViewModel
        {
            Name = "No User",
            Alias = "no-user-exception",
        };
    }
}
