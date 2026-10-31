using ApplicationContracts = Linkubator.Application.Contracts;
using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using LinkReturnRoute = Linkubator.Web.Navigation.LinkReturnRoute;
using PrivateResourceUnavailablePresenter = Linkubator.Web.Models.Presentation.PrivateResourceUnavailablePresenter;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Pages.App.Links;

public abstract class LinkManagementPageModel : PrivatePageModel
{
    /// <summary>
    /// Inicializa el modelo base de gestión de enlaces con el perfil y servicio de colecciones.
    /// </summary>
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

    /// <summary>
    /// Redirige al listado cuando el enlace solicitado no está disponible.
    /// </summary>
    protected IActionResult RedirectUnavailableLink()
    {
        TempData[nameof(LinksModel.LinkManagementError)] = PrivateResourceUnavailablePresenter.GenericMessage;
        return LocalRedirect($"{Request.PathBase}{AppRoutes.Links}");
    }
}
