using WebExtensions = Linkubator.Web.Extensions;

namespace Linkubator.Tests.Web;

public sealed class AliasExtensionsTests
{
    [Theory]
    [InlineData("my-demo-user", "my demo user")]
    [InlineData("usuario", "usuario")]
    [InlineData("ana-2", "ana 2")]
    public void GetDisplayName_WhenAliasHasHyphens_ReplacesThemWithSpaces(string alias, string expected)
    {
        string result = WebExtensions.AliasExtensions.GetDisplayName(alias);

        Assert.Equal(expected, result);
    }
}
