using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "URLs")]
[Trait("Category", "Hosts")]
public class UriAuthorityPolicyTests
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
    [InlineData("example_com.org", false)]
    [InlineData("example.xn--p1ai", true)]
    [InlineData("127.1", false)]
    [InlineData("10.20", false)]
    [InlineData("2130706433", false)]
    [InlineData("256.0.0.1", false)]
    [InlineData("", false)]
    public void IsValidHostRecognizesAllowedAndRejectedHosts(string input, bool expected)
    {
        Assert.Equal(expected, DomainPolicies.UriAuthorityPolicy.IsValidHost(input));
    }

    [Trait("Category", "Hosts")]
    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("example.com:8080", true)]
    [InlineData("http://[::1]:8080/", true)]
    [InlineData("https://intranet:8080", false)]
    [InlineData("http://:8080", false)]
    [InlineData("https://example.com:abc", false)]
    [InlineData("https://example.com:0", false)]
    public void IsValidAuthorityRequiresAValidHostAndAPortWithinRange(string input, bool expected)
    {
        DomainPolicies.UriAuthority? authority = DomainPolicies.UriAuthorityPolicy.Analyze(input);

        Assert.Equal(expected, DomainPolicies.UriAuthorityPolicy.IsValidAuthority(authority));
    }

    [Trait("Category", "Hosts")]
    [Theory]
    [InlineData("1", true)]
    [InlineData("80", true)]
    [InlineData("080", true)]
    [InlineData("65535", true)]
    [InlineData("0", false)]
    [InlineData("65536", false)]
    [InlineData("999999999999999999999", false)]
    [InlineData("", false)]
    [InlineData("+80", false)]
    [InlineData(" 80", false)]
    [InlineData("80 ", false)]
    [InlineData("abc", false)]
    public void IsValidPortAcceptsOnlyDigitsWithinRange(string port, bool expected)
    {
        Assert.Equal(expected, DomainPolicies.UriAuthorityPolicy.IsValidPort(port));
    }

    [Trait("Category", "Hosts")]
    [Fact]
    public void IsValidPortTreatsAnAbsentPortAsValid()
    {
        Assert.True(DomainPolicies.UriAuthorityPolicy.IsValidPort(null));
    }

    [Trait("Category", "Hosts")]
    [Theory]
    [InlineData("[2001:db8::1]:8080/path", true, "[2001:db8::1]")]
    [InlineData("[2001:db8::1]:abc/path", false, null)]
    [InlineData("[2001:db8::1", false, null)]
    [InlineData("[2001:db8::1]suffix", false, null)]
    [InlineData("example.com:65536", false, null)]
    [InlineData("example.com:0", false, null)]
    [InlineData("example.com:", false, null)]
    [InlineData("example.com:999999999999999999999", false, null)]
    [InlineData(":8080", false, null)]
    [InlineData("", false, null)]
    public void TryExtractHostRejectsMalformedAuthoritiesAndClearsOutput(
        string input,
        bool expectedSuccess,
        string? expectedHost)
    {
        bool success = DomainPolicies.UriAuthorityPolicy.TryExtractHost(input, out string? extractedHost);

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedHost, extractedHost);
    }

    [Trait("Category", "Hosts")]
    [Fact]
    public void AnalyzeReturnsNullForAnEmptyProtocolRelativeReference()
    {
        Assert.Null(DomainPolicies.UriAuthorityPolicy.Analyze("//"));
    }

    [Trait("Category", "Hosts")]
    [Fact]
    public void AnalyzeDoesNotTreatSchemeWithIpAndPortAsHostAndPortForm()
    {
        DomainPolicies.UriAuthority? authority = DomainPolicies.UriAuthorityPolicy.Analyze("https:127.0.0.1:8080");

        Assert.NotNull(authority);
        Assert.False(authority.HasExplicitScheme);
        Assert.Equal("8080", authority.Port);
    }

    [Trait("Category", "Hosts")]
    [Fact]
    public void AnalyzeKeepsTheSchemeWithoutHostWhenCredentialsEndTheAuthority()
    {
        DomainPolicies.UriAuthority? authority = DomainPolicies.UriAuthorityPolicy.Analyze("https://user@");

        Assert.NotNull(authority);
        Assert.True(authority.HasExplicitScheme);
        Assert.Null(authority.Host);
    }

    /// <summary>
    /// Fija que los colaboradores internos del análisis de autoridad no se hagan públicos:
    /// el único punto de entrada sigue siendo <see cref="DomainPolicies.UriAuthorityPolicy"/>.
    /// Los tipos se localizan por nombre porque son internos al ensamblado de Domain.
    /// </summary>
    [Trait("Category", "Hosts")]
    [Fact]
    public void AuthorityCollaboratorsAreNotVisibleOutsideDomain()
    {
        string[] collaboratorNames = ["UriAuthoritySyntax", "UriHostFormat", "UriPortFormat"];

        Type[] collaborators = typeof(DomainPolicies.UriAuthorityPolicy).Assembly.GetTypes()
            .Where(type => collaboratorNames.Contains(type.Name))
            .ToArray();

        Assert.Equal(collaboratorNames.Length, collaborators.Length);
        Assert.All(collaborators, collaborator => Assert.False(collaborator.IsVisible));
    }
}
