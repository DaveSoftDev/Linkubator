namespace Linkubator.Web.Models.PrivateListing;

public sealed record PrivateListingQueryAnalysisResult(
    int PageNumber,
    long? CollectionId,
    string? SearchText,
    bool HasCollectionError,
    bool HasSearchError,
    string? CollectionErrorMessage,
    string? SearchErrorMessage)
{
    public static PrivateListingQueryAnalysisResult Empty { get; } = new(
        1,
        null,
        null,
        false,
        false,
        null,
        null);

    public bool HasAnyError => HasCollectionError || HasSearchError;
}
