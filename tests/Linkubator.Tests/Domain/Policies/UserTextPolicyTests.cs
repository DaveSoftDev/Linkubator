using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "Users")]
[Trait("Category", "Text")]
public class UserTextPolicyTests
{
    [Theory]
    [InlineData("  ab  ", 2, "ab")]
    [InlineData("  \U0001F600  ", 1, "\U0001F600")]
    [InlineData("a\u0301", 2, "a\u0301")]
    public void EnsureRequiredText_WithBoundaryInput_TrimsAndCountsCodePoints(string value, int maximum, string expected)
    {
        Assert.Equal(expected, DomainPolicies.UserTextPolicy.EnsureRequiredText(value, maximum));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureRequiredText_WithMissingValue_Rejects(string? value)
    {
        Assert.Throws<Linkubator.Domain.Exceptions.NotValidInputException>(() => DomainPolicies.UserTextPolicy.EnsureRequiredText(value, 2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureOptionalMaximumLength_WithMissingValue_ReturnsNull(string? value)
    {
        Assert.Null(DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(value, 2));
    }

    [Theory]
    [InlineData("  ab  ", 2, "ab")]
    [InlineData("  \U0001F600  ", 1, "\U0001F600")]
    [InlineData("a\u0301", 2, "a\u0301")]
    public void EnsureOptionalMaximumLength_WithBoundaryInput_ReturnsExactValue(string value, int maximum, string expected)
    {
        Assert.Equal(expected, DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(value, maximum));
    }

    [Theory]
    [InlineData("abc", 2)]
    [InlineData("\U0001F600x", 1)]
    [InlineData("a\u0301", 1)]
    public void EnsureMaximumLengths_WithExcess_RejectsWithoutTruncating(string value, int maximum)
    {
        Assert.Throws<Linkubator.Domain.Exceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.UserTextPolicy.EnsureRequiredText(value, maximum));
        Assert.Throws<Linkubator.Domain.Exceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(value, maximum));
    }

    [Theory]
    [InlineData("a", 1, 2)]
    [InlineData("ab", 1, 2)]
    [InlineData("\U0001F600", 1, 1)]
    public void EnsureWithinRange_AtBoundaries_Accepts(string value, int minimum, int maximum)
    {
        DomainPolicies.UserTextPolicy.EnsureWithinRange(value, minimum, maximum);
    }

    [Fact]
    public void EnsureWithinRange_OutsideBoundaries_RejectsWithSpecificCause()
    {
        Assert.Throws<Linkubator.Domain.Exceptions.TextBelowMinimumLengthException>(() => DomainPolicies.UserTextPolicy.EnsureWithinRange("", 1, 2));
        Assert.Throws<Linkubator.Domain.Exceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.UserTextPolicy.EnsureWithinRange("abc", 1, 2));
    }

    [Theory]
    [InlineData("  texto  ", "texto")]
    [InlineData(" texto interior ", "texto interior")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void TrimToNullTrimsAndMapsEmptyValuesToNull(string? value, string? expected)
    {
        Assert.Equal(expected, DomainPolicies.UserTextPolicy.TrimToNull(value));
    }

    [Theory]
    [InlineData("abc", 3)]
    [InlineData("\uD83D\uDE00", 1)]
    [InlineData("a\u0301", 2)]
    public void CountCodePointsCountsUnicodeCodePoints(string value, int expected)
    {
        Assert.Equal(expected, DomainPolicies.UserTextPolicy.CountCodePoints(value));
    }

    [Theory]
    [InlineData("ab", 2, true)]
    [InlineData("abc", 2, false)]
    [InlineData("x", -1, false)]
    [InlineData("x\uD83D\uDE00", 2, true)]
    [InlineData("😀", 1, true)]
    [InlineData("😀", 0, false)]
    public void IsWithinMaximumLengthComparesCodePointCount(string value, int maximum, bool expected)
    {
        Assert.Equal(expected, DomainPolicies.UserTextPolicy.IsWithinMaximumLength(value, maximum));
    }
}