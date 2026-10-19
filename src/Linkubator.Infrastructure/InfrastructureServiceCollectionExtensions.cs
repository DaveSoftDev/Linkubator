using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.Repositories;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;

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

        services.AddSingleton<ISqliteMigrationService, SqliteMigrationService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}