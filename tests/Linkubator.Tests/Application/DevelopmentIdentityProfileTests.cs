using Linkubator.Application.Models;

namespace Linkubator.Tests.Application;

public class DevelopmentIdentityProfileTests
{
    [Fact]
    public void Constructor_WhenValuesAreProvided_StoresTheValues()
    {
        DevelopmentIdentityProfile profile = new(42, "Demo User", "demo-user");

        Assert.Equal(42, profile.UserId);
        Assert.Equal("Demo User", profile.Name);
        Assert.Equal("demo-user", profile.Alias);
    }

    [Theory]
    [InlineData(42, "Demo User", "demo-user", true)]
    [InlineData(0, "Demo User", "demo-user", false)]
    [InlineData(-1, "Demo User", "demo-user", false)]
    [InlineData(42, "", "demo-user", false)]
    [InlineData(42, "   ", "demo-user", false)]
    [InlineData(42, "Demo User", "", false)]
    [InlineData(42, "Demo User", "   ", false)]
    public void IsValid_WhenStateChanges_ReturnsExpectedValue(long userId, string name, string alias, bool expected)
    {
        DevelopmentIdentityProfile profile = new(userId, name, alias);

        Assert.Equal(expected, profile.IsValid);
    }
}
