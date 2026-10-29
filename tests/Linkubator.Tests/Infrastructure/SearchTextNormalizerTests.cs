using Linkubator.Infrastructure.Search;
using System.Text;

namespace Linkubator.Tests.Infrastructure;

public sealed class SearchTextNormalizerTests
{
    [Theory]
    [InlineData("cancio\u0301n", "canción")]
    [InlineData("coleccio\u0301n y árbol", "colección y árbol")]
    public void Normalize_WhenTextIsDecomposed_ReturnsUnicodeFormC(string input, string expected)
    {
        string original = input;

        string? result = SearchTextNormalizer.Normalize(input);

        Assert.Equal(expected, result);
        Assert.True(result!.IsNormalized(NormalizationForm.FormC));
        Assert.Equal(original, input);
    }

    [Theory]
    [InlineData("canción")]
    [InlineData("Plain ASCII 2026")]
    [InlineData("")]
    public void Normalize_WhenTextIsAlreadyFormC_ReturnsEquivalentText(string input)
    {
        string? result = SearchTextNormalizer.Normalize(input);

        string? normalizedAgain = SearchTextNormalizer.Normalize(result);

        Assert.Equal(input, result);
        Assert.Equal(result, normalizedAgain);
        Assert.True(result!.IsNormalized(NormalizationForm.FormC));
    }

    [Fact]
    public void Normalize_WhenTextIsNull_ReturnsNull()
    {
        string? result = SearchTextNormalizer.Normalize(null);

        Assert.Null(result);
    }
}
