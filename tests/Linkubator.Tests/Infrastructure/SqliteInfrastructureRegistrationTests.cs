using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure;
using Linkubator.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteInfrastructureRegistrationTests
{
    [Fact]
    public async Task AddInfrastructure_WhenConnectionStringIsConfigured_ResolvesUnitOfWorkAndAllRepositories()
    {
        await using SqliteRepositoryTestDatabase database = await SqliteRepositoryTestDatabase.Create();
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = database.ConnectionString
            })
            .Build();
        services.AddInfrastructure(configuration);
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>().Get(UnitOfWorkContexts.Linkubator));
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICollectionRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ILinkRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IDashboardRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPublicReadRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ISqliteConnectionFactory>());

        Microsoft.Data.Sqlite.SqliteConnection connection = scope.ServiceProvider.GetRequiredService<Microsoft.Data.Sqlite.SqliteConnection>();
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);

        await scope.DisposeAsync();

        Assert.Equal(System.Data.ConnectionState.Closed, connection.State);
    }

    [Fact]
    public void AddInfrastructure_WhenConnectionStringIsMissing_ThrowsConfigurationError()
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => services.AddInfrastructure(configuration));

        Assert.Equal("A connection string named 'DefaultConnection' is required.", exception.Message);
    }
}
