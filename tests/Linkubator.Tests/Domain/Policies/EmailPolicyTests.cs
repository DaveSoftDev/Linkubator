using System.Text.RegularExpressions;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "Users")]
[Trait("Category", "Email")]
public class EmailPolicyTests
{
    [Theory]
    [InlineData(" Ana+Tag@Example.com ", "ana+tag@example.com")]
    [InlineData("user.name@sub.example.io", "user.name@sub.example.io")]
    [InlineData("USER@EXAMPLE.COM", "user@example.com")]
    [InlineData("\"quoted.local\"@Example.com", "\"quoted.local\"@example.com")]
    [InlineData("user@[192.0.2.1]", "user@[192.0.2.1]")]
    public void Normalize_WhenValid_ReturnsTrimmedLowercase(string input, string expected)
    {
        Assert.Equal(expected, DomainPolicies.EmailPolicy.Normalize(input));
    }

    [Theory]
    [InlineData("user")]
    [InlineData("user@")]
    [InlineData("user..name@example.com")]
    [InlineData("user@example..com")]
    [InlineData("user @example.com")]
    [InlineData("user(comment)@example.com")]
    public void Normalize_WhenSyntaxIsInvalid_ThrowsInvalidEmailException(string email)
    {
        Assert.Throws<DomainExceptions.InvalidEmailException>(() => DomainPolicies.EmailPolicy.Normalize(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Normalize_WhenMissingOrWhitespace_ThrowsNotValidInputException(string? email)
    {
        Assert.Throws<DomainExceptions.NotValidInputException>(() => DomainPolicies.EmailPolicy.Normalize(email));
    }

    [Fact]
    public void Normalize_WhenAtMaximumLength_AcceptsEmail()
    {
        string email = new string('a', 238) + "@example.com";

        Assert.Equal(250, email.Length);
        Assert.Equal(email, DomainPolicies.EmailPolicy.Normalize(email));
    }

    [Fact]
    public void Normalize_WhenOverMaximumLength_ThrowsTextExceedsMaximumLengthException()
    {
        string email = new string('a', 239) + "@example.com";

        Assert.Equal(251, email.Length);
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() =>
            DomainPolicies.EmailPolicy.Normalize(email));
    }

    /// <summary>
    /// El patrón debe replicar literalmente la expresión de specifications.md → «Email»,
    /// anclada con \A y \z. Se prueba sobre el patrón y no sobre Normalize porque el recorte
    /// previo elimina el salto de línea final, que es la única diferencia que el anclaje aprecia.
    /// </summary>
    [Fact]
    public void EmailPatternIsAnchoredAsTheSpecificationWritesIt()
    {
        Regex pattern = InvokeEmailPattern();

        Assert.StartsWith(@"\A", pattern.ToString(), StringComparison.Ordinal);
        Assert.EndsWith(@"\z", pattern.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Con \z final, una dirección seguida de un salto de línea no casa por la posición
    /// anterior a ese salto, que es lo que permitiría el anclaje $.
    /// </summary>
    [Fact]
    public void EmailPatternRejectsATrailingLineBreakInsteadOfMatchingBeforeIt()
    {
        Regex pattern = InvokeEmailPattern();

        Assert.DoesNotMatch(pattern, "ana@example.com\n");
        Assert.Matches(pattern, "ana@example.com");
    }

    private static Regex InvokeEmailPattern()
    {
        return (Regex)PrivatePolicyInvoker.InvokeStatic(typeof(DomainPolicies.EmailPolicy), "EmailPattern")!;
    }
}
