namespace Linkubator.Application.Dtos.Requests;

public sealed record CreateCollectionRequest(string Name, string? Description = null, bool IsPublic = false);
