using WebExtensions = Linkubator.Web.Extensions;
using WebModels = Linkubator.Web.Models.AppDashboard;

namespace Linkubator.Tests.Web;

public sealed class UrlExtensionsTests
{
    [Theory]
    [InlineData("https://example.com/articles/read?source=home#intro", "example.com")]
    [InlineData("http://www.example.com:8080/articles", "www.example.com")]
    [InlineData("https://docs.example.com/guide", "docs.example.com")]
    [InlineData("https://EXAMPLE.COM/guide", "example.com")]
    [InlineData("http://localhost:5228/app", "localhost")]
    [InlineData("https://127.0.0.1:7263/app", "127.0.0.1")]
    [InlineData("not-a-url", "not-a-url")]
    [InlineData("", "")]
    public void GetDomain_WithUrl_PreservesExistingPrivateDisplay(string url, string expectedDomain)
    {
        Assert.Equal(expectedDomain, WebExtensions.UrlExtensions.GetDomain(url));
    }

    [Fact]
    public void Domain_WithPrivateLink_UsesSharedDomainFormatting()
    {
        WebModels.LinkCardViewModel model = new()
        {
            Url = "https://docs.example.com:8443/guide?language=es#intro",
        };

        Assert.Equal("docs.example.com", model.Domain);
    }
}
