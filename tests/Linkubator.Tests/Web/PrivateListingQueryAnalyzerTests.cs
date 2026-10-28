using Linkubator.Web.Models.PrivateListing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Linkubator.Tests.Web;

public sealed class PrivateListingQueryAnalyzerTests
{
    [Fact]
    public void Analyze_WhenPageIsMissingOrEmpty_UsesDefaultPage()
    {
        // Arrange
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues(string.Empty),
            ["q"] = new StringValues("   ")
        });

        // Act
        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        // Assert
        Assert.Equal(1, result.PageNumber);
        Assert.Null(result.SearchText);
        Assert.False(result.HasAnyError);
    }

    [Fact]
    public void Analyze_WhenParametersAreRepeated_UsesFirstValueAndIgnoresUnknownKeys()
    {
        // Arrange
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues(["003", "2"]),
            ["c"] = new StringValues(["42", "99"]),
            ["q"] = new StringValues(["primera", "segunda"]),
            ["desconocido"] = new StringValues("no válido")
        });

        // Act
        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(
            query,
            PrivateListingKind.Links,
            isAccessibleCollection: collectionId => collectionId == 42);

        // Assert
        Assert.Equal(3, result.PageNumber);
        Assert.Equal(42, result.CollectionId);
        Assert.Equal("primera", result.SearchText);
        Assert.False(result.HasAnyError);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("+2")]
    [InlineData(" 2 ")]
    [InlineData("٢")]
    [InlineData("2147483648")]
    public void Analyze_WhenPageIsInvalid_ReportsErrorAndKeepsValidValues(string pageValue)
    {
        // Arrange
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues(pageValue),
            ["q"] = new StringValues(" válidos ")
        });

        // Act
        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        // Assert
        Assert.True(result.HasPageError);
        Assert.Equal("Los valores no son correctos.", result.PageErrorMessage);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal("válidos", result.SearchText);
    }

    [Fact]
    public void Analyze_WhenCollectionIsMalformed_ReportsErrorButPreservesPageAndSearch()
    {
        // Arrange
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues("2"),
            ["c"] = new StringValues(" 12 "),
            ["q"] = new StringValues("perro")
        });

        // Act
        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        // Assert
        Assert.True(result.HasCollectionError);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal("perro", result.SearchText);
        Assert.Null(result.CollectionId);
    }

    [Fact]
    public void Analyze_WhenCollectionIsEmptyOrNotAccessible_IgnoresItAndPreservesOtherValues()
    {
        // Arrange
        IQueryCollection emptyQuery = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["c"] = new StringValues(string.Empty),
            ["q"] = new StringValues("blanco")
        });
        IQueryCollection inaccessibleQuery = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["c"] = new StringValues("99"),
            ["q"] = new StringValues("otro")
        });

        // Act
        PrivateListingQueryAnalysisResult emptyResult = PrivateListingQueryAnalyzer.Analyze(
            emptyQuery, PrivateListingKind.Links, isAccessibleCollection: static _ => true);
        PrivateListingQueryAnalysisResult inaccessibleResult = PrivateListingQueryAnalyzer.Analyze(
            inaccessibleQuery, PrivateListingKind.Links, isAccessibleCollection: static _ => false);

        // Assert
        Assert.Null(emptyResult.CollectionId);
        Assert.False(emptyResult.HasCollectionError);
        Assert.Equal("blanco", emptyResult.SearchText);
        Assert.Null(inaccessibleResult.CollectionId);
        Assert.False(inaccessibleResult.HasCollectionError);
        Assert.Equal("otro", inaccessibleResult.SearchText);
    }

    [Fact]
    public void Analyze_WhenCollectionIsValidButNoAccessCheckIsProvided_IgnoresCollection()
    {
        // Arrange
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["c"] = new StringValues("42")
        });

        // Act
        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        // Assert
        Assert.Null(result.CollectionId);
        Assert.False(result.HasCollectionError);
    }

    [Theory]
    [InlineData("e", "links")]
    [InlineData("c", "collections")]
    public void Analyze_WhenParameterIsNotApplicableToListing_IgnoresIt(string parameterName, string listing)
    {
        // Arrange
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            [parameterName] = new StringValues("not-valid")
        });
        PrivateListingKind kind = listing == "links" ? PrivateListingKind.Links : PrivateListingKind.Collections;

        // Act
        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, kind);

        // Assert
        Assert.False(result.HasAnyError);
        Assert.Null(result.CollectionId);
        Assert.Null(result.SearchText);
    }

    [Fact]
    public void Analyze_WhenSearchExceedsUnicodeCodePointLimit_ReportsErrorWithoutSearchValue()
    {
        // Arrange
        string tooLong = string.Concat(Enumerable.Repeat("😀", PrivateListingQueryAnalyzer.MaxSearchTextLength + 1));
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["q"] = new StringValues(tooLong),
            ["p"] = new StringValues("2")
        });

        // Act
        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Collections);

        // Assert
        Assert.True(result.HasSearchError);
        Assert.Equal(2, result.PageNumber);
        Assert.Null(result.SearchText);
    }

    [Fact]
    public void Analyze_WhenSearchHasText_TrimsExtremesAndPreservesInternalSpaces()
    {
        // Arrange
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["q"] = new StringValues("  dos términos  ")
        });

        // Act
        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Collections);

        // Assert
        Assert.Equal("dos términos", result.SearchText);
        Assert.False(result.HasSearchError);
    }
}
