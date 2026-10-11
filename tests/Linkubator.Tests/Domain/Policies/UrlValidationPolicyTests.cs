using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class UrlValidationPolicyTests
{
    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://example.com/a")]
    [InlineData("https://Example.com/a")]
    [InlineData("http://localhost:5000/admin")]
    [InlineData("http://192.168.1.1")]
    [InlineData("http://[2001:db8::1]/")]
    [InlineData("https://xn--bcher-kva.example/")]
    public void ValidateAcceptsAllowedUrls(string input)
    {
        var exception = Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(input));

        Assert.Null(exception);
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("intranet")]
    [InlineData("https://intranet")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://ana:clave@example.com")]
    [InlineData("https://example.com:0")]
    [InlineData("https://example.com:70000")]
    public void ValidateRejectsInvalidUrls(string input)
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));
    }

    [Trait("Category", "Validation")]
    [Fact]
    public void ValidHostAcceptsDnsAndIpFormats()
    {
        Assert.True(DomainPolicies.UrlHostPolicy.IsValidHost("localhost"));
        Assert.True(DomainPolicies.UrlHostPolicy.IsValidHost("example.com"));
        Assert.True(DomainPolicies.UrlHostPolicy.IsValidHost("192.168.1.1"));
        Assert.True(DomainPolicies.UrlHostPolicy.IsValidHost("[2001:db8::1]"));
        Assert.True(DomainPolicies.UrlHostPolicy.IsValidHost("xn--bcher-kva.example"));
        Assert.False(DomainPolicies.UrlHostPolicy.IsValidHost("intranet"));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://usuario:clave@example.com/path")]
    [InlineData("https://example.com:0")]
    [InlineData("https://example.com:70000")]
    [InlineData("https://example.com:abc")]
    public void ValidateRejectsAuthorityProblemsWithoutIncludingTheInputText(string input)
    {
        var exception = Assert.ThrowsAny<DomainExceptions.DomainException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));

        Assert.DoesNotContain(input, exception.Message, StringComparison.Ordinal);
    }

    [Trait("Category", "Validation")]
    [Fact]
    public void ValidateDoesNotTransformTheAcceptedUrl()
    {
        string input = "https://EXAMPLE.com:8443/a/b?c=1";
        string actual = input;

        var exception = Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(actual));

        Assert.Null(exception);
        Assert.Equal(input, actual);
    }
}
