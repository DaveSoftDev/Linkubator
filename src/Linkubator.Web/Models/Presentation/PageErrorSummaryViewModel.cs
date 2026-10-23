namespace Linkubator.Web.Models.Presentation;

public sealed class PageErrorSummaryViewModel
{
    public string Title { get; init; } = "Hay errores en el formulario";

    public IReadOnlyList<PageErrorSummaryItemViewModel> Items { get; init; } = [];
}
