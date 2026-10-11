using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class UrlHostPolicyTests
{
    [Trait("Category", "Hosts")]
    [Theory]
    [InlineData("localhost", true)]
    [InlineData("example.com", true)]
    [InlineData("sub.example.com", true)]
    [InlineData("xn--bcher-kva.example", true)]
    [InlineData("münchen.de", true)]
    [InlineData("192.168.1.1", true)]
    [InlineData("[::1]", true)]
    [InlineData("[2001:db8::1]", true)]
    [InlineData("intranet", false)]
    [InlineData("example..com", false)]
    [InlineData("-example.com", false)]
    [InlineData("example-.com", false)]
    [InlineData("example", false)]
    [InlineData("example.c", false)]
    [InlineData("example.c0m", false)]
    [InlineData("example.123", false)]
    [InlineData("example.xn--p1ai", true)]
    [InlineData("127.1", false)]
    [InlineData("10.20", false)]
    [InlineData("2130706433", false)]
    [InlineData("256.0.0.1", false)]
    [InlineData("", false)]
    public void IsValidHostRecognizesAllowedAndRejectedHosts(string input, bool expected)
    {
        Assert.Equal(expected, DomainPolicies.UrlHostPolicy.IsValidHost(input));
    }

    [Trait("Category", "Hosts")]
    [Theory]
    [InlineData("localhost:5000/path", true, "localhost")]
    [InlineData("example.com:8080/x", true, "example.com")]
    [InlineData("https://example.com/path", true, "example.com")]
    [InlineData("http://[::1]/", true, "[::1]")]
    [InlineData("http://[::1]:8080/", true, "[::1]")]
    [InlineData("example.com/r?u=https://x.com", true, "example.com")]
    [InlineData("localhost:5000/r?u=http://x.com", true, "localhost")]
    [InlineData("example.com/a#https://x", true, "example.com")]
    [InlineData("https://intranet:8080", false, null)]
    [InlineData("http://:8080", false, null)]
    [InlineData("https://example.com:abc", false, null)]
    public void TryExtractValidHostReturnsOnlyValidHosts(string input, bool expectedSuccess, string? expectedHost)
    {
        bool success = DomainPolicies.UrlHostPolicy.TryExtractValidHost(input, out string? extractedHost);

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedHost, extractedHost);
    }
}
