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
}
