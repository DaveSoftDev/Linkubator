using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Linkubator.Web.Models.PrivateListing;

public static class PrivateListingQueryAnalyzer
{
    public const int DefaultPageNumber = 1;
    public const int MaxSearchTextLength = 100;
    public const long MaxIdentifierValue = long.MaxValue;

    public static PrivateListingQueryAnalysisResult Analyze(
        IQueryCollection query,
        PrivateListingKind kind,
        Func<long, bool>? isAccessibleCollection = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        int pageNumber = DefaultPageNumber;
        long? collectionId = null;
        string? searchText = null;
        bool hasPageError = false;
        bool hasCollectionError = false;
        bool hasSearchError = false;
        string? pageErrorMessage = null;
        string? collectionErrorMessage = null;
        string? searchErrorMessage = null;

        bool supportsCollection = kind == PrivateListingKind.Links;
        bool supportsSearch = kind is PrivateListingKind.Links or PrivateListingKind.Collections;
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        string? pageValue = GetFirstValue(query, "p");
        if (!string.IsNullOrEmpty(pageValue))
        {
            if (IsPositiveAsciiInteger(pageValue, int.MaxValue, out long parsedPage))
            {
                pageNumber = (int)parsedPage;
            }
            else
            {
                hasPageError = true;
                pageNumber = DefaultPageNumber;
                pageErrorMessage = "Los valores no son correctos.";
            }
        }

        if (supportsCollection)
        {
            string? collectionValue = GetFirstValue(query, "c");
            if (!string.IsNullOrEmpty(collectionValue))
            {
                if (IsPositiveAsciiInteger(collectionValue, MaxIdentifierValue, out long parsedCollection))
                {
                    if (isAccessibleCollection?.Invoke(parsedCollection) == true)
                    {
                        collectionId = parsedCollection;
                    }
                }
                else
                {
                    hasCollectionError = true;
                    collectionErrorMessage = "Los valores no son correctos.";
                }
            }
        }

        if (supportsSearch)
        {
            string? searchValue = GetFirstValue(query, "q");
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                string trimmedSearch = searchValue.Trim();
                int searchLength = CountCodePoints(trimmedSearch);
                if (searchLength > MaxSearchTextLength)
                {
                    hasSearchError = true;
                    searchErrorMessage = "El texto de búsqueda supera la longitud máxima permitida.";
                }
                else if (trimmedSearch.Any(character => !char.IsWhiteSpace(character)))
                {
                    searchText = trimmedSearch;
                }
            }
        }

        return new PrivateListingQueryAnalysisResult(
            pageNumber,
            collectionId,
            searchText,
            hasPageError,
            hasCollectionError,
            hasSearchError,
            pageErrorMessage,
            collectionErrorMessage,
            searchErrorMessage);
    }

    private static bool IsPositiveAsciiInteger(string value, long maximum, out long parsed)
    {
        parsed = 0;
        if (value.Length == 0)
        {
            return false;
        }

        foreach (char character in value)
        {
            if (character is < '0' or > '9')
            {
                return false;
            }
        }

        return long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out parsed)
            && parsed > 0
            && parsed <= maximum;
    }

    private static int CountCodePoints(string value)
    {
        int count = 0;
        foreach (Rune _ in value.EnumerateRunes())
        {
            count++;
        }

        return count;
    }

    private static string? GetFirstValue(IQueryCollection query, string key)
    {
        if (!query.TryGetValue(key, out StringValues values) || values.Count == 0)
        {
            return null;
        }

        return values[0];
    }
}
