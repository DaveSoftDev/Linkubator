namespace Linkubator.Web.Models.Presentation;

public sealed class PaginationPageLinkViewModel
{
    public string Label { get; init; } = string.Empty;

    public string? Url { get; init; }

    public bool IsCurrent { get; init; }

    public bool IsDisabled { get; init; }
}
