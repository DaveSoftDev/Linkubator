namespace Linkubator.Web.Models.Presentation;

public sealed class PageErrorSummaryItemViewModel
{
    public string Message { get; init; } = string.Empty;

    public string? FieldId { get; init; }
}
