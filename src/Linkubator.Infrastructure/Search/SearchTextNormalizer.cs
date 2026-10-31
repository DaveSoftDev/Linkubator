using System.Text;

namespace Linkubator.Infrastructure.Search;

public static class SearchTextNormalizer
{
    /// <summary>
    /// Normaliza el texto a Unicode Form C, conservando <c>null</c> si no se proporcionó texto.
    /// </summary>
    public static string? Normalize(string? text)
        => text?.Normalize(NormalizationForm.FormC);
}
