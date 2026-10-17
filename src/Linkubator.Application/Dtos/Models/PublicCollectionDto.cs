namespace Linkubator.Application.Dtos.Models;

public sealed record PublicCollectionDto(string Alias, string CollectionName, string Slug, IReadOnlyList<PublicLinkDto> Links);
