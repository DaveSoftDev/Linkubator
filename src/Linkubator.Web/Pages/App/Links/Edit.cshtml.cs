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

public sealed class EditModel : LinkManagementPageModel
{
    private const string LinkListRoute = AppRoutes.Links;
    private readonly ApplicationContracts.ILinkService _linkService;
    private long? _currentCollectionId;

    public EditModel(
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
    public LinkEditInput Input { get; set; } = new();

    public string LinkUrl { get; private set; } = string.Empty;

    public long? CurrentCollectionId => _currentCollectionId;

    public bool IsCollectionChanging => _currentCollectionId is long currentCollectionId
        && TryGetCollectionId(Input.CollectionId, out long selectedCollectionId)
        && selectedCollectionId != currentCollectionId;

    public bool CanMakePublic => !IsCollectionChanging
        && TryGetCollectionId(Input.CollectionId, out long collectionId)
        && Collections.Any(collection => collection.Id == collectionId && collection.IsPublic);

    public string CollectionPrivacyHint
    {
        get
        {
            if (IsCollectionChanging)
            {
                return "Al cambiar de colección, el enlace pasará a ser privado.";
            }

            if (TryGetCollectionId(Input.CollectionId, out long collectionId)
                && Collections.Any(collection => collection.Id == collectionId && !collection.IsPublic))
            {
                return "Este enlace pertenece a una colección privada y no se puede hacer público.";
            }

            return "Selecciona una colección pública para poder hacer público el enlace.";
        }
    }

    public IReadOnlyList<ApplicationDtos.CollectionDto> Collections { get; private set; } = [];

    public Presentation.PageErrorSummaryViewModel ErrorSummary { get; private set; } = new();

    public Presentation.PageAlertViewModel Alert { get; private set; } = new();

    public Presentation.PrivateResourceUnavailableViewModel? ResourceUnavailable { get; private set; }

    [TempData]
    public string? LinkManagementError { get; set; }

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

        LoadInputContext(currentResult.Value);
        string publicStateField = $"{nameof(Input)}.{nameof(LinkEditInput.IsPublic)}";
        if (ModelState.GetFieldValidationState(publicStateField) == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
        {
            ModelState.SetModelValue(publicStateField, false, bool.FalseString);
            ErrorSummary = new Presentation.PageErrorSummaryViewModel
            {
                Items = ModelState[publicStateField]!.Errors
                    .Select(error => new Presentation.PageErrorSummaryItemViewModel
                    {
                        Message = error.ErrorMessage,
                        FieldId = "link-public",
                    })
                    .ToArray(),
            };
            return Page();
        }

        if (!TryGetCollectionId(Input.CollectionId, out long collectionId))
        {
            Input.IsPublic = false;
            ModelState.SetModelValue("Input.IsPublic", false, bool.FalseString);
            string message = string.IsNullOrWhiteSpace(Input.CollectionId)
                ? ApplicationConstants.LinkErrors.CollectionIdInvalid().Message ?? string.Empty
                : ApplicationConstants.LinkErrors.CollectionSelectionInvalid().Message ?? string.Empty;
            AddCollectionError(message);
            return Page();
        }

        if (IsCollectionChanging)
        {
            Input.IsPublic = false;
            ModelState.SetModelValue("Input.IsPublic", false, bool.FalseString);
        }

        ApplicationResults.Result<ApplicationDtos.LinkDto> result = await _linkService.UpdateLink(
            new ApplicationRequests.LinkUpdateRequest(
                linkId,
                collectionId,
                Input.Title,
                Input.Description,
                Input.Image,
                Input.IsPublic),
            cancellationToken);
        if (result.IsSuccess)
        {
            return LocalRedirect(ReturnRoute);
        }

        if (result.Errors.Any(error => error.Code == ApplicationConstants.LinkErrors.LinkNotAccessible().Code))
        {
            return RedirectUnavailableLink();
        }

        ApplicationResults.ApplicationError? publicInPrivateCollection = result.Errors.FirstOrDefault(
            error => error.Code == ApplicationConstants.LinkErrors.PublicLinkInPrivateCollection().Code);
        if (publicInPrivateCollection is not null)
        {
            LinkManagementError = publicInPrivateCollection.Message;
            return LocalRedirect(ReturnRoute);
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
        LoadInputContext(link);
        Input.CollectionId = link.CollectionId.ToString(CultureInfo.InvariantCulture);
        Input.Title = link.Title;
        Input.Description = link.Description;
        Input.Image = link.Image;
        Input.IsPublic = link.IsPublic;
    }

    private void LoadInputContext(ApplicationDtos.LinkDto link)
    {
        _currentCollectionId = link.CollectionId;
        LinkUrl = link.UrlOriginal;
    }

    private static bool TryGetCollectionId(string? value, out long collectionId)
    {
        collectionId = 0;
        return !string.IsNullOrWhiteSpace(value)
            && value.All(char.IsAsciiDigit)
            && long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out collectionId)
            && collectionId > 0;
    }

    private void AddCollectionError(string message)
    {
        ModelState.AddModelError("Input.CollectionId", message);
        ErrorSummary = new Presentation.PageErrorSummaryViewModel
        {
            Items = [new Presentation.PageErrorSummaryItemViewModel { Message = message, FieldId = "link-collection" }],
        };
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
            else if (GetFieldId(error.Field) is string fieldId)
            {
                string message = Presentation.LinkValidationErrorMessagePresenter.ResolveFieldMessage(
                    error,
                    error.Message ?? error.Code);
                string modelStateKey = $"Input.{error.Field}";
                ModelState.AddModelError(modelStateKey, message);
                items.Add(new Presentation.PageErrorSummaryItemViewModel { Message = message, FieldId = fieldId });
            }
            else
            {
                Alert = new Presentation.PageAlertViewModel { Message = error.Message ?? error.Code, Kind = "error" };
            }
        }

        ErrorSummary = new Presentation.PageErrorSummaryViewModel { Items = items };
    }

    private static string? GetFieldId(string? field)
        => field switch
        {
            nameof(LinkEditInput.Title) => "link-title",
            nameof(LinkEditInput.Description) => "link-description",
            nameof(LinkEditInput.Image) => "link-image",
            nameof(LinkEditInput.CollectionId) => "link-collection",
            nameof(LinkEditInput.IsPublic) => "link-public",
            _ => null,
        };
}
