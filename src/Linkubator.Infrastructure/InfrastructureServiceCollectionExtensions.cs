using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.DevelopmentIdentity;
using Linkubator.Infrastructure.Options;
using Linkubator.Infrastructure.Repositories;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Linkubator.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string named 'DefaultConnection' is required.");
        }

        services.AddSingleton(new SqliteConnectionFactory(connectionString));
        services.AddSingleton<ISqliteConnectionFactory>(serviceProvider =>
            serviceProvider.GetRequiredService<SqliteConnectionFactory>());

        services.Configure<DevelopmentIdentityOptions>(configuration.GetSection("DevelopmentIdentity"));
        services.AddSingleton(provider =>
        {
            DevelopmentIdentityOptions configured = provider.GetRequiredService<IOptions<DevelopmentIdentityOptions>>().Value;
            configured.Email ??= "user@linkubator.test";
            configured.Name ??= "Demo User";
            configured.Alias ??= "my-demo-user";
            configured.PasswordHash ??= string.Empty;
            return configured;
        });

        services.AddSingleton<DevelopmentIdentityProvisioner>();
        services.AddSingleton<IDevelopmentIdentityProvider, DevelopmentIdentityProvider>();

        services.AddScoped<IUnitOfWorkFactory, SqliteUnitOfWorkFactory>();
        services.AddScoped(serviceProvider =>
            serviceProvider.GetRequiredService<ISqliteConnectionFactory>().CreateOpenConnection());
        services.AddScoped<ICollectionRepository>(serviceProvider =>
            new SqliteCollectionRepository(serviceProvider.GetRequiredService<SqliteConnection>()));
        services.AddScoped<ILinkRepository>(serviceProvider =>
            new SqliteLinkRepository(serviceProvider.GetRequiredService<SqliteConnection>()));
        services.AddScoped<IDashboardRepository>(serviceProvider =>
            new SqliteDashboardRepository(serviceProvider.GetRequiredService<SqliteConnection>()));
        services.AddScoped<IPublicReadRepository>(serviceProvider =>
            new SqlitePublicReadRepository(serviceProvider.GetRequiredService<SqliteConnection>()));

        services.AddSingleton<ISqliteMigrationService>(serviceProvider =>
            new SqliteMigrationService(serviceProvider.GetRequiredService<ISqliteConnectionFactory>()));

        return services;
    }
}