using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationEnums = Linkubator.Application.Enums;
using ApplicationModelDtos = Linkubator.Application.Dtos.Models;
using ApplicationRequestDtos = Linkubator.Application.Dtos.Requests;
using ApplicationResults = Linkubator.Application.Results;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using PrivateResourceUnavailablePresenter = Linkubator.Web.Models.Presentation.PrivateResourceUnavailablePresenter;
using PrivateResourceUnavailableViewModel = Linkubator.Web.Models.Presentation.PrivateResourceUnavailableViewModel;
using PageAlertViewModel = Linkubator.Web.Models.Presentation.PageAlertViewModel;
using PageErrorSummaryItemViewModel = Linkubator.Web.Models.Presentation.PageErrorSummaryItemViewModel;
using PageErrorSummaryViewModel = Linkubator.Web.Models.Presentation.PageErrorSummaryViewModel;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App.Collections;

public sealed class EditModel : PrivatePageModel
{
    private const string NameField = "Name";
    private const string DescriptionField = "Description";
    private const string NameInputId = "collection-name";
    private const string DescriptionInputId = "collection-description";
    private const string CollectionListRoute = AppRoutes.Collections;

    private readonly ApplicationContracts.ICollectionService _collectionService;

    public EditModel(
        ApplicationContracts.ICollectionService collectionService,
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile, collectionService)
    {
        _collectionService = collectionService ?? throw new ArgumentNullException(nameof(collectionService));
    }

    protected override AppSidebarSelectionKind SidebarSelectionKind => AppSidebarSelectionKind.Collections;

    [BindProperty(SupportsGet = true, Name = "id")]
    public string? RouteId { get; set; }

    [BindProperty]
    public CollectionInput Input { get; set; } = new();

    public PageErrorSummaryViewModel ErrorSummary { get; private set; } = new();

    public PageAlertViewModel Alert { get; private set; } = new();

    public PrivateResourceUnavailableViewModel? ResourceUnavailable { get; private set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        if (!TryGetCollectionId(out long collectionId))
        {
            Alert = new PageAlertViewModel { Message = Linkubator.Application.Constants.CollectionErrors.CollectionIdInvalidMessage, Kind = "error" };
            return Page();
        }

        ApplicationResults.Result<ApplicationModelDtos.CollectionDto> result = await _collectionService.GetCollection(
            new ApplicationRequestDtos.CollectionGetRequest(collectionId),
            cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            ApplyErrors(result.Errors);
            return Page();
        }

        Input.Name = result.Value.Name;
        Input.Description = result.Value.Description;
        Input.IsPublic = result.Value.IsPublic;
        return Page();
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        if (!TryGetCollectionId(out long collectionId))
        {
            Alert = new PageAlertViewModel { Message = Linkubator.Application.Constants.CollectionErrors.CollectionIdInvalidMessage, Kind = "error" };
            return Page();
        }

        ApplicationResults.Result<ApplicationModelDtos.CollectionDto> result = await _collectionService.UpdateCollection(
            new ApplicationRequestDtos.CollectionUpdateRequest(collectionId, Input.Name, Input.Description, Input.IsPublic),
            cancellationToken);
        if (!result.IsSuccess)
        {
            ApplyErrors(result.Errors);
            return Page();
        }

        return Redirect(CollectionListRoute);
    }

    private bool TryGetCollectionId(out long collectionId)
    {
        collectionId = 0;

        if (!string.IsNullOrWhiteSpace(RouteId))
        {
            string value = RouteId.Trim();
            return value.Length > 0 && value.All(char.IsAsciiDigit) && long.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out collectionId) && collectionId > 0;
        }

        string?[] values = Request.Query["id"].ToArray();
        if (Request.Method == "POST")
        {
            Microsoft.Extensions.Primitives.StringValues formValues = Request.Form["id"];
            values = formValues.Count > 0 ? formValues.ToArray() : values;
        }

        if (values.Length != 1)
        {
            return false;
        }

        string? formValue = values[0];
        return !string.IsNullOrWhiteSpace(formValue)
            && formValue.All(char.IsAsciiDigit)
            && long.TryParse(formValue, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out collectionId)
            && collectionId > 0;
    }

    private void ApplyErrors(IReadOnlyCollection<ApplicationResults.ApplicationError> errors)
    {
        List<PageErrorSummaryItemViewModel> items = [];
        foreach (ApplicationResults.ApplicationError error in errors)
        {
            if (error.Category == ApplicationEnums.ApplicationErrorCategory.ResourceNotAccessible)
            {
                ResourceUnavailable = PrivateResourceUnavailablePresenter.Create(error, CollectionListRoute);
            }
            else if (!string.IsNullOrWhiteSpace(error.Field))
            {
                items.Add(new PageErrorSummaryItemViewModel
                {
                    Message = error.Message ?? string.Empty,
                    FieldId = error.Field == NameField ? NameInputId : error.Field == DescriptionField ? DescriptionInputId : null,
                });
            }
            else
            {
                Alert = new PageAlertViewModel { Message = error.Message ?? string.Empty, Kind = "error" };
            }
        }

        ErrorSummary = new PageErrorSummaryViewModel { Items = items };
    }
}
