namespace Linkubator.Application.Dtos.Requests;

public sealed record LinkChangeVisibilityRequest(long LinkId, bool IsPublic);