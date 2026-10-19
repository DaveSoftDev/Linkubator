using Linkubator.Infrastructure;
using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.DevelopmentIdentity;
using Serilog;
using ApplicationContracts = Linkubator.Application.Contracts;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddInfrastructure(builder.Configuration);
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

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
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
