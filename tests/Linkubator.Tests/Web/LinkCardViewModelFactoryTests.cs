using LinkDto = Linkubator.Application.Dtos.Models.LinkDto;
using LinkCardViewModelFactory = Linkubator.Web.Models.AppDashboard.LinkCardViewModelFactory;

namespace Linkubator.Tests.Web;

public sealed class LinkCardViewModelFactoryTests
{
    [Theory]
    [InlineData("DAEFIDY", "green")]
    [InlineData("daefidy", "green")]
    [InlineData("  DAEFIDY  ", "green")]
    [InlineData("DAEDIFY", "yellow")]
    [InlineData("UNA COLECCION QUE ES MAS LARGA", "amber")]
    [InlineData("Colección A", "amber")]
    [InlineData("Colección B", "rose")]
    [InlineData("Colección C", "cyan")]
    [InlineData("", "blue")]
    [InlineData("   ", "blue")]
    public void ResolveAccent_WhenNameIsProvided_ReturnsExpectedColor(string name, string expectedColor)
    {
        string color = LinkCardViewModelFactory.ResolveAccent(name);

        Assert.Equal(expectedColor, color);
    }

    [Theory]
    [InlineData("DAEFIDY")]
    [InlineData("daefidy")]
    public void Create_WhenCollectionHashIsMinimumInteger_ReturnsCardWithExpectedColor(string collectionName)
    {
        LinkDto link = new(1, 2, "https://example.com", "examplecom", null, null, null, false, DateTime.UnixEpoch)
        {
            CollectionName = collectionName,
        };

        var card = LinkCardViewModelFactory.Create(link);

        Assert.Equal("green", card.AccentClass);
    }
}
