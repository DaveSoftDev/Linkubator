using System.Globalization;
using ApplicationConstants = Linkubator.Application.Constants;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationDtos = Linkubator.Application.Dtos.Models;
using ApplicationRequests = Linkubator.Application.Dtos.Requests;
using ApplicationResults = Linkubator.Application.Results;
using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using Presentation = Linkubator.Web.Models.Presentation;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App.Links;

public sealed class NewModel : LinkManagementPageModel
{
    private readonly ApplicationContracts.ILinkService _linkService;

    public NewModel(
        ApplicationContracts.ILinkService linkService,
        ApplicationContracts.ICollectionService collectionService,
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile, collectionService)
    {
        _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
    }

    protected override AppSidebarSelectionKind SidebarSelectionKind => AppSidebarSelectionKind.Links;

    [BindProperty]
    public LinkInput Input { get; set; } = new();

    [TempData]
    public string? LinkCreationError { get; set; }

    [TempData]
    public string? CreatedLinkUrl { get; set; }

    public IReadOnlyList<ApplicationDtos.CollectionDto> Collections { get; private set; } = [];

    public bool CanCreate { get; private set; }

    public bool CanMakePublic => long.TryParse(Input.CollectionId, NumberStyles.None, CultureInfo.InvariantCulture, out long collectionId)
        && Collections.Any(collection => collection.Id == collectionId && collection.IsPublic);

    public Presentation.PageErrorSummaryViewModel ErrorSummary { get; private set; } = new();

    public Presentation.PageAlertViewModel Alert { get; private set; } = new();

    public IActionResult OnGet(string? url, string? collectionId)
    {
        Input.Url = url;
        LoadOptions();
        if (long.TryParse(collectionId, NumberStyles.None, CultureInfo.InvariantCulture, out long selectedId)
            && Collections.Any(collection => collection.Id == selectedId))
        {
            Input.CollectionId = selectedId.ToString(CultureInfo.InvariantCulture);
        }
        return Page();
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        LoadOptions();
        if (!CanCreate)
        {
            return Page();
        }

        if (!ModelState.IsValid)
        {
            UpdateErrorSummary();
            return Page();
        }

        string collectionValue = Input.CollectionId ?? string.Empty;
        long collectionId = 0;
        bool validCollectionId = collectionValue.Length > 0
            && collectionValue.All(char.IsAsciiDigit)
            && long.TryParse(collectionValue, NumberStyles.None, CultureInfo.InvariantCulture, out collectionId);
        if (!validCollectionId)
        {
            collectionId = 0;
        }

        ApplicationResults.Result<ApplicationDtos.LinkDto> result = await _linkService.CreateLink(
            new ApplicationRequests.LinkCreateRequest(collectionId, Input.Url ?? string.Empty, Input.Title, Input.Description, Input.Image, Input.IsPublic),
            cancellationToken);
        if (result.IsSuccess)
        {
            ApplicationDtos.LinkDto link = result.Value
                ?? throw new InvalidOperationException("A successful link creation result must contain the created link.");
            CreatedLinkUrl = link.UrlOriginal;
            return LocalRedirect(ReturnRoute);
        }

        foreach (ApplicationResults.ApplicationError error in result.Errors)
        {
            string message = error.Message ?? error.Code;
            if (error.Code == ApplicationConstants.LinkErrors.PublicLinkInPrivateCollection().Code)
            {
                LinkCreationError = message;
                return LocalRedirect(ReturnRoute);
            }

            if (GetFieldId(error.Field) is not null)
            {
                string fieldMessage = GetFieldMessage(error, message);
                fieldMessage = Presentation.LinkValidationErrorMessagePresenter.ResolveFieldMessage(error, fieldMessage);
                ModelState.AddModelError($"Input.{error.Field}", fieldMessage);
            }
            else
            {
                Alert = new Presentation.PageAlertViewModel { Message = message, Kind = "error" };
            }
        }

        UpdateErrorSummary();
        return Page();
    }

    protected override bool AllowCollectionsReturn => true;

    private void LoadOptions()
    {
        ApplicationResults.Result<IReadOnlyCollection<ApplicationDtos.CollectionDto>> result = CollectionsResult
            ?? throw new InvalidOperationException("The private page must load collections before executing its handler.");
        if (!result.IsSuccess)
        {
            Alert = new Presentation.PageAlertViewModel
            {
                Message = "No se han podido cargar las colecciones. Vuelve a intentarlo.",
                Kind = "error",
            };
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        IReadOnlyCollection<ApplicationDtos.CollectionDto> collections = result.Value
            ?? throw new InvalidOperationException("A successful collection result must contain its collection list.");
        Collections = collections.ToArray();
        CanCreate = true;
    }

    private void UpdateErrorSummary()
    {
        if (!CanMakePublic)
        {
            Input.IsPublic = false;
            ModelState.SetModelValue("Input.IsPublic", false, bool.FalseString);
        }

        ErrorSummary = new Presentation.PageErrorSummaryViewModel
        {
            Items = ModelState.SelectMany(entry => entry.Value!.Errors.Select(error => new Presentation.PageErrorSummaryItemViewModel
            {
                Message = error.ErrorMessage,
                FieldId = GetFieldId(entry.Key.StartsWith("Input.", StringComparison.Ordinal) ? entry.Key["Input.".Length..] : null),
            })).ToArray(),
        };
    }

    private static string GetFieldMessage(ApplicationResults.ApplicationError error, string message)
        => error.Message != error.Code ? message : error.Field switch
        {
            nameof(LinkInput.Url) => "La URL no es válida.",
            nameof(LinkInput.Image) => "La dirección de la imagen no es válida.",
            nameof(LinkInput.Title) => "El título supera la longitud máxima permitida.",
            nameof(LinkInput.Description) => "La descripción supera la longitud máxima permitida.",
            _ => message,
        };

    private static string? GetFieldId(string? field)
        => field switch
        {
            nameof(LinkInput.CollectionId) => "link-collection",
            nameof(LinkInput.Url) => "link-url",
            nameof(LinkInput.Title) => "link-title",
            nameof(LinkInput.Description) => "link-description",
            nameof(LinkInput.Image) => "link-image",
            nameof(LinkInput.IsPublic) => "link-public",
            _ => null,
        };
}
