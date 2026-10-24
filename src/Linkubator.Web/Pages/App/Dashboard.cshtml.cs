using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationResults = Linkubator.Application.Results;
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
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile)
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
            Action = "/app/enlaces",
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
        Cards = dashboard.RecentLinks.Select(link => new LinkCardViewModel
        {
            Title = string.IsNullOrWhiteSpace(link.Title) ? link.UrlOriginal : link.Title,
            CollectionName = dashboard.Collections.FirstOrDefault(collection => collection.Id == link.CollectionId)?.Name ?? string.Empty,
            Description = link.Description ?? string.Empty,
            ImageUrl = link.Image,
            AccentClass = Linkubator.Web.Models.AppDashboard.LinkCardViewModelFactory.ResolveAccent(dashboard.Collections.FirstOrDefault(collection => collection.Id == link.CollectionId)?.Name ?? string.Empty),
            Url = link.UrlOriginal,
        }).ToList();

        Sidebar = CreateSidebar(AppSidebarSelectionKind.Home, dashboard.Collections);
        
        return Page();
    }
}
