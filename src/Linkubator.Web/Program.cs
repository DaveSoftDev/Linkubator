using Linkubator.Infrastructure;
using Microsoft.AspNetCore.HostFiltering;
using Serilog;
using ApplicationContracts = Linkubator.Application.Contracts;
using CollectionService = Linkubator.Application.UseCases.Collections.CollectionService;
using DashboardService = Linkubator.Application.UseCases.Dashboard.DashboardService;
using InfrastructureContracts = Linkubator.Infrastructure.Contracts;
using InfrastructureDevelopmentIdentity = Linkubator.Infrastructure.DevelopmentIdentity;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using WebLogging = Linkubator.Web.Logging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string[] allowedHosts = builder.Configuration.GetValue<string>("AllowedHosts")?
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["localhost", "127.0.0.1", "[::1]"];

builder.Services.Configure<HostFilteringOptions>(options => options.AllowedHosts = allowedHosts);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ApplicationContracts.ICollectionService, CollectionService>();
builder.Services.AddScoped<ApplicationContracts.IDashboardService, DashboardService>();
builder.Services.AddScoped<WebDevelopmentIdentity.DevelopmentCurrentUser>();
builder.Services.AddScoped<WebDevelopmentIdentity.IWebUserProfile>(serviceProvider =>
    serviceProvider.GetRequiredService<WebDevelopmentIdentity.DevelopmentCurrentUser>());
builder.Services.AddScoped<ApplicationContracts.ICurrentUser>(serviceProvider =>
    serviceProvider.GetRequiredService<WebDevelopmentIdentity.DevelopmentCurrentUser>());
builder.Services.AddRazorPages();

WebApplication app = builder.Build();

using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    InfrastructureContracts.ISqliteMigrationService migrationService = scope.ServiceProvider.GetRequiredService<InfrastructureContracts.ISqliteMigrationService>();
    await migrationService.ApplyPendingMigrations();

    InfrastructureDevelopmentIdentity.DevelopmentIdentityProvisioner provisioner = scope.ServiceProvider.GetRequiredService<InfrastructureDevelopmentIdentity.DevelopmentIdentityProvisioner>();
    await provisioner.Provision();

    ApplicationContracts.IDevelopmentIdentityProvider identityProvider =
        scope.ServiceProvider.GetRequiredService<ApplicationContracts.IDevelopmentIdentityProvider>();
    await identityProvider.GetUserId();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHostFiltering();
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Content-Security-Policy"] = "frame-ancestors 'none'";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        return Task.CompletedTask;
    });

    await next();
});
app.UseStatusCodePages(async ctx =>
{
    HttpContext httpContext = ctx.HttpContext;
    string requestPath = httpContext.Request.Path.Value ?? string.Empty;
    bool privatePath = requestPath.StartsWith("/app", StringComparison.OrdinalIgnoreCase);
    bool unsupportedPrivateMethod = privatePath
        && httpContext.Request.Method is not ("GET" or "POST")
        && (ctx.HttpContext.Response.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status405MethodNotAllowed);

    if (unsupportedPrivateMethod)
    {
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        httpContext.SetEndpoint(null);
        httpContext.Request.Path = Linkubator.Web.Constants.AppRoutes.PrivateNotFoundPage;
        httpContext.Request.Method = HttpMethods.Get;
        await ctx.Next(httpContext);
    }
    else if (ctx.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound
        && httpContext.Request.Path != Linkubator.Web.Constants.AppRoutes.PublicNotFoundPage
        && httpContext.Request.Path != Linkubator.Web.Constants.AppRoutes.PrivateNotFoundPage)
    {
        string targetPath = privatePath
            ? Linkubator.Web.Constants.AppRoutes.PrivateNotFoundPage
            : Linkubator.Web.Constants.AppRoutes.PublicNotFoundPage;
        httpContext.SetEndpoint(null);
        httpContext.Request.Path = targetPath;
        await ctx.Next(httpContext);
    }
});
app.UseHttpsRedirection();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        WebLogging.RequestLoggingDiagnosticContext.Enrich(diagnosticContext, httpContext);
    };
});
app.UseRouting();
app.UseMiddleware<WebDevelopmentIdentity.DevelopmentIdentityMiddleware>();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
}
