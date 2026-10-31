using Linkubator.Infrastructure.Search;

namespace Linkubator.Tests.Infrastructure;

public sealed class SearchQueryBuilderTests
{
    [Theory]
    [InlineData("canción árbol", "\"canción\"* \"árbol\"*")]
    [InlineData("uno-dos* tres", "\"uno\"* \"dos\"* \"tres\"*")]
    [InlineData("AND OR NOT NEAR", "\"AND\"* \"OR\"* \"NOT\"* \"NEAR\"*")]
    [InlineData("café 2026", "\"café\"* \"2026\"*")]
    [InlineData("árbol", "\"árbol\"*")]
    [InlineData("東京 𐐀", "\"東京\"* \"𐐀\"*")]
    [InlineData("Ⅷ ²", "\"Ⅷ\"* \"²\"*")]
    [InlineData("q\u0301", "\"q\"*")]
    public void Build_WhenTextContainsTerms_ReturnsQuotedPrefixTermsCombinedImplicitly(string text, string expected)
    {
        string original = text;

        string? result = SearchQueryBuilder.Build(text);

        Assert.Equal(expected, result);
        Assert.Equal(original, text);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("- * \" +")]
    public void Build_WhenTextContainsNoTerms_ReturnsNull(string text)
    {
        string? result = SearchQueryBuilder.Build(text);

        Assert.Null(result);
    }

    [Fact]
    public void Build_WhenTextIsNull_ReturnsNull()
    {
        string? result = SearchQueryBuilder.Build(null);

        Assert.Null(result);
    }
}
