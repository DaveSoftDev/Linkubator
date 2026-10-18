namespace Linkubator.Application.Dtos.Requests;

public sealed record CollectionCreateRequest(string Name, string? Description = null, bool IsPublic = false);
