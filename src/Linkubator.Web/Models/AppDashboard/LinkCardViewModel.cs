namespace Linkubator.Web.Models.AppDashboard;

public sealed class LinkCardViewModel
{
    public string Title { get; init; } = string.Empty;

    public string CollectionName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Domain { get; init; } = string.Empty;

    public string? ImageUrl { get; init; }

    public string AccentClass { get; init; } = "blue";
}
