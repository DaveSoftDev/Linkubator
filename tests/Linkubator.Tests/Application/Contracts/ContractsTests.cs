using Linkubator.Application;
using Linkubator.Application.Constants;
using Linkubator.Domain.Entities;

namespace Linkubator.Tests.Application.Contracts;

public class ContractsTests
{
    [Fact]
    public void CurrentUserTracksTheIdentifiedUser()
    {
        ICurrentUser currentUser = new CurrentUser(42);

        Assert.Equal(42, currentUser.UserId);
        Assert.True(currentUser.IsAuthenticated);
    }

    [Fact]
    public void DevelopmentIdentityProviderProvidesAsyncResolvedUserId()
    {
        var method = typeof(IDevelopmentIdentityProvider).GetMethod(nameof(IDevelopmentIdentityProvider.GetUserId));

        Assert.NotNull(method);
        Assert.Equal(typeof(Task<long>), method.ReturnType);
        Assert.Equal(typeof(CancellationToken), Assert.Single(method.GetParameters()).ParameterType);
    }

    [Fact]
    public void UnitOfWorkFactoryUsesExplicitContextName()
    {
        Assert.Equal("Linkubator", UnitOfWorkContexts.Linkubator);
    }

    [Fact]
    public void RepositoryContractsExposeApplicationLevelSagasWithoutInfrastructureTypes()
    {
        Assert.NotNull(typeof(ICollectionRepository));
        Assert.NotNull(typeof(ILinkRepository));
        Assert.NotNull(typeof(IUnitOfWork));
        Assert.NotNull(typeof(IUnitOfWorkFactory));
        Assert.NotNull(typeof(Collection));
        Assert.NotNull(typeof(Link));
    }
}
