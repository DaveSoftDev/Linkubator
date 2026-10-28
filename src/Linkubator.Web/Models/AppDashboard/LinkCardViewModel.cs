namespace Linkubator.Web.Models.AppDashboard;

public sealed class LinkCardViewModel
{
    public string Domain => Linkubator.Web.Extensions.UrlExtensions.GetDomain(Url);

    public long Id { get; init; }
    public bool IsPublic { get; init; }
    public string Title { get; init; } = string.Empty;
    public string CollectionName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string AccentClass { get; init; } = "blue";
    public string Url { get; init; } = string.Empty;
}
