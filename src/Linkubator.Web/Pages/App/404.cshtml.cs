using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;

namespace Linkubator.Web.Pages.App;

public sealed class AppNotFoundModel : PrivatePageModel
{
    public AppNotFoundModel(WebDevelopmentIdentity.IWebUserProfile userProfile)
        : base(userProfile)
    {
    }

    public void OnGet()
    {
        Sidebar = CreateDefaultSidebar();
    }
}
