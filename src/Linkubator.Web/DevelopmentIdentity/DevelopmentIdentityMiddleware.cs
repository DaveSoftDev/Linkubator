using ApplicationContracts = Linkubator.Application.Contracts;

namespace Linkubator.Web.DevelopmentIdentity;

public sealed class DevelopmentIdentityMiddleware
{
    private readonly RequestDelegate _next;

    public DevelopmentIdentityMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ApplicationContracts.IDevelopmentIdentityProvider identityProvider,
        DevelopmentCurrentUser currentUser)
    {
        if (context.Request.Path.StartsWithSegments("/app"))
        {
            long userId = await identityProvider.GetUserId(context.RequestAborted);
            currentUser.SetUserId(userId);
        }

        await _next(context);
    }
}