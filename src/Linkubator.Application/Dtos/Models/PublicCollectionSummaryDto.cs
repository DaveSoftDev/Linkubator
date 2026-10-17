namespace Linkubator.Application.Dtos.Models;

public sealed record PublicCollectionSummaryDto(
    string Name,
    string Slug,
    string? Description);
