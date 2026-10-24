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

    public Task InvokeAsync(
        HttpContext context,
        ApplicationContracts.IDevelopmentIdentityProvider identityProvider,
        DevelopmentCurrentUser currentUser)
    {
        return InvokeAsync(context, identityProvider, currentUser, configuration: null);
    }

    private async Task InvokeAsync(
        HttpContext context,
        ApplicationContracts.IDevelopmentIdentityProvider identityProvider,
        DevelopmentCurrentUser currentUser,
        IConfiguration? configuration)
    {
        if (context.Request.Path.StartsWithSegments("/app"))
        {
            long userId = await identityProvider.GetUserId(context.RequestAborted);
            currentUser.SetUserId(userId);

            string name = configuration?["DevelopmentIdentity:Name"] ?? "Demo User";
            string alias = configuration?["DevelopmentIdentity:Alias"] ?? "my-demo-user";
            currentUser.SetProfile(name, alias);
        }

        await _next(context);
    }
}