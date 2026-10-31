using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationEnums = Linkubator.Application.Enums;
using ApplicationModelDtos = Linkubator.Application.Dtos.Models;
using ApplicationRequestDtos = Linkubator.Application.Dtos.Requests;
using ApplicationResults = Linkubator.Application.Results;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using PageAlertViewModel = Linkubator.Web.Models.Presentation.PageAlertViewModel;
using PageErrorSummaryItemViewModel = Linkubator.Web.Models.Presentation.PageErrorSummaryItemViewModel;
using PageErrorSummaryViewModel = Linkubator.Web.Models.Presentation.PageErrorSummaryViewModel;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App.Collections;

public sealed class NewModel : PrivatePageModel
{
    private const string NameField = "Name";
    private const string DescriptionField = "Description";
    private const string NameInputId = "collection-name";
    private const string DescriptionInputId = "collection-description";
    private const string CollectionListRoute = AppRoutes.Collections;

    private readonly ApplicationContracts.ICollectionService _collectionService;

    public NewModel(
        ApplicationContracts.ICollectionService collectionService,
        WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile, collectionService)
    {
        _collectionService = collectionService ?? throw new ArgumentNullException(nameof(collectionService));
    }

    protected override AppSidebarSelectionKind SidebarSelectionKind => AppSidebarSelectionKind.Collections;

    [BindProperty]
    public CollectionInput Input { get; set; } = new();

    public PageErrorSummaryViewModel ErrorSummary { get; private set; } = new();

    public PageAlertViewModel Alert { get; private set; } = new();

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            string privacyField = $"{nameof(Input)}.{nameof(CollectionInput.IsPublic)}";
            if (ModelState.GetFieldValidationState(privacyField) == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
            {
                ModelState[privacyField]!.Errors.Clear();
                ModelState.AddModelError(privacyField, Linkubator.Web.Constants.CollectionErrorMessages.InvalidPrivacy);
                ModelState.SetModelValue(privacyField, false, bool.FalseString);
            }

            ErrorSummary = new PageErrorSummaryViewModel
            {
                Items = ModelState.SelectMany(entry => entry.Value!.Errors.Select(error => new PageErrorSummaryItemViewModel
                {
                    Message = error.ErrorMessage,
                    FieldId = GetFieldId(entry.Key.StartsWith("Input.", StringComparison.Ordinal) ? entry.Key["Input.".Length..] : entry.Key),
                })).ToArray(),
            };
            return Page();
        }

        ApplicationRequestDtos.CollectionCreateRequest request = new(Input.Name ?? string.Empty, Input.Description, Input.IsPublic);
        ApplicationResults.Result<ApplicationModelDtos.CollectionDto> result = await _collectionService.CreateCollection(request, cancellationToken);
        if (!result.IsSuccess)
        {
            ApplyErrors(result.Errors);
            return Page();
        }

        return Redirect(CollectionListRoute);
    }

    private void ApplyErrors(IReadOnlyCollection<ApplicationResults.ApplicationError> errors)
    {
        List<PageErrorSummaryItemViewModel> items = [];
        foreach (ApplicationResults.ApplicationError error in errors)
        {
            if (error.Category == ApplicationEnums.ApplicationErrorCategory.Validation && !string.IsNullOrWhiteSpace(error.Field))
            {
                AddModelError(error.Field, error.Message ?? string.Empty);
                items.Add(new PageErrorSummaryItemViewModel
                {
                    Message = error.Message ?? string.Empty,
                    FieldId = GetFieldId(error.Field),
                });
            }
            else if (error.Category == ApplicationEnums.ApplicationErrorCategory.DomainRule && !string.IsNullOrWhiteSpace(error.Field))
            {
                AddModelError(error.Field, error.Message ?? string.Empty);
                items.Add(new PageErrorSummaryItemViewModel
                {
                    Message = error.Message ?? string.Empty,
                    FieldId = error.Field == NameField ? NameInputId : null,
                });
            }
            else
            {
                Alert = new PageAlertViewModel { Message = error.Message ?? string.Empty, Kind = "error" };
            }
        }

        ErrorSummary = new PageErrorSummaryViewModel { Items = items };
    }

    private void AddModelError(string field, string message)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return;
        }

        string modelStateKey = field switch
        {
            NameField => "Input.Name",
            DescriptionField => "Input.Description",
            _ => string.Empty,
        };

        if (!string.IsNullOrWhiteSpace(modelStateKey))
        {
            ModelState.AddModelError(modelStateKey, message);
        }
    }

    private static string? GetFieldId(string field)
        => field switch
        {
            NameField => NameInputId,
            DescriptionField => DescriptionInputId,
            nameof(CollectionInput.IsPublic) => "collection-public",
            _ => null,
        };
}
