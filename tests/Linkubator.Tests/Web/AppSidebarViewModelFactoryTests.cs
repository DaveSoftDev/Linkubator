using Linkubator.Web.Models.AppSidebar;

namespace Linkubator.Tests.Web;

public sealed class AppSidebarViewModelFactoryTests
{
    [Fact]
    public void CreateSidebar_WhenUserProvided_UsesDevelopmentIdentityData()
    {
        // Arrange
        AppSidebarUserViewModel user = new()
        {
            Name = "Demo User",
            Alias = "my-demo-user",
        };

        // Act
        AppSidebarViewModel sidebar = AppSidebarViewModelFactory.CreateSidebar(
            AppSidebarSelectionKind.Home,
            [],
            user);

        // Assert
        Assert.Equal("Demo User", sidebar.User.Name);
        Assert.Equal("my-demo-user", sidebar.User.Alias);
    }

    [Fact]
    public void CreateDefault_WhenUserProvided_UsesDevelopmentIdentityData()
    {
        // Arrange
        AppSidebarUserViewModel user = new()
        {
            Name = "Demo User",
            Alias = "my-demo-user",
        };

        // Act
        AppSidebarViewModel sidebar = AppSidebarViewModelFactory.CreateDefault(user);

        // Assert
        Assert.Equal("Demo User", sidebar.User.Name);
        Assert.Equal("my-demo-user", sidebar.User.Alias);
    }
}
