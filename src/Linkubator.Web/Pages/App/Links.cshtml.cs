using System.Globalization;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationResults = Linkubator.Application.Results;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using AppSidebar = Linkubator.Web.Models.AppSidebar;
using Cards = Linkubator.Web.Models.AppDashboard;
using Presentation = Linkubator.Web.Models.Presentation;
using PrivateListing = Linkubator.Web.Models.PrivateListing;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App;

public sealed class LinksModel : PrivatePageModel
{
    private readonly ApplicationContracts.ILinkService _linkService;

    public LinksModel(
        ApplicationContracts.ILinkService linkService,
        ApplicationContracts.ICollectionService collectionService,
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile, collectionService)
    {
        _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
    }

    protected override AppSidebar.AppSidebarSelectionKind SidebarSelectionKind => AppSidebar.AppSidebarSelectionKind.Links;

    public string Heading { get; private set; } = "Mis enlaces";

    public IReadOnlyList<Cards.LinkCardViewModel> LinkCards { get; private set; } = [];

    public Presentation.SearchFormViewModel SearchForm { get; private set; } = new();

    public Presentation.PaginationViewModel Pagination { get; private set; } = new();

    public Presentation.PageAlertViewModel Alert { get; private set; } = new();

    public bool HasListingError { get; private set; }

    public bool HasInvalidQuery { get; private set; }

    public bool HasInvalidCollectionFilter { get; private set; }

    [TempData]
    public string? LinkManagementError { get; set; }

    public IActionResult OnPost() => NotFound();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(LinkManagementError))
        {
            Alert = new Presentation.PageAlertViewModel { Message = LinkManagementError, Kind = "error" };
        }

        ApplicationResults.Result<IReadOnlyCollection<ApplicationDtos.CollectionDto>> collectionsResult = CollectionsResult
            ?? throw new InvalidOperationException("The private page must load collections before executing its handler.");
        if (!collectionsResult.IsSuccess)
        {
            const string collectionsError = "No se han podido cargar las colecciones. Vuelve a intentarlo.";
            Alert = Alert.AppendError(collectionsError);
            HasListingError = true;
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return Page();
        }

        IReadOnlyCollection<ApplicationDtos.CollectionDto> collections = collectionsResult.Value
            ?? throw new InvalidOperationException("A successful collection result must contain its collection list.");
        PrivateListing.PrivateListingQueryAnalysisResult analysis = PrivateListing.PrivateListingQueryAnalyzer.Analyze(
            Request.Query, PrivateListing.PrivateListingKind.Links, id => collections.Any(collection => collection.Id == id));
        ApplicationDtos.CollectionDto? selected = collections.FirstOrDefault(collection => collection.Id == analysis.CollectionId);
        Heading = selected?.Name ?? Heading;
        Sidebar = AppSidebar.AppSidebarViewModelFactory.CreateSidebar(
            selected is null ? SidebarSelectionKind : AppSidebar.AppSidebarSelectionKind.Collection,
            collections.ToArray(), SidebarUser, selected?.Id);
        SearchForm = new Presentation.SearchFormViewModel
        {
            Action = $"{Request.PathBase}{AppRoutes.Links}",
            Label = "Buscar enlaces",
            Placeholder = "Buscar enlaces...",
            QueryInputId = "links-search-query",
            QueryValue = analysis.HasSearchError ? Request.Query["q"].FirstOrDefault() ?? string.Empty : analysis.SearchText ?? string.Empty,
            HiddenFields = selected is null ? [] : [new("c", selected.Id.ToString(CultureInfo.InvariantCulture))],
            ErrorSummary = new Presentation.PageErrorSummaryViewModel
            {
                Items = analysis.SearchErrorMessage is null
                    ? []
                    : [new Presentation.PageErrorSummaryItemViewModel { Message = analysis.SearchErrorMessage, FieldId = "links-search-query" }],
            },
        };

        if (analysis.HasAnyError)
        {
            if (analysis.CollectionErrorMessage is not null)
            {
                Alert = Alert.AppendError(analysis.CollectionErrorMessage);
            }
            HasListingError = true;
            HasInvalidQuery = true;
            HasInvalidCollectionFilter = analysis.HasCollectionError;
            return Page();
        }

        ApplicationResults.Result<ApplicationDtos.LinkPageDto> result = await _linkService.ListLinksPage(
            analysis.SearchText, analysis.CollectionId, analysis.PageNumber, cancellationToken);
        if (!result.IsSuccess)
        {
            Alert = Alert.AppendError(string.Join(" ", result.Errors.Select(error => error.Message ?? error.Code)));
            HasListingError = true;
            return Page();
        }

        ApplicationDtos.LinkPageDto linkPage = result.Value
            ?? throw new InvalidOperationException("A successful link page result must contain its page.");
        Dictionary<long, string> collectionNames = collections.ToDictionary(collection => collection.Id, collection => collection.Name);
        LinkCards = linkPage.Items.Select(link => Cards.LinkCardViewModelFactory.Create(
            link with { CollectionName = collectionNames.GetValueOrDefault(link.CollectionId) })).ToArray();
        Pagination = Presentation.PaginationViewModelFactory.Create(
            linkPage.Page, linkPage.PageCount, page => BuildPageUrl(page, analysis.SearchText, analysis.CollectionId));
        return Page();
    }

    private string BuildPageUrl(int? page, string? query, long? collectionId)
    {
        List<string> parameters = [];
        if (collectionId.HasValue)
        {
            parameters.Add($"c={collectionId.Value.ToString(CultureInfo.InvariantCulture)}");
        }
        if (!string.IsNullOrEmpty(query))
        {
            parameters.Add($"q={Uri.EscapeDataString(query)}");
        }
        if (page.HasValue)
        {
            parameters.Add($"p={page.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        string route = $"{Request.PathBase}{AppRoutes.Links}";
        return parameters.Count == 0 ? route : $"{route}?{string.Join("&", parameters)}";
    }
}
