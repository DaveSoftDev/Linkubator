using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationResults = Linkubator.Application.Results;
using CollectionDto = Linkubator.Application.Dtos.Models.CollectionDto;
using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using AppSidebarUserViewModel = Linkubator.Web.Models.AppSidebar.AppSidebarUserViewModel;
using AppSidebarViewModel = Linkubator.Web.Models.AppSidebar.AppSidebarViewModel;
using AppSidebarViewModelFactory = Linkubator.Web.Models.AppSidebar.AppSidebarViewModelFactory;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Linkubator.Web.Pages.App;

public abstract class PrivatePageModel : PageModel
{
    private readonly ApplicationContracts.ICollectionService _collectionService;

    protected PrivatePageModel(WebDevelopmentIdentity.IWebUserProfile userProfile, ApplicationContracts.ICollectionService collectionService)
    {
        UserProfile = userProfile ?? throw new ArgumentNullException(nameof(userProfile));
        _collectionService = collectionService ?? throw new ArgumentNullException(nameof(collectionService));
        Sidebar = CreateSidebar(SidebarSelectionKind);
    }

    protected virtual AppSidebarSelectionKind SidebarSelectionKind => AppSidebarSelectionKind.Home;

    protected WebDevelopmentIdentity.IWebUserProfile UserProfile { get; }

    public AppSidebarViewModel Sidebar { get; protected set; }

    protected AppSidebarUserViewModel SidebarUser => new()
    {
        Name = UserProfile.Name,
        Alias = UserProfile.Alias,
        Email = UserProfile.Email,
    };

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await LoadSidebarCollections(SidebarSelectionKind, context.HttpContext.RequestAborted);
        await next();
    }

    protected async Task LoadSidebarCollections(AppSidebarSelectionKind selectionKind = AppSidebarSelectionKind.Home, CancellationToken cancellationToken = default)
    {
        ApplicationResults.Result<IReadOnlyCollection<CollectionDto>> result = await _collectionService.ListAllCollections(cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            Sidebar = CreateSidebar(selectionKind);
            return;
        }

        Sidebar = CreateSidebar(selectionKind, result.Value);
    }

    protected AppSidebarViewModel CreateSidebar(
        AppSidebarSelectionKind selectionKind,
        IEnumerable<CollectionDto>? collections = null)
    {
        IReadOnlyList<CollectionDto> collectionItems = collections as IReadOnlyList<CollectionDto>
            ?? collections?.ToArray()
            ?? Array.Empty<CollectionDto>();

        Sidebar = AppSidebarViewModelFactory.CreateSidebar(selectionKind, collectionItems, SidebarUser);
        return Sidebar;
    }
}
