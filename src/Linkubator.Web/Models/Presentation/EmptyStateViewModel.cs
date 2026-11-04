namespace Linkubator.Web.Models.Presentation;

public sealed class EmptyStateViewModel
{
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? ActionUrl { get; init; }

    public string? ActionText { get; init; }
}
