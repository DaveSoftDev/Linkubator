using System.Globalization;
using ApplicationConstants = Linkubator.Application.Constants;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationRequests = Linkubator.Application.Dtos.Requests;
using ApplicationResults = Linkubator.Application.Results;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using Presentation = Linkubator.Web.Models.Presentation;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App.Links;

public sealed class MoveModel : LinkManagementPageModel
{
    private const string LinkListRoute = AppRoutes.Links;
    private readonly ApplicationContracts.ILinkService _linkService;

    public MoveModel(
        ApplicationContracts.ILinkService linkService,
        ApplicationContracts.ICollectionService collectionService,
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile, collectionService)
    {
        _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
    }

    protected override AppSidebarSelectionKind SidebarSelectionKind => AppSidebarSelectionKind.Links;

    [BindProperty(SupportsGet = true, Name = "id")]
    public string? RouteId { get; set; }

    [BindProperty]
    public LinkMoveInput Input { get; set; } = new();

    public IReadOnlyList<ApplicationDtos.CollectionDto> Collections { get; private set; } = [];

    public string LinkLabel { get; private set; } = string.Empty;

    public bool HasLoadedLink { get; private set; }

    public Presentation.PageErrorSummaryViewModel ErrorSummary { get; private set; } = new();

    public Presentation.PageAlertViewModel Alert { get; private set; } = new();

    public Presentation.PrivateResourceUnavailableViewModel? ResourceUnavailable { get; private set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        if (!LoadCollections())
        {
            return Page();
        }

        if (!TryGetLinkId(out long linkId))
        {
            Alert = new Presentation.PageAlertViewModel { Message = ApplicationConstants.LinkErrors.LinkIdInvalid().Message ?? string.Empty, Kind = "error" };
            return Page();
        }

        ApplicationResults.Result<ApplicationDtos.LinkDto> result = await _linkService.GetLink(
            new ApplicationRequests.LinkGetRequest(linkId),
            cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            ApplyErrors(result.Errors);
            return Page();
        }

        LoadInput(result.Value);
        return Page();
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        if (!LoadCollections())
        {
            return Page();
        }

        if (!TryGetLinkId(out long linkId))
        {
            Alert = new Presentation.PageAlertViewModel { Message = ApplicationConstants.LinkErrors.LinkIdInvalid().Message ?? string.Empty, Kind = "error" };
            return Page();
        }

        ApplicationResults.Result<ApplicationDtos.LinkDto> currentResult = await _linkService.GetLink(
            new ApplicationRequests.LinkGetRequest(linkId),
            cancellationToken);
        if (!currentResult.IsSuccess || currentResult.Value is null)
        {
            if (currentResult.Errors.Any(error => error.Category == Linkubator.Application.Enums.ApplicationErrorCategory.ResourceNotAccessible))
            {
                return RedirectUnavailableLink();
            }

            ApplyErrors(currentResult.Errors);
            return Page();
        }

        SetLinkLabel(currentResult.Value);
        if (!TryGetCollectionId(Input.DestinationCollectionId, out long destinationCollectionId))
        {
            AddDestinationError(ApplicationConstants.LinkErrors.DestinationCollectionIdInvalid().Message ?? string.Empty);
            return Page();
        }

        ApplicationResults.Result<ApplicationDtos.LinkDto> result = await _linkService.MoveLink(
            new ApplicationRequests.LinkMoveRequest(linkId, destinationCollectionId),
            cancellationToken);
        if (result.IsSuccess)
        {
            return LocalRedirect(ReturnRoute);
        }

        if (result.Errors.Any(error => error.Code == ApplicationConstants.LinkErrors.LinkNotAccessible().Code))
        {
            return RedirectUnavailableLink();
        }

        if (result.Errors.Any(error =>
            error.Code == ApplicationConstants.LinkErrors.DestinationCollectionIdInvalid().Code
            || error.Code == ApplicationConstants.LinkErrors.CollectionNotAccessible().Code))
        {
            await LoadSidebarCollections(SidebarSelectionKind, cancellationToken);
            if (!LoadCollections())
            {
                HasLoadedLink = false;
                return Page();
            }
        }

        ApplyErrors(result.Errors);
        return Page();
    }

    private bool LoadCollections()
    {
        ApplicationResults.Result<IReadOnlyCollection<ApplicationDtos.CollectionDto>> result = CollectionsResult
            ?? throw new InvalidOperationException("The private page must load collections before executing its handler.");
        if (!result.IsSuccess || result.Value is null)
        {
            Alert = new Presentation.PageAlertViewModel
            {
                Message = "No se han podido cargar las colecciones. Vuelve a intentarlo.",
                Kind = "error",
            };
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return false;
        }

        Collections = result.Value.ToArray();
        return true;
    }

    private void LoadInput(ApplicationDtos.LinkDto link)
    {
        Input.DestinationCollectionId = link.CollectionId.ToString(CultureInfo.InvariantCulture);
        SetLinkLabel(link);
    }

    private void SetLinkLabel(ApplicationDtos.LinkDto link)
    {
        LinkLabel = string.IsNullOrWhiteSpace(link.Title) ? link.UrlOriginal : link.Title;
        HasLoadedLink = true;
    }

    private bool TryGetLinkId(out long linkId)
    {
        linkId = 0;
        string? canonicalId = RouteData.Values["id"]?.ToString();
        return !string.IsNullOrWhiteSpace(canonicalId)
            && canonicalId.All(char.IsAsciiDigit)
            && long.TryParse(canonicalId, NumberStyles.None, CultureInfo.InvariantCulture, out linkId)
            && linkId > 0;
    }

    private static bool TryGetCollectionId(string? value, out long collectionId)
    {
        collectionId = 0;
        return !string.IsNullOrWhiteSpace(value)
            && value.All(char.IsAsciiDigit)
            && long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out collectionId)
            && collectionId > 0;
    }

    private void AddDestinationError(string message)
    {
        ModelState.AddModelError("Input.DestinationCollectionId", message);
        ErrorSummary = new Presentation.PageErrorSummaryViewModel
        {
            Items = [new Presentation.PageErrorSummaryItemViewModel { Message = message, FieldId = "link-collection" }],
        };
    }

    private void ApplyErrors(IReadOnlyCollection<ApplicationResults.ApplicationError> errors)
    {
        List<Presentation.PageErrorSummaryItemViewModel> items = [];
        foreach (ApplicationResults.ApplicationError error in errors)
        {
            if (error.Code == ApplicationConstants.LinkErrors.CollectionNotAccessible().Code)
            {
                string message = ApplicationConstants.LinkErrors.DestinationCollectionIdInvalid().Message ?? string.Empty;
                ModelState.AddModelError("Input.DestinationCollectionId", message);
                items.Add(new Presentation.PageErrorSummaryItemViewModel { Message = message, FieldId = "link-collection" });
            }
            else if (error.Category == Linkubator.Application.Enums.ApplicationErrorCategory.ResourceNotAccessible)
            {
                ResourceUnavailable = Presentation.PrivateResourceUnavailablePresenter.Create(error, LinkListRoute);
            }
            else if (error.Field == nameof(LinkMoveInput.DestinationCollectionId))
            {
                string message = error.Message ?? error.Code;
                ModelState.AddModelError("Input.DestinationCollectionId", message);
                items.Add(new Presentation.PageErrorSummaryItemViewModel { Message = message, FieldId = "link-collection" });
            }
            else
            {
                Alert = new Presentation.PageAlertViewModel { Message = error.Message ?? error.Code, Kind = "error" };
            }
        }

        ErrorSummary = new Presentation.PageErrorSummaryViewModel { Items = items };
    }
}
