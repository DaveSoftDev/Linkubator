namespace Linkubator.Web.Pages.App.Links;

public sealed class LinkInput
{
    public string? CollectionId { get; set; }

    public string? Url { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Image { get; set; }

    public bool IsPublic { get; set; }
}
