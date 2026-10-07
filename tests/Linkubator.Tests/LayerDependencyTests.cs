using NetArchTest.Rules;

namespace Linkubator.Tests;

public class LayerDependencyTests
{
    [Fact]
    public void WebTypesOutsideTheCompositionRootDoNotDependOnInfrastructure()
    {
        var infrastructureDependentTypes = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .HaveDependencyOn("Linkubator.Infrastructure")
            .GetTypes();

        Assert.NotEmpty(infrastructureDependentTypes);
        Assert.All(infrastructureDependentTypes, dependentType => Assert.Equal("Program", dependentType.Name));
    }
}