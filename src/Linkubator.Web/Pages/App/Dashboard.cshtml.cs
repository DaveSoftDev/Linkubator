using Linkubator.Web.Models.AppDashboard;
using Linkubator.Web.Models.AppSidebar;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Linkubator.Web.Pages.App;

public class DashboardModel : PageModel
{
    public AppSidebarViewModel Sidebar { get; private set; } = AppSidebarViewModelFactory.CreateDefault();

    public IReadOnlyList<LinkCardViewModel> Cards { get; private set; } = LinkCardViewModelFactory.CreateDefault();

    public void OnGet()
    {
    }
}
