namespace Linkubator.Application.Dtos.Models;

public sealed record CollectionPageDto(IReadOnlyCollection<CollectionDto> Items, int Page, int PageCount);
