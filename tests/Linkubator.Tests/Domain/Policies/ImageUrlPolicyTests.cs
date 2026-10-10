using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class ImageUrlPolicyTests
{
    [Theory]
    [InlineData("https://cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("http://cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("//cdn.example.com/img/img-01.jpg", "https://cdn.example.com/img/img-01.jpg")]
    [InlineData("cdn.example.com/img.jpg", "https://example.com/a/b/cdn.example.com/img.jpg", "https://example.com/a/b/post")]
    [InlineData("../assets/cover.webp?width=800&format=webp", "https://example.com/a/assets/cover.webp?width=800&format=webp", "https://example.com/a/b/post")]
    [InlineData("?width=1200", "https://example.com/a/b/post?width=1200", "https://example.com/a/b/post?old=1")]
    public void AdjustResolvesUrlsUsingTheConfiguredBase(
        string input,
        string expected,
        string? baseUrl = null)
    {
        if (baseUrl is null)
        {
            baseUrl = "https://example.com/a/b/post";
        }

        Assert.Equal(expected, DomainPolicies.ImageUrlPolicy.Adjust(input, baseUrl));
    }

    [Fact]
    public void AdjustReturnsEmptyStringForEmptyOrWhitespaceInput()
    {
        Assert.Equal(string.Empty, DomainPolicies.ImageUrlPolicy.Adjust(string.Empty, "https://example.com/a/b/post"));
        Assert.Equal(string.Empty, DomainPolicies.ImageUrlPolicy.Adjust("   ", "https://example.com/a/b/post"));
    }

    [Fact]
    public void AdjustRejectsNullOrWhitespaceBaseUrl()
    {
        Assert.Throws<ArgumentNullException>(() => DomainPolicies.ImageUrlPolicy.Adjust("https://cdn.example.com/img.jpg", null!));
        Assert.Throws<ArgumentException>(() => DomainPolicies.ImageUrlPolicy.Adjust("https://cdn.example.com/img.jpg", "   "));
    }

    [Theory]
    [InlineData("ana\nlopez")]
    [InlineData("ana\tlopez")]
    [InlineData("ana\u0007lopez")]
    public void AdjustRejectsControlsAndLineBreaks(string input)
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:ana@example.com")]
    [InlineData("ftp://example.com")]
    public void AdjustRejectsUnsupportedSchemes(string input)
    {
        Assert.Throws<DomainExceptions.UrlSchemeNotAllowedException>(() => DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Theory]
    [InlineData("localhost:5000/img.jpg")]
    [InlineData("https://192.168.1.1/img.jpg")]
    [InlineData("https://[::1]/img.jpg")]
    public void AdjustRejectsLocalhostAndIpHosts(string input)
    {
        Assert.Throws<DomainExceptions.ImageHostNotAllowedException>(() => DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Fact]
    public void AdjustRejectsUrlsThatExceedTheMaximumLength()
    {
        string url = "https://example.com/" + new string('a', 2048);
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.ImageUrlPolicy.Adjust(url, "https://example.com/a/b/post"));
    }
}
