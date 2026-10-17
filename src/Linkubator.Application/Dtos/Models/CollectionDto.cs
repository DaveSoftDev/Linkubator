namespace Linkubator.Application.Dtos.Models;

public sealed record CollectionDto(long Id, string Name, string Slug, string? Description, bool IsPublic, int LinkCount, DateTime CreatedAt);
