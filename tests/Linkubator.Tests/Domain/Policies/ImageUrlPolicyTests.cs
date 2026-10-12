using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "Images")]
[Trait("Category", "URLs")]
public class ImageUrlPolicyTests
{
    [Theory]
    [InlineData("https://cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("http://cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("HTTP://cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("http://cdn.example.com:8080/img.jpg", "https://cdn.example.com:8080/img.jpg")]
    [InlineData("http://cdn.example.com:80/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("//cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("cdn.example.com:8080/img.jpg", "https://cdn.example.com:8080/img.jpg")]
    [InlineData("/img/a.jpg", "https://example.com/img/a.jpg", "http://example.com/post")]
    [InlineData("img.jpg", "https://example.com/a/b/img.jpg", "http://example.com/a/b/post")]
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
    public void AdjustRejectsNullOrWhitespaceInputOrBaseUrl()
    {
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.ImageUrlPolicy.Adjust(null!, "https://example.com/a/b/post"));
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.ImageUrlPolicy.Adjust(string.Empty, "https://example.com/a/b/post"));
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.ImageUrlPolicy.Adjust("   ", "https://example.com/a/b/post"));
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.ImageUrlPolicy.Adjust("https://cdn.example.com/img.jpg", null!));
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.ImageUrlPolicy.Adjust("https://cdn.example.com/img.jpg", "   "));
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
    [InlineData("https://")]
    public void AdjustRejectsUnsupportedSchemesOrMissingAuthority(string input)
    {
        DomainExceptions.DomainException exception = Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));

        if (input == "https://")
        {
            Assert.IsType<DomainExceptions.UrlHostInvalidException>(exception);
        }
        else
        {
            Assert.IsType<DomainExceptions.UrlSchemeNotAllowedException>(exception);
        }
    }

    [Theory]
    [InlineData("localhost:5000/img.jpg")]
    [InlineData("http://localhost/img.jpg")]
    [InlineData("https://LOCALHOST/img.jpg")]
    [InlineData("//localhost/img.jpg")]
    [InlineData("http://127.0.0.1/img.jpg")]
    [InlineData("https://192.168.1.1/img.jpg")]
    [InlineData("https://[::1]/img.jpg")]
    public void AdjustRejectsLocalhostAndIpHosts(string input)
    {
        Assert.Throws<DomainExceptions.ImageHostNotAllowedException>(() => DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Theory]
    [InlineData("/img/a.jpg", "https://192.168.1.1/post")]
    [InlineData("/img/a.jpg", "http://localhost/post")]
    public void AdjustRejectsRelativeReferencesResolvedAgainstLocalhostOrIpBases(string input, string baseUrl)
    {
        Assert.Throws<DomainExceptions.ImageHostNotAllowedException>(() => DomainPolicies.ImageUrlPolicy.Adjust(input, baseUrl));
    }

    [Fact]
    public void AdjustRejectsMalformedBaseUrlForRelativeReference()
    {
        Assert.Throws<DomainExceptions.UrlHostInvalidException>(() =>
            DomainPolicies.ImageUrlPolicy.Adjust("image.jpg", "not-a-url"));
    }

    [Fact]
    public void AdjustRejectsUrlsThatExceedTheMaximumLength()
    {
        string url = "https://example.com/" + new string('a', 2048);
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() =>
            DomainPolicies.ImageUrlPolicy.Adjust(url, "https://example.com/a/b/post"));
    }

    [Fact]
    public void AdjustAcceptsUrlAtMaximumLength()
    {
        string prefix = "https://example.com/";
        string url = prefix + new string('a', 2048 - prefix.Length);

        Assert.Equal(2048, url.Length);
        Assert.Equal(url, DomainPolicies.ImageUrlPolicy.Adjust(url, "https://example.com/a/b/post"));
    }

    [Fact]
    public void AdjustTrimsWhitespaceAroundAbsoluteUrl()
    {
        string actual = DomainPolicies.ImageUrlPolicy.Adjust(
            "  https://cdn.example.com/img.jpg  ",
            "https://example.com/a/b/post");

        Assert.Equal("https://cdn.example.com/img.jpg", actual);
    }

    [Fact]
    public void AdjustRejectsMalformedUnicode()
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() =>
            DomainPolicies.ImageUrlPolicy.Adjust("\uD800", "https://example.com/a/b/post"));
    }

    [Fact]
    public void AdjustResolvesFragmentOnlyReferenceAgainstHttpBase()
    {
        string actual = DomainPolicies.ImageUrlPolicy.Adjust(
            "#new-section",
            "http://example.com:8080/a/b/post?width=800#old-section");

        Assert.Equal("https://example.com:8080/a/b/post?width=800#new-section", actual);
    }
}
