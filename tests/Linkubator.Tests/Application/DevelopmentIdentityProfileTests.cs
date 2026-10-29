using Linkubator.Application.Models;

namespace Linkubator.Tests.Application;

public class DevelopmentIdentityProfileTests
{
    [Fact]
    public void Constructor_WhenValuesAreProvided_StoresTheValues()
    {
        DevelopmentIdentityProfile profile = new(42, "Demo User", "demo-user", "user@example.test");

        Assert.Equal(42, profile.UserId);
        Assert.Equal("Demo User", profile.Name);
        Assert.Equal("demo-user", profile.Alias);
        Assert.Equal("user@example.test", profile.Email);
    }

    [Theory]
    [InlineData(42, "Demo User", "demo-user", "user@example.test", true)]
    [InlineData(0, "Demo User", "demo-user", "user@example.test", false)]
    [InlineData(-1, "Demo User", "demo-user", "user@example.test", false)]
    [InlineData(42, "", "demo-user", "user@example.test", false)]
    [InlineData(42, "   ", "demo-user", "user@example.test", false)]
    [InlineData(42, "Demo User", "", "user@example.test", false)]
    [InlineData(42, "Demo User", "   ", "user@example.test", false)]
    [InlineData(42, "Demo User", "demo-user", "", false)]
    [InlineData(42, "Demo User", "demo-user", "   ", false)]
    public void IsValid_WhenStateChanges_ReturnsExpectedValue(long userId, string name, string alias, string email, bool expected)
    {
        DevelopmentIdentityProfile profile = new(userId, name, alias, email);

        Assert.Equal(expected, profile.IsValid);
    }
}
