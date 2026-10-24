namespace Linkubator.Web.Models.Presentation;

public sealed class PaginationViewModel
{
    public string Label { get; init; } = "Paginación de resultados";

    public IReadOnlyList<PaginationPageLinkViewModel> Pages { get; init; } = [];
}
