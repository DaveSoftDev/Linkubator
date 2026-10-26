namespace Linkubator.Application.Dtos.Requests;

public sealed record LinkUpdateRequest(long LinkId, long CollectionId, string? Title = null, string? Description = null, string? Image = null, bool? IsPublic = null);