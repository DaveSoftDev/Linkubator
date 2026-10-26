namespace Linkubator.Application.Dtos.Requests;

public sealed record CollectionUpdateRequest(long CollectionId, string? Name, string? Description, bool IsPublic);
