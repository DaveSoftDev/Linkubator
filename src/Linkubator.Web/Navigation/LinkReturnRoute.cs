using AppRoutes = Linkubator.Web.Constants.AppRoutes;
using Microsoft.AspNetCore.Mvc;

namespace Linkubator.Web.Navigation;

public static class LinkReturnRoute
{
    /// <summary>
    /// Resuelve una URL local de retorno admitida o utiliza la ruta predeterminada de enlaces.
    /// </summary>
    public static string Resolve(IUrlHelper url, PathString pathBase, string? returnUrl, bool allowCollections = false)
    {
        string defaultRoute = $"{pathBase}{AppRoutes.Links}";
        if (string.IsNullOrWhiteSpace(returnUrl) || !url.IsLocalUrl(returnUrl))
        {
            return defaultRoute;
        }

        int separator = returnUrl.IndexOfAny(['?', '#']);
        string path = (separator < 0 ? returnUrl : returnUrl[..separator]).TrimEnd('/');
        bool isAllowed = string.Equals(path, $"{pathBase}{AppRoutes.Root}", StringComparison.OrdinalIgnoreCase)
            || string.Equals(path, defaultRoute, StringComparison.OrdinalIgnoreCase)
            || (allowCollections && string.Equals(path, $"{pathBase}{AppRoutes.Collections}", StringComparison.OrdinalIgnoreCase));
        return isAllowed ? returnUrl : defaultRoute;
    }
}
