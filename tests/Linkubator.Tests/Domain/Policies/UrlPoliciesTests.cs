using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

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
    [InlineData("https://example.com/path", true, true, "https", "example.com")]
    [InlineData("mailto:ana@example.com", false, true, "mailto", null)]
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
    [InlineData("localhost:5000/path", "https://localhost:5000/path")]
    [InlineData("example.com:8080/x", "https://example.com:8080/x")]
    [InlineData("https://Example.com/a", "https://Example.com/a")]
    public void AdjustReturnsTheAdjustedUrl(string input, string expected)
    {
        Assert.Equal(expected, DomainPolicies.LinkUrlPolicy.Adjust(input));
    }

    [Trait("Category", "Links")]
    [Fact]
    public void AdjustRejectsNullOrWhitespaceInput()
    {
        Assert.Throws<ArgumentNullException>(() => DomainPolicies.LinkUrlPolicy.Adjust(null!));
        Assert.Throws<ArgumentException>(() => DomainPolicies.LinkUrlPolicy.Adjust("   "));
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
    public void AdjustRejectsUrlsThatExceedTheMaximumLength()
    {
        string url = "https://" + new string('a', 2048);
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.LinkUrlPolicy.Adjust(url));
    }

    #endregion
}
