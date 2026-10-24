namespace Linkubator.Web.Models.PrivateListing;

public sealed record PrivateListingQueryAnalysisResult(
    int PageNumber,
    long? CollectionId,
    string? SearchText,
    bool HasPageError,
    bool HasCollectionError,
    bool HasSearchError,
    string? PageErrorMessage,
    string? CollectionErrorMessage,
    string? SearchErrorMessage)
{
    public static PrivateListingQueryAnalysisResult Empty { get; } = new(
        1,
        null,
        null,
        false,
        false,
        false,
        null,
        null,
        null);

    public bool HasAnyError => HasPageError || HasCollectionError || HasSearchError;
}
