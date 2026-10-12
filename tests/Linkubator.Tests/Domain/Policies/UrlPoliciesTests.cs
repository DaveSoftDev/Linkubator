using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "URLs")]
[Trait("Category", "Links")]
public class UrlPoliciesTests
{
    #region Hosts

    [Trait("Category", "Hosts")]
    [Theory]
    [InlineData("https://example.com/path", "example.com")]
    [InlineData("localhost:5000/path", "localhost")]
    [InlineData("example.com:8080/x", "example.com")]
    [InlineData("https://EXAMPLE.com:8443", "EXAMPLE.com")]
    public void ExtractHostReturnsTheHostWithoutPortOrPath(string input, string expected)
    {
        Assert.Equal(expected, DomainPolicies.UrlHostPolicy.ExtractHost(input));
    }

    [Trait("Category", "Hosts")]
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://")]
    [InlineData("http://:8080")]
    [InlineData("https://example.com:abc")]
    public void ExtractHostRejectsInvalidHosts(string input)
    {
        Assert.Throws<DomainExceptions.UrlHostInvalidException>(() => DomainPolicies.UrlHostPolicy.ExtractHost(input));
    }

    [Trait("Category", "Hosts")]
    [Theory]
    [InlineData("localhost:5000/path", true, "localhost")]
    [InlineData("example.com:8080/x", true, "example.com")]
    [InlineData("https://example.com/path", true, "example.com")]
    [InlineData("intranet:8080", true, "intranet")]
    public void TryExtractHostBehavesAsAPredicate(string input, bool expectedSuccess, string? expectedHost)
    {
        bool success = DomainPolicies.UrlHostPolicy.TryExtractHost(input, out string? extractedHost);

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedHost, extractedHost);
    }

    #endregion

    #region Schemes

    [Trait("Category", "Schemes")]
    [Theory]
    [InlineData("localhost:5000/path", true, false, null, "localhost")]
    [InlineData("example.com:8080/x", true, false, null, "example.com")]
    [InlineData("example.com:8080?x=1", true, false, null, "example.com")]
    [InlineData("example.com:8080#section", true, false, null, "example.com")]
    [InlineData("https://example.com/path", true, true, "https", "example.com")]
    [InlineData("https:example.com", false, true, "https", null)]
    [InlineData("intranet:8080", false, true, "intranet", null)]
    [InlineData("mailto:ana@example.com", false, true, "mailto", null)]
    [InlineData("bad_scheme:value", false, false, null, null)]
    [InlineData("not a host", false, false, null, null)]
    public void DetectClassifiesHostAndExplicitSchemeInputs(
        string input,
        bool expectedHostPort,
        bool expectedExplicitScheme,
        string? expectedScheme,
        string? expectedHost)
    {
        var detection = DomainPolicies.UriSchemePolicy.Detect(input);

        Assert.Equal(expectedHostPort, detection.HasHostAndPort);
        Assert.Equal(expectedExplicitScheme, detection.HasExplicitScheme);
        Assert.Equal(expectedScheme, detection.Scheme);
        Assert.Equal(expectedHost, detection.Host);
    }

    #endregion

    #region Links

    [Trait("Category", "Links")]
    [Theory]
    [InlineData("https://example.com/path", "https://example.com/path")]
    [InlineData("//example.com/other", "https://example.com/other")]
    [InlineData("localhost:5000/path", "http://localhost:5000/path")]
    [InlineData("localhost:5000/admin", "http://localhost:5000/admin")]
    [InlineData("localhost", "http://localhost")]
    [InlineData("LOCALHOST:5000/x", "http://LOCALHOST:5000/x")]
    [InlineData("localhost.example.com/x", "https://localhost.example.com/x")]
    [InlineData("//localhost:5000/x", "https://localhost:5000/x")]
    [InlineData("http://localhost:5000/x", "http://localhost:5000/x")]
    [InlineData("https://localhost:5000/x", "https://localhost:5000/x")]
    [InlineData("example.com/articulo", "https://example.com/articulo")]
    [InlineData("192.168.1.1/x", "https://192.168.1.1/x")]
    [InlineData("example.com/r?u=https://x.com", "https://example.com/r?u=https://x.com")]
    [InlineData("example.com/a#https://x", "https://example.com/a#https://x")]
    [InlineData("localhost:5000/r?u=http://x.com", "http://localhost:5000/r?u=http://x.com")]
    [InlineData("https://example.com/r?u=ftp://x.com", "https://example.com/r?u=ftp://x.com")]
    [InlineData("example.com:8080/x", "https://example.com:8080/x")]
    [InlineData("example.com:8080?x=1", "https://example.com:8080?x=1")]
    [InlineData("example.com:8080#section", "https://example.com:8080#section")]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("https://Example.com/a", "https://Example.com/a")]
    public void AdjustReturnsTheAdjustedUrl(string input, string expected)
    {
        Assert.Equal(expected, DomainPolicies.LinkUrlPolicy.Adjust(input));
    }

    [Trait("Category", "Links")]
    [Fact]
    public void AdjustRejectsNullOrWhitespaceInput()
    {
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.LinkUrlPolicy.Adjust(null!));
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.LinkUrlPolicy.Adjust(string.Empty));
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.LinkUrlPolicy.Adjust("   "));
    }

    [Trait("Category", "Links")]
    [Theory]
    [InlineData("ana\nlopez")]
    [InlineData("ana\tlopez")]
    [InlineData("ana\u0007lopez")]
    public void AdjustRejectsControlsAndLineBreaks(string input)
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.LinkUrlPolicy.Adjust(input));
    }

    [Trait("Category", "Links")]
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:ana@example.com")]
    [InlineData("ftp://example.com")]
    public void AdjustRejectsUnsupportedSchemes(string input)
    {
        Assert.Throws<DomainExceptions.UrlSchemeNotAllowedException>(() => DomainPolicies.LinkUrlPolicy.Adjust(input));
    }

    [Trait("Category", "Links")]
    [Theory]
    [InlineData("http:example.com")]
    [InlineData("https:example.com")]
    public void AdjustRejectsMissingAuthorityAfterHttpScheme(string input)
    {
        Assert.Throws<DomainExceptions.UrlAuthorityMissingException>(() => DomainPolicies.LinkUrlPolicy.Adjust(input));
    }

    [Trait("Category", "Links")]
    [Fact]
    public void AdjustRejectsSchemeCandidateWithUnsupportedCharactersAsInvalidHost()
    {
        Assert.Throws<DomainExceptions.UrlHostInvalidException>(() =>
            DomainPolicies.LinkUrlPolicy.Adjust("bad_scheme:value"));
    }

    [Trait("Category", "Links")]
    [Fact]
    public void AdjustRejectsMalformedUnicode()
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() =>
            DomainPolicies.LinkUrlPolicy.Adjust("\uD800"));
    }

    [Trait("Category", "Links")]
    [Fact]
    public void AdjustRejectsUrlsThatExceedTheMaximumLength()
    {
        string url = "https://" + new string('a', 2048);
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() =>
            DomainPolicies.LinkUrlPolicy.Adjust(url));
    }

    [Trait("Category", "Links")]
    [Fact]
    public void AdjustAcceptsUrlAtMaximumLength()
    {
        string prefix = "https://example.com/";
        string url = prefix + new string('a', 2048 - prefix.Length);

        Assert.Equal(2048, url.Length);
        Assert.Equal(url, DomainPolicies.LinkUrlPolicy.Adjust(url));
    }

    [Trait("Category", "Links")]
    [Fact]
    public void AdjustTrimsWhitespaceAroundAbsoluteUrl()
    {
        Assert.Equal(
            "https://example.com/path?x=1",
            DomainPolicies.LinkUrlPolicy.Adjust("  https://example.com/path?x=1  "));
    }

    #endregion
}
