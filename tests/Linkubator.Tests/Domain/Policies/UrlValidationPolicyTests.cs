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

    [Trait("Category", "Validation")]
    [Fact]
    public void ValidateRejectsNullAsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(() => DomainPolicies.UrlValidationPolicy.Validate(null!));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ValidateRejectsEmptyOrBlankAsArgumentWithoutTrimming(string input)
    {
        Assert.Throws<ArgumentException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("intranet")]
    [InlineData("example.com/articulo")]
    [InlineData("/ruta/relativa")]
    [InlineData("https://exa mple.com")]
    [InlineData("https://example.com:70000")]
    [InlineData("https://example.com:abc")]
    public void ValidateRejectsNonAbsoluteUris(string input)
    {
        Assert.Throws<DomainExceptions.UrlNotAbsoluteException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:ana@example.com")]
    [InlineData("file:///c:/datos.txt")]
    public void ValidateRejectsSchemesOtherThanHttpAndHttps(string input)
    {
        Assert.Throws<DomainExceptions.UrlSchemeNotAllowedException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("HTTP://example.com")]
    [InlineData("HTTPS://example.com")]
    [InlineData("hTtPs://example.com")]
    public void ValidateAcceptsHttpAndHttpsInAnyCase(string input)
    {
        Assert.Null(Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(input)));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://intranet")]
    [InlineData("https://example.c")]
    [InlineData("https://example.c0m")]
    [InlineData("https://example.123")]
    [InlineData("http://127.1")]
    public void ValidateRejectsHostsWithoutAnAllowedFormat(string input)
    {
        Assert.Throws<DomainExceptions.UrlHostInvalidException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://localhost")]
    [InlineData("http://127.0.0.1")]
    [InlineData("http://10.0.0.5:8080/x")]
    [InlineData("http://[::1]/")]
    [InlineData("https://example.co")]
    [InlineData("https://sub.example.com")]
    [InlineData("https://example.xn--p1ai")]
    [InlineData("https://xn--bcher-kva.example")]
    [InlineData("https://bücher.example")]
    [InlineData("https://例え.jp")]
    public void ValidateAcceptsAllowedHostFormats(string input)
    {
        Assert.Null(Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(input)));
    }

    [Trait("Category", "Validation")]
    [Fact]
    public void ValidateGivesTheSameResultForAUnicodeDomainAndItsPunycode()
    {
        string unicode = "https://bücher.example/a";
        string punycode = "https://xn--bcher-kva.example/a";

        Assert.Null(Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(unicode)));
        Assert.Null(Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(punycode)));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://usuario:clave@example.com/path")]
    [InlineData("https://usuario@example.com")]
    [InlineData("http://ana:@example.com")]
    public void ValidateRejectsCredentials(string input)
    {
        Assert.Throws<DomainExceptions.UrlCredentialsNotAllowedException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://example.com:1")]
    [InlineData("https://example.com:443/x")]
    [InlineData("https://example.com:8443/x")]
    [InlineData("https://example.com:65535")]
    [InlineData("http://[2001:db8::1]:8080/")]
    public void ValidateAcceptsMissingOrValidPorts(string input)
    {
        Assert.Null(Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(input)));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://example.com:0")]
    [InlineData("https://example.com:0/x")]
    [InlineData("http://[2001:db8::1]:0/")]
    public void ValidateRejectsAPortOutsideTheValidRange(string input)
    {
        Assert.Throws<DomainExceptions.UrlPortInvalidException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("ftp://intranet", typeof(DomainExceptions.UrlSchemeNotAllowedException))]
    [InlineData("ftp://ana:clave@example.com:0", typeof(DomainExceptions.UrlSchemeNotAllowedException))]
    [InlineData("https://intranet:0", typeof(DomainExceptions.UrlHostInvalidException))]
    [InlineData("https://ana:clave@intranet", typeof(DomainExceptions.UrlHostInvalidException))]
    [InlineData("https://ana:clave@example.com:0", typeof(DomainExceptions.UrlCredentialsNotAllowedException))]
    public void ValidateAppliesTheChecksInTheDocumentedOrder(string input, Type expected)
    {
        Exception exception = Assert.ThrowsAny<Exception>(() => DomainPolicies.UrlValidationPolicy.Validate(input));

        Assert.IsType(expected, exception);
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://intranet")]
    [InlineData("ftp://example.com")]
    [InlineData("https://ana:clave@example.com")]
    [InlineData("https://example.com:0")]
    [InlineData("intranet")]
    public void ValidateRejectionsAreDomainExceptionsWithoutTheInputText(string input)
    {
        DomainExceptions.DomainException exception = Assert.ThrowsAny<DomainExceptions.DomainException>(() => DomainPolicies.UrlValidationPolicy.Validate(input));

        Assert.DoesNotContain(input, exception.Message, StringComparison.Ordinal);
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://example.com/a")]
    [InlineData("https://intranet")]
    [InlineData("ftp://example.com")]
    public void ValidateGivesTheSameResultEachTimeForTheSameUrl(string input)
    {
        Exception? first = Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(input));
        Exception? second = Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(input));

        Assert.Equal(first?.GetType(), second?.GetType());
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("example.com/articulo", "https://example.com/articulo")]
    [InlineData(" https://Example.com/a ", "https://Example.com/a")]
    [InlineData("localhost:5000/admin", "http://localhost:5000/admin")]
    [InlineData("example.com:8080/x", "https://example.com:8080/x")]
    [InlineData("http://192.168.1.1", "http://192.168.1.1")]
    [InlineData("http://[2001:db8::1]/", "http://[2001:db8::1]/")]
    public void SpecificationExamplesAreTrimmedAdjustedAndAccepted(string input, string expected)
    {
        string adjusted = DomainPolicies.LinkUrlPolicy.Adjust(DomainPolicies.UserTextPolicy.TrimToNull(input)!);

        Assert.Equal(expected, adjusted);
        Assert.Null(Record.Exception(() => DomainPolicies.UrlValidationPolicy.Validate(adjusted)));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("mailto:ana@example.com")]
    [InlineData("ftp://example.com")]
    public void SpecificationExamplesWithAForbiddenSchemeAreRejectedWhenAdjusting(string input)
    {
        Assert.Throws<DomainExceptions.UrlSchemeNotAllowedException>(() => DomainPolicies.LinkUrlPolicy.Adjust(input));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://intranet", typeof(DomainExceptions.UrlAuthorityMissingException))]
    [InlineData("intranet", typeof(DomainExceptions.UrlHostInvalidException))]
    [InlineData("https://ana:clave@example.com", typeof(DomainExceptions.UrlCredentialsNotAllowedException))]
    public void SpecificationExamplesAreRejectedByAdjustmentOrValidation(string input, Type expected)
    {
        Exception exception = Assert.ThrowsAny<Exception>(() =>
            DomainPolicies.UrlValidationPolicy.Validate(DomainPolicies.LinkUrlPolicy.Adjust(input)));

        Assert.IsType(expected, exception);
    }
}
