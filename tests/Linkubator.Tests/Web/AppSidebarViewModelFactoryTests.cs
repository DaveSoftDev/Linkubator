using Linkubator.Web.Models.AppSidebar;

namespace Linkubator.Tests.Web;

public sealed class AppSidebarViewModelFactoryTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateSidebar_WithCollection_PreservesPublicState(bool isPublic)
    {
        Linkubator.Application.Dtos.Models.CollectionDto collection = new(
            1, "Collection", "collection", null, isPublic, 0, DateTime.UnixEpoch);

        AppSidebarViewModel sidebar = AppSidebarViewModelFactory.CreateSidebar(
            AppSidebarSelectionKind.Collections,
            [collection]);

        Assert.Equal(isPublic, Assert.Single(sidebar.Collections).IsPublic);
    }

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
    public void CreateSidebar_WithEmptyCollections_UsesDevelopmentIdentityData()
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
        Assert.Empty(sidebar.Collections);
        Assert.Equal(0, sidebar.Navigation.First(item => item.Id == "links").Count);
    }
}
