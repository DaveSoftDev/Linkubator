using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class UserTextPolicyTests
{
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