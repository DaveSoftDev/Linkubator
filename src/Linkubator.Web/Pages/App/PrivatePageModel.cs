using AppSidebarSelectionKind = Linkubator.Web.Models.AppSidebar.AppSidebarSelectionKind;
using AppSidebarUserViewModel = Linkubator.Web.Models.AppSidebar.AppSidebarUserViewModel;
using AppSidebarViewModel = Linkubator.Web.Models.AppSidebar.AppSidebarViewModel;
using AppSidebarViewModelFactory = Linkubator.Web.Models.AppSidebar.AppSidebarViewModelFactory;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Linkubator.Web.Pages.App;

public abstract class PrivatePageModel : PageModel
{
    protected PrivatePageModel(WebDevelopmentIdentity.IWebUserProfile userProfile)
    {
        UserProfile = userProfile ?? throw new ArgumentNullException(nameof(userProfile));
        Sidebar = AppSidebarViewModelFactory.CreateDefault(SidebarUser);
    }

    protected WebDevelopmentIdentity.IWebUserProfile UserProfile { get; }

    public AppSidebarViewModel Sidebar { get; protected set; }

    protected AppSidebarUserViewModel SidebarUser => new()
    {
        Name = UserProfile.Name,
        Alias = UserProfile.Alias,
    };

    protected AppSidebarViewModel CreateDefaultSidebar()
    {
        Sidebar = AppSidebarViewModelFactory.CreateDefault(SidebarUser);
        return Sidebar;
    }

    protected AppSidebarViewModel CreateSidebar(
        AppSidebarSelectionKind selectionKind,
        IEnumerable<Linkubator.Application.Dtos.Models.CollectionDto>? collections = null)
    {
        IReadOnlyList<Linkubator.Application.Dtos.Models.CollectionDto> collectionItems = collections?.ToArray() ?? [];
        Sidebar = AppSidebarViewModelFactory.CreateSidebar(selectionKind, collectionItems.ToArray(), SidebarUser);
        return Sidebar;
    }
}
