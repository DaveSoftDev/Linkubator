namespace Linkubator.Application;

public sealed record CreateCollectionRequest(string Name, string? Description = null, bool IsPublic = false);
