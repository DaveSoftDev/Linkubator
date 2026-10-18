namespace Linkubator.Application.Dtos.Requests;

public sealed record CollectionChangeVisibilityRequest(long CollectionId, bool IsPublic);
