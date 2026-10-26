namespace Linkubator.Application.Dtos.Models;

public sealed record LinkPageDto(IReadOnlyList<LinkDto> Items, int Page, int PageCount);
