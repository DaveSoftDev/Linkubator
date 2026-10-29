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

public sealed class DeleteModel : LinkManagementPageModel
{
    private const string LinkDeleteNotAccessibleMessage = "No se ha podido eliminar el enlace.";
    private const string LinkListRoute = AppRoutes.Links;
    private readonly ApplicationContracts.ILinkService _linkService;

    public DeleteModel(
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

    public string LinkLabel { get; private set; } = string.Empty;

    public Presentation.PageErrorSummaryViewModel ErrorSummary { get; private set; } = new();

    public Presentation.PageAlertViewModel Alert { get; private set; } = new();

    public Presentation.PrivateResourceUnavailableViewModel? ResourceUnavailable { get; private set; }

    [TempData]
    public string? LinkManagementError { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
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

        LinkLabel = string.IsNullOrWhiteSpace(result.Value.Title) ? result.Value.UrlOriginal : result.Value.Title;
        return Page();
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        if (!TryGetLinkId(out long linkId))
        {
            Alert = new Presentation.PageAlertViewModel { Message = ApplicationConstants.LinkErrors.LinkIdInvalid().Message ?? string.Empty, Kind = "error" };
            return Page();
        }

        ApplicationResults.Result result = await _linkService.DeleteLink(
            new ApplicationRequests.LinkDeleteRequest(linkId),
            cancellationToken);
        if (result.IsSuccess)
        {
            return LocalRedirect(ReturnRoute);
        }

        if (result.Errors.Any(error => error.Category == Linkubator.Application.Enums.ApplicationErrorCategory.ResourceNotAccessible))
        {
            LinkManagementError = LinkDeleteNotAccessibleMessage;
            return LocalRedirect(ReturnRoute);
        }

        ApplicationResults.Result<ApplicationDtos.LinkDto> linkResult = await _linkService.GetLink(
            new ApplicationRequests.LinkGetRequest(linkId),
            cancellationToken);
        if (linkResult.IsSuccess && linkResult.Value is not null)
        {
            LinkLabel = string.IsNullOrWhiteSpace(linkResult.Value.Title)
                ? linkResult.Value.UrlOriginal
                : linkResult.Value.Title;
        }

        ApplyErrors(result.Errors);
        return Page();
    }

    private bool TryGetLinkId(out long linkId)
    {
        linkId = 0;
        return !string.IsNullOrWhiteSpace(RouteId)
            && RouteId.All(char.IsAsciiDigit)
            && long.TryParse(RouteId, NumberStyles.None, CultureInfo.InvariantCulture, out linkId)
            && linkId > 0;
    }

    private void ApplyErrors(IReadOnlyCollection<ApplicationResults.ApplicationError> errors)
    {
        List<Presentation.PageErrorSummaryItemViewModel> items = [];
        foreach (ApplicationResults.ApplicationError error in errors)
        {
            if (error.Category == Linkubator.Application.Enums.ApplicationErrorCategory.ResourceNotAccessible)
            {
                ResourceUnavailable = Presentation.PrivateResourceUnavailablePresenter.Create(error, LinkListRoute);
            }
            else
            {
                items.Add(new Presentation.PageErrorSummaryItemViewModel { Message = error.Message ?? error.Code });
            }
        }

        ErrorSummary = new Presentation.PageErrorSummaryViewModel { Items = items };
    }
}
