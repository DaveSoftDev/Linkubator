using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationResults = Linkubator.Application.Results;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using LinkCardViewModel = Linkubator.Web.Models.AppDashboard.LinkCardViewModel;
using SearchFormViewModel = Linkubator.Web.Models.Presentation.SearchFormViewModel;
using EmptyStateViewModel = Linkubator.Web.Models.Presentation.EmptyStateViewModel;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App;

public class DashboardModel : PrivatePageModel
{
    private readonly ApplicationContracts.IDashboardService _dashboardService;

    public DashboardModel(
        ApplicationContracts.IDashboardService dashboardService,
        ApplicationContracts.ICollectionService collectionService,
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile, collectionService)
    {
        _dashboardService = dashboardService;
    }

    public IReadOnlyList<LinkCardViewModel> Cards { get; private set; } = [];

    public SearchFormViewModel SearchForm { get; private set; } = new();

    public EmptyStateViewModel EmptyState { get; private set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        SearchForm = new SearchFormViewModel
        {
            Action = AppRoutes.Links,
            Label = "Buscar enlaces",
            Placeholder = "Buscar enlaces...",
            ButtonText = "Buscar",
            QueryInputId = "dashboard-search-links",
        };
        EmptyState = new EmptyStateViewModel
        {
            Title = "Todavía no tienes enlaces guardados.",
            Description = "Añade un enlace para empezar a organizar tus recursos.",
        };

        ApplicationResults.Result<ApplicationDtos.DashboardDto> result = await _dashboardService.GetDashboard(cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return NotFound();
        }

        ApplicationDtos.DashboardDto dashboard = result.Value;
        Cards = dashboard.RecentLinks.Select(link =>
        {
            string collectionName = link.CollectionName ?? string.Empty;
            return new LinkCardViewModel
            {
                Title = string.IsNullOrWhiteSpace(link.Title) ? link.UrlOriginal : link.Title,
                CollectionName = collectionName,
                Description = link.Description ?? string.Empty,
                ImageUrl = link.Image,
                AccentClass = Linkubator.Web.Models.AppDashboard.LinkCardViewModelFactory.ResolveAccent(collectionName),
                Url = link.UrlOriginal,
            };
        }).ToList();

        return Page();
    }
}
