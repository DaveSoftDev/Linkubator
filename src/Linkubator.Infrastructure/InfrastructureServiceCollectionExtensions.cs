using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddSingleton<ISqliteMigrationService, SqliteMigrationService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}