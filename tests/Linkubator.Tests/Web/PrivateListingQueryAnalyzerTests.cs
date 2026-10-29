using Linkubator.Web.Models.PrivateListing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Linkubator.Tests.Web;

public sealed class PrivateListingQueryAnalyzerTests
{
    [Fact]
    public void Analyze_WhenPageIsMissingOrEmpty_UsesDefaultPage()
    {
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues(string.Empty),
            ["q"] = new StringValues("   ")
        });

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        Assert.Equal(1, result.PageNumber);
        Assert.Null(result.SearchText);
        Assert.False(result.HasAnyError);
    }

    [Fact]
    public void Analyze_WhenParametersAreRepeated_UsesFirstValueAndIgnoresUnknownKeys()
    {
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues(["003", "2"]),
            ["c"] = new StringValues(["42", "99"]),
            ["q"] = new StringValues(["primera", "segunda"]),
            ["desconocido"] = new StringValues("no válido")
        });

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(
            query,
            PrivateListingKind.Links,
            isAccessibleCollection: collectionId => collectionId == 42);

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
    [InlineData("abc")]
    public void Analyze_WhenPageIsInvalid_UsesFirstPageAndKeepsValidValues(string pageValue)
    {
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues(pageValue),
            ["q"] = new StringValues(" válidos ")
        });

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        Assert.False(result.HasAnyError);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal("válidos", result.SearchText);
    }

    [Theory]
    [InlineData("2147483648", int.MaxValue)]
    [InlineData("999999999999999999999999999999", int.MaxValue)]
    [InlineData("000000000000000000000000000002", 2)]
    public void Analyze_WhenPageContainsOnlyDigits_PreservesOrBoundsItsValue(string pageValue, int expected)
    {
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues(pageValue),
        });

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        Assert.Equal(expected, result.PageNumber);
        Assert.False(result.HasAnyError);
    }

    [Fact]
    public void Analyze_WhenCollectionIsMalformed_ReportsErrorButPreservesPageAndSearch()
    {
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["p"] = new StringValues("2"),
            ["c"] = new StringValues(" 12 "),
            ["q"] = new StringValues("perro")
        });

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        Assert.True(result.HasCollectionError);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal("perro", result.SearchText);
        Assert.Null(result.CollectionId);
    }

    [Fact]
    public void Analyze_WhenCollectionIsEmptyOrNotAccessible_IgnoresItAndPreservesOtherValues()
    {
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

        PrivateListingQueryAnalysisResult emptyResult = PrivateListingQueryAnalyzer.Analyze(
            emptyQuery, PrivateListingKind.Links, isAccessibleCollection: static _ => true);
        PrivateListingQueryAnalysisResult inaccessibleResult = PrivateListingQueryAnalyzer.Analyze(
            inaccessibleQuery, PrivateListingKind.Links, isAccessibleCollection: static _ => false);

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
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["c"] = new StringValues("42")
        });

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Links);

        Assert.Null(result.CollectionId);
        Assert.False(result.HasCollectionError);
    }

    [Theory]
    [InlineData("e", "links")]
    [InlineData("c", "collections")]
    public void Analyze_WhenParameterIsNotApplicableToListing_IgnoresIt(string parameterName, string listing)
    {
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            [parameterName] = new StringValues("not-valid")
        });
        PrivateListingKind kind = listing == "links" ? PrivateListingKind.Links : PrivateListingKind.Collections;

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, kind);

        Assert.False(result.HasAnyError);
        Assert.Null(result.CollectionId);
        Assert.Null(result.SearchText);
    }

    [Fact]
    public void Analyze_WhenSearchExceedsUnicodeCodePointLimit_ReportsErrorWithoutSearchValue()
    {
        string tooLong = string.Concat(Enumerable.Repeat("😀", PrivateListingQueryAnalyzer.MaxSearchTextLength + 1));
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["q"] = new StringValues(tooLong),
            ["p"] = new StringValues("2")
        });

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Collections);

        Assert.True(result.HasSearchError);
        Assert.Equal(2, result.PageNumber);
        Assert.Null(result.SearchText);
    }

    [Fact]
    public void Analyze_WhenSearchHasText_TrimsExtremesAndPreservesInternalSpaces()
    {
        IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["q"] = new StringValues("  dos términos  ")
        });

        PrivateListingQueryAnalysisResult result = PrivateListingQueryAnalyzer.Analyze(query, PrivateListingKind.Collections);

        Assert.Equal("dos términos", result.SearchText);
        Assert.False(result.HasSearchError);
    }
}
