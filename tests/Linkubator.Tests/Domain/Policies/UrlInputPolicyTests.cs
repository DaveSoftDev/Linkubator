using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "URLs")]
public class UrlInputPolicyTests
{
    [Trait("Category", "Input")]
    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void PrepareRejectsNullOrWhiteSpace(string? input)
    {
        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            DomainPolicies.UrlInputPolicy.Prepare(input!));
    }

    [Trait("Category", "Input")]
    [Theory]
    [InlineData("example.com/articulo 2")]
    [InlineData("example.com/articulo\u00A02")]
    [InlineData("example.com/a\u0007b")]
    public void PrepareRejectsUnsupportedCharacters(string input)
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() =>
            DomainPolicies.UrlInputPolicy.Prepare(input));
    }

    [Trait("Category", "Input")]
    [Fact]
    public void PrepareRejectsInvalidUnicode()
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() =>
            DomainPolicies.UrlInputPolicy.Prepare("\uD800"));
    }

    [Trait("Category", "Input")]
    [Fact]
    public void PrepareTrimsTheExtremes()
    {
        Assert.Equal(
            "https://example.com/path",
            DomainPolicies.UrlInputPolicy.Prepare("  https://example.com/path  "));
    }

    [Trait("Category", "Input")]
    [Fact]
    public void PrepareChecksTheMaximumLengthWhenIndicated()
    {
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() =>
            DomainPolicies.UrlInputPolicy.Prepare(new string('a', 11), 10));
    }

    [Trait("Category", "Input")]
    [Fact]
    public void PrepareSkipsTheMaximumLengthWhenNotIndicated()
    {
        string input = new string('a', 2049);

        Assert.Equal(input, DomainPolicies.UrlInputPolicy.Prepare(input));
    }
}
