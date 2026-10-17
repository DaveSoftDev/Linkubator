namespace Linkubator.Application;

public sealed record PublicCollectionSummaryDto(
    string Name,
    string Slug,
    string? Description);
