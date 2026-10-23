namespace Linkubator.Web.Models.Presentation;

public sealed class SearchFormViewModel
{
    public string Action { get; init; } = string.Empty;

    public string Method { get; init; } = "get";

    public string Label { get; init; } = "Buscar";

    public string Placeholder { get; init; } = "Buscar";

    public string ButtonText { get; init; } = "Buscar";

    public string QueryParameterName { get; init; } = "q";

    public string QueryValue { get; init; } = string.Empty;

    public string QueryInputId { get; init; } = "search-query";

    public IReadOnlyList<KeyValuePair<string, string>> HiddenFields { get; init; } = [];
}
