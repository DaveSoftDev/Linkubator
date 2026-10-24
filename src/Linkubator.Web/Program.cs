using Linkubator.Infrastructure;
using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.DevelopmentIdentity;
using Microsoft.AspNetCore.HostFiltering;
using Serilog;
using ApplicationContracts = Linkubator.Application.Contracts;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;
using DashboardService = Linkubator.Application.UseCases.Dashboard.DashboardService;

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
builder.Services.AddScoped<ApplicationContracts.IDashboardService, DashboardService>();
builder.Services.AddScoped<WebDevelopmentIdentity.DevelopmentCurrentUser>();
builder.Services.AddScoped<ApplicationContracts.ICurrentUser>(serviceProvider =>
    serviceProvider.GetRequiredService<WebDevelopmentIdentity.DevelopmentCurrentUser>());
builder.Services.AddRazorPages();

WebApplication app = builder.Build();

using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    ISqliteMigrationService migrationService = scope.ServiceProvider.GetRequiredService<ISqliteMigrationService>();
    await migrationService.ApplyPendingMigrations();

    DevelopmentIdentityProvisioner provisioner = scope.ServiceProvider.GetRequiredService<DevelopmentIdentityProvisioner>();
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
    if (ctx.HttpContext.Response.StatusCode == 404 && (ctx.HttpContext.Request.Path != "/404" && ctx.HttpContext.Request.Path != "/app/404"))
    {
        string requestPath = ctx.HttpContext.Request.Path.Value ?? string.Empty;

        string targetPath = requestPath.StartsWith("/app", StringComparison.OrdinalIgnoreCase)
            ? "/app/404"
            : "/404";
        ctx.HttpContext.SetEndpoint(null);
        ctx.HttpContext.Request.Path = targetPath;
        await ctx.Next(ctx.HttpContext);
    }
});

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseMiddleware<WebDevelopmentIdentity.DevelopmentIdentityMiddleware>();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();

public partial class Program;
