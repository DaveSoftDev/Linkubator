namespace Linkubator.Application.Dtos.Requests;

public sealed record LinkEditRequest(long LinkId, long CollectionId, string? Title = null, string? Description = null, string? Image = null, bool? IsPublic = null);