using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationResults = Linkubator.Application.Results;
using ApplicationRequests = Linkubator.Application.Dtos.Requests;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using LinkCardViewModel = Linkubator.Web.Models.AppDashboard.LinkCardViewModel;
using SearchFormViewModel = Linkubator.Web.Models.Presentation.SearchFormViewModel;
using EmptyStateViewModel = Linkubator.Web.Models.Presentation.EmptyStateViewModel;
using Presentation = Linkubator.Web.Models.Presentation;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App;

public class DashboardModel : PrivatePageModel
{
    private readonly ApplicationContracts.IDashboardService _dashboardService;
    private readonly ApplicationContracts.ILinkService _linkService;

    public DashboardModel(
        ApplicationContracts.IDashboardService dashboardService,
        ApplicationContracts.ILinkService linkService,
        ApplicationContracts.ICollectionService collectionService,
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile, collectionService)
    {
        _dashboardService = dashboardService;
        _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
    }

    [BindProperty]
    public string? QuickUrl { get; set; }

    [TempData]
    public string? LinkManagementError { get; set; }

    public Presentation.PageErrorSummaryViewModel QuickAddErrors { get; private set; } = new();

    public Presentation.PageAlertViewModel QuickAddAlert { get; private set; } = new();

    public IReadOnlyList<LinkCardViewModel> Cards { get; private set; } = [];

    public SearchFormViewModel SearchForm { get; private set; } = new();

    public EmptyStateViewModel EmptyState { get; private set; } = new();

    public async Task<IActionResult> OnPostQuickAdd(CancellationToken cancellationToken)
    {
        ApplicationResults.Result<IReadOnlyCollection<ApplicationDtos.CollectionDto>> collectionsResult = CollectionsResult
            ?? throw new InvalidOperationException("The private page must load collections before executing its handler.");
        if (!collectionsResult.IsSuccess)
        {
            QuickAddAlert = new Presentation.PageAlertViewModel
            {
                Message = "No se han podido cargar las colecciones. Vuelve a intentarlo.",
                Kind = "error",
            };
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return await OnGet(cancellationToken);
        }

        IReadOnlyCollection<ApplicationDtos.CollectionDto> collections = collectionsResult.Value
            ?? throw new InvalidOperationException("A successful collection result must contain its collection list.");
        ApplicationDtos.CollectionDto? collection = collections.FirstOrDefault(item => item.Slug == "bandeja-de-entrada")
            ?? collections.FirstOrDefault();
        if (collection is null)
        {
            QuickAddAlert = new Presentation.PageAlertViewModel
            {
                Message = "No tienes colecciones disponibles. Crea una colección para añadir enlaces.",
                Kind = "error",
            };
            return await OnGet(cancellationToken);
        }

        ApplicationResults.Result<ApplicationDtos.LinkDto> result = await _linkService.CreateLink(
            new ApplicationRequests.LinkCreateRequest(collection.Id, QuickUrl ?? string.Empty, null, null, null, false),
            cancellationToken);
        if (result.IsSuccess)
        {
            return Redirect(AppRoutes.Root);
        }

        QuickAddErrors = new Presentation.PageErrorSummaryViewModel
        {
            Items = result.Errors.Select(error => new Presentation.PageErrorSummaryItemViewModel
            {
                Message = !string.IsNullOrWhiteSpace(error.Message) && error.Message != error.Code
                    ? error.Message
                    : "No se ha podido añadir el enlace. Revisa la URL e inténtalo de nuevo.",
                FieldId = error.Field == "Url" ? "quick-url" : null,
            }).ToArray(),
        };
        return await OnGet(cancellationToken);
    }

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
        Cards = dashboard.RecentLinks.Select(Linkubator.Web.Models.AppDashboard.LinkCardViewModelFactory.Create).ToList();

        return Page();
    }
}
