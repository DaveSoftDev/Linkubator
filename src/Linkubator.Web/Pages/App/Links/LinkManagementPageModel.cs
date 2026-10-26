using ApplicationContracts = Linkubator.Application.Contracts;
using LinkReturnRoute = Linkubator.Web.Navigation.LinkReturnRoute;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App.Links;

public abstract class LinkManagementPageModel : PrivatePageModel
{
    protected LinkManagementPageModel(
        WebDevelopmentIdentity.IWebUserProfile userProfile,
        ApplicationContracts.ICollectionService collectionService)
        : base(userProfile, collectionService)
    {
    }

    [BindProperty(Name = "ru", SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string ReturnRoute => LinkReturnRoute.Resolve(Url, Request.PathBase, ReturnUrl, AllowCollectionsReturn);

    protected virtual bool AllowCollectionsReturn => false;
}
