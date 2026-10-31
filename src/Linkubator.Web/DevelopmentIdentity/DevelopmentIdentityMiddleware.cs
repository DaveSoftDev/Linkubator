using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationModels = Linkubator.Application.Models;

namespace Linkubator.Web.DevelopmentIdentity;

public sealed class DevelopmentIdentityMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Crea el middleware con el siguiente componente de la canalización HTTP.
    /// </summary>
    public DevelopmentIdentityMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    /// <summary>
    /// Carga la identidad de desarrollo para las solicitudes privadas y continúa la canalización HTTP.
    /// </summary>
    public async Task InvokeAsync(
        HttpContext context,
        ApplicationContracts.IDevelopmentIdentityProvider identityProvider,
        DevelopmentCurrentUser currentUser)
    {
        if (context.Request.Path.StartsWithSegments("/app"))
        {
            ApplicationModels.DevelopmentIdentityProfile profile = await identityProvider.GetProfile(context.RequestAborted);
            currentUser.SetUserId(profile.UserId);
            currentUser.SetProfile(profile.Name, profile.Alias, profile.Email);
        }

        await _next(context);
    }
}