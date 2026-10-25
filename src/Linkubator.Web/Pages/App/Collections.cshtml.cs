using ApplicationConstants = Linkubator.Application.Constants;
using ApplicationEnums = Linkubator.Application.Enums;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationResults = Linkubator.Application.Results;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using CollectionPageDto = Linkubator.Application.Dtos.Models.CollectionPageDto;
using EmptyStateViewModel = Linkubator.Web.Models.Presentation.EmptyStateViewModel;
using PageAlertViewModel = Linkubator.Web.Models.Presentation.PageAlertViewModel;
using PageErrorSummaryItemViewModel = Linkubator.Web.Models.Presentation.PageErrorSummaryItemViewModel;
using PageErrorSummaryViewModel = Linkubator.Web.Models.Presentation.PageErrorSummaryViewModel;
using PaginationPageLinkViewModel = Linkubator.Web.Models.Presentation.PaginationPageLinkViewModel;
using PaginationViewModel = Linkubator.Web.Models.Presentation.PaginationViewModel;
using PrivateListingKind = Linkubator.Web.Models.PrivateListing.PrivateListingKind;
using PrivateListingQueryAnalyzer = Linkubator.Web.Models.PrivateListing.PrivateListingQueryAnalyzer;
using SearchFormViewModel = Linkubator.Web.Models.Presentation.SearchFormViewModel;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App;

public sealed class CollectionsModel : PrivatePageModel
{
    private readonly ApplicationContracts.ICollectionService _collectionService;

    public CollectionsModel(
        ApplicationContracts.ICollectionService collectionService,
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile, collectionService)
    {
        _collectionService = collectionService ?? throw new ArgumentNullException(nameof(collectionService));
    }

    protected override AppSidebarSelectionKind SidebarSelectionKind => AppSidebarSelectionKind.Collections;

    public IReadOnlyCollection<ApplicationDtos.CollectionDto> Collections { get; private set; } = [];

    public SearchFormViewModel SearchForm { get; private set; } = new();

    public PaginationViewModel Pagination { get; private set; } = new();

    public EmptyStateViewModel EmptyState { get; private set; } = new();

    public EmptyStateViewModel ListingError { get; private set; } = new();

    public PageAlertViewModel Alert { get; private set; } = new();

    public PageErrorSummaryViewModel ErrorSummary { get; private set; } = new();

    public bool HasListingError { get; private set; }

    [TempData]
    public string? CollectionDeleteError { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(CollectionDeleteError))
        {
            Alert = new PageAlertViewModel { Message = CollectionDeleteError, Kind = "error" };
        }

        Models.PrivateListing.PrivateListingQueryAnalysisResult analysis = PrivateListingQueryAnalyzer.Analyze(Request.Query, PrivateListingKind.Collections);
        SearchForm = new SearchFormViewModel
        {
            Action = AppRoutes.Collections,
            Label = "Buscar colecciones",
            Placeholder = "Buscar colecciones...",
            ButtonText = "Buscar",
            QueryInputId = "collections-search-query",
            QueryValue = analysis.SearchText ?? string.Empty,
        };
        EmptyState = new EmptyStateViewModel
        {
            Title = "No se han encontrado colecciones.",
            Description = "Prueba con otra búsqueda o crea una colección.",
        };
        ListingError = new EmptyStateViewModel
        {
            Title = "No se ha podido cargar el listado de colecciones.",
            Description = "Prueba de nuevo con otra búsqueda o crea una colección.",
        };

        if (analysis.HasSearchError)
        {
            List<PageErrorSummaryItemViewModel> errors = [];
            if (analysis.SearchErrorMessage is not null)
            {
                errors.Add(new PageErrorSummaryItemViewModel { Message = analysis.SearchErrorMessage });
            }
            ErrorSummary = new PageErrorSummaryViewModel { Items = errors };
            HasListingError = true;
            return Page();
        }

        ApplicationResults.Result<CollectionPageDto> result = await _collectionService.ListCollectionsPage(
            analysis.SearchText,
            analysis.HasPageError ? 1 : analysis.PageNumber,
            cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            bool hasValidationError = result.Errors.Any(error => error.Category == ApplicationEnums.ApplicationErrorCategory.Validation);
            string message = hasValidationError
                ? result.Errors.First(error => error.Category == ApplicationEnums.ApplicationErrorCategory.Validation).Message ?? ApplicationConstants.CommonErrors.PageInvalidMessage
                : "No se ha podido cargar el listado de colecciones.";
            Alert = new PageAlertViewModel { Message = message };
            HasListingError = true;
            return Page();
        }

        CollectionPageDto collectionPage = result.Value;
        Collections = collectionPage.Items;
        Pagination = new PaginationViewModel
        {
            Pages = BuildPageLinks(collectionPage.Page, collectionPage.PageCount, analysis.SearchText),
        };

        return Page();
    }

    private static IReadOnlyList<PaginationPageLinkViewModel> BuildPageLinks(int currentPage, int pageCount, string? query)
    {
        if (pageCount <= 1)
        {
            return [];
        }

        List<PaginationPageLinkViewModel> pages = [];
        if (currentPage > 1)
        {
            pages.Add(new PaginationPageLinkViewModel
            {
                Label = "Anterior",
                Url = BuildPageUrl(currentPage - 1, query),
            });
        }

        for (int page = 1; page <= pageCount; page++)
        {
            pages.Add(new PaginationPageLinkViewModel
            {
                Label = page.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Url = page == currentPage ? null : page == 1 ? BuildPageUrl(null, query) : BuildPageUrl(page, query),
                IsCurrent = page == currentPage,
            });
        }

        if (currentPage < pageCount)
        {
            pages.Add(new PaginationPageLinkViewModel
            {
                Label = "Siguiente",
                Url = BuildPageUrl(currentPage + 1, query),
            });
        }

        return pages;
    }

    private static string BuildPageUrl(int? page, string? query)
    {
        List<string> parameters = [];
        if (!string.IsNullOrEmpty(query))
        {
            parameters.Add($"q={Uri.EscapeDataString(query)}");
        }
        if (page.HasValue)
        {
            parameters.Add($"p={page.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }
        return parameters.Count == 0 ? AppRoutes.Collections : $"{AppRoutes.Collections}?{string.Join("&", parameters)}";
    }
}
