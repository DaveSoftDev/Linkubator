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
        // Arrange
        string original = input;

        // Act
        string? result = SearchTextNormalizer.Normalize(input);

        // Assert
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
        // Arrange
        string? result = SearchTextNormalizer.Normalize(input);

        // Act
        string? normalizedAgain = SearchTextNormalizer.Normalize(result);

        // Assert
        Assert.Equal(input, result);
        Assert.Equal(result, normalizedAgain);
        Assert.True(result!.IsNormalized(NormalizationForm.FormC));
    }

    [Fact]
    public void Normalize_WhenTextIsNull_ReturnsNull()
    {
        // Act
        string? result = SearchTextNormalizer.Normalize(null);

        // Assert
        Assert.Null(result);
    }
}
