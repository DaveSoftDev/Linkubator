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
        Assert.Equal("/app/enlaces?c=1", Assert.Single(sidebar.Collections).Route);
    }

    [Theory]
    [InlineData(1, true, "collection")]
    [InlineData(999, false, null)]
    public void CreateSidebar_WithSelectedCollection_MarksOnlyOwnedMatchingItem(long selectedId, bool isActive, string? itemId)
    {
        Linkubator.Application.Dtos.Models.CollectionDto collection = new(
            1, "Collection", "collection", null, false, 0, DateTime.UnixEpoch);

        AppSidebarViewModel sidebar = AppSidebarViewModelFactory.CreateSidebar(
            AppSidebarSelectionKind.Collection, [collection], selectedCollectionId: selectedId);

        Assert.Equal(isActive, Assert.Single(sidebar.Collections).IsActive);
        Assert.Equal(itemId, sidebar.Selection.ItemId);
        Assert.Equal(isActive ? (long?)selectedId : null, sidebar.Selection.CollectionId);
        Assert.DoesNotContain(sidebar.Navigation, item => item.IsActive);
    }

    [Theory]
    [InlineData(AppSidebarSelectionKind.Home, "home")]
    [InlineData(AppSidebarSelectionKind.Links, "links")]
    [InlineData(AppSidebarSelectionKind.Collections, "collections")]
    [InlineData(AppSidebarSelectionKind.Collection, null)]
    public void CreateSidebar_WithSelection_ActivatesOnlyMatchingNavigationItem(AppSidebarSelectionKind kind, string? expectedId)
    {
        AppSidebarViewModel sidebar = AppSidebarViewModelFactory.CreateSidebar(kind, []);

        Assert.Equal(expectedId, sidebar.Navigation.SingleOrDefault(item => item.IsActive)?.Id);
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
