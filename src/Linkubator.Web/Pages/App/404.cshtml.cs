using ApplicationContracts = Linkubator.Application.Contracts;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;

namespace Linkubator.Web.Pages.App;

public sealed class AppNotFoundModel : PrivatePageModel
{
    public AppNotFoundModel(
        WebDevelopmentIdentity.IWebUserProfile userProfile,
        ApplicationContracts.ICollectionService collectionService)
        : base(userProfile, collectionService)
    {
    }

}
