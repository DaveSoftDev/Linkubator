using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "Images")]
[Trait("Category", "URLs")]
public class ImageUrlPolicyTests
{
    [Fact]
    public void Adjust_WithRelativeUnicodeExpansion_CrossesFinalLengthBoundary()
    {
        const string prefix = "https://example.com/";
        string within = new string('a', 2048 - prefix.Length - 12) + "\U0001F600";
        string expected = prefix + new string('a', 2048 - prefix.Length - 12) + "%F0%9F%98%80";

        Assert.Equal(expected, DomainPolicies.ImageUrlPolicy.Adjust(within, prefix));
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.ImageUrlPolicy.Adjust("a" + within, prefix));
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.ImageUrlPolicy.Adjust(prefix + new string('a', 2048 - prefix.Length) + "\U0001F600", prefix));
    }

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
    [InlineData("cdn.example.com/a b.jpg")]
    [InlineData("cdn.example.com/a\u00A0b.jpg")]
    public void AdjustRejectsInteriorSpaces(string input)
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
    [InlineData("https://[2001:db8::1]/img.jpg")]
    [InlineData("https://[::ffff:127.0.0.1]/img.jpg")]
    [InlineData("https://[::1]:8443/img.jpg")]
    [InlineData("https://[fe80::1%25eth0]/img.jpg")]
    [InlineData("http://[::1]/img.jpg")]
    [InlineData("//[::1]/img.jpg")]
    [InlineData("[::1]:8443/img.jpg")]
    public void AdjustRejectsLocalhostAndIpHosts(string input)
    {
        Assert.Throws<DomainExceptions.ImageHostNotAllowedException>(() => DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Theory]
    [InlineData("/img/a.jpg", "https://192.168.1.1/post")]
    [InlineData("/img/a.jpg", "http://localhost/post")]
    [InlineData("/img/a.jpg", "https://[::1]/post")]
    [InlineData("/img/a.jpg", "https://[2001:db8::1]:8443/post")]
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
    public void AdjustRejectsRelativeUrlWhenResolutionPushesItOverMaximumLength()
    {
        string input = new('a', 2040);

        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() =>
            DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Fact]
    public void AdjustCountsUnicodeCodePointsForMaximumLength()
    {
        string prefix = "https://example.com/";
        string input = prefix + new string('a', 2048 - prefix.Length - 1) + "😀";

        string adjusted = DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post");

        Assert.Equal(input, adjusted);
        Assert.Equal(2048, DomainPolicies.UserTextPolicy.CountCodePoints(adjusted));
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

    [Theory]
    [InlineData("cdn.example.com/img.jpg?u=https://x.com", "https://example.com/a/b/cdn.example.com/img.jpg?u=https://x.com")]
    [InlineData("cdn.example.com/img.jpg#https://x", "https://example.com/a/b/cdn.example.com/img.jpg#https://x")]
    [InlineData("cdn.example.com/img.jpg?u=https://x.com&w=1", "https://example.com/a/b/cdn.example.com/img.jpg?u=https://x.com&w=1")]
    public void AdjustAcceptsImageWithSchemeLikeTokenInQueryOrFragment(string input, string expected)
    {
        Assert.Equal(expected, DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Theory]
    [InlineData("https://cdn.example.com/a%20b.jpg", "https://cdn.example.com/a%20b.jpg")]
    [InlineData("http://cdn.example.com/a%20b.jpg", "https://cdn.example.com/a%20b.jpg")]
    [InlineData("//cdn.example.com/a%20b.jpg", "https://cdn.example.com/a%20b.jpg")]
    [InlineData("https://cdn.example.com/img%2Fa.jpg", "https://cdn.example.com/img%2Fa.jpg")]
    [InlineData("https://EXAMPLE.com/a.jpg", "https://EXAMPLE.com/a.jpg")]
    [InlineData("https://cdn.example.com/a/../b.jpg", "https://cdn.example.com/a/../b.jpg")]
    [InlineData("img%20a.jpg", "https://example.com/a/b/img%20a.jpg")]
    [InlineData("../a%2Fb.jpg", "https://example.com/a/a%2Fb.jpg")]
    public void AdjustPreservesTheAdjustedUrlWithoutReserializingIt(string input, string expected)
    {
        Assert.Equal(expected, DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Theory]
    [InlineData("a\U0001F600b.jpg", "https://example.com/a/b/a%F0%9F%98%80b.jpg")]
    public void AdjustPercentEncodesNonAsciiCharactersOfRelativeReferences(string input, string expected)
    {
        Assert.Equal(expected, DomainPolicies.ImageUrlPolicy.Adjust(input, "https://example.com/a/b/post"));
    }

    [Fact]
    public void AdjustMeasuresTheLengthOverTheAdjustedUrlWithoutDecodedPercentEncodings()
    {
        string url = "https://example.com/" + string.Concat(Enumerable.Repeat("%20", 700));

        Assert.True(url.Length > 2048);
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() =>
            DomainPolicies.ImageUrlPolicy.Adjust(url, "https://example.com/a/b/post"));
    }

    [Fact]
    public void AdjustUpgradesAnHttpUrlWithoutPathToHttps()
    {
        string actual = DomainPolicies.ImageUrlPolicy.Adjust("http://example.com", "https://example.com/a/b/post");

        Assert.Equal("https://example.com", actual);
    }

    [Fact]
    public void AdjustRejectsARelativeReferenceThatUriCannotResolve()
    {
        Assert.Throws<DomainExceptions.UrlHostInvalidException>(() =>
            DomainPolicies.ImageUrlPolicy.Adjust("a|b.jpg", "https://example.com/a/b/post"));
    }

    [Fact]
    public void ReplaceHttpWithHttpsReturnsTheUrlWhenItDoesNotStartWithTheHttpScheme()
    {
        object? actual = PrivatePolicyInvoker.InvokeStatic(
            typeof(DomainPolicies.ImageUrlPolicy),
            "ReplaceHttpWithHttps",
            "http:example.com",
            new Uri("http://example.com"));

        Assert.Equal("http:example.com", actual as string);
    }

    [Fact]
    public void ResolveRelativeRejectsABaseUrlThatIsNullOrWhitespace()
    {
        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            PrivatePolicyInvoker.InvokeStatic(typeof(DomainPolicies.ImageUrlPolicy), "ResolveRelative", "image.jpg", "   "));
    }
}
