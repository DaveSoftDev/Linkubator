using System.Text;

namespace Linkubator.Infrastructure.Search;

public static class SearchTextNormalizer
{
    public static string? Normalize(string? text)
        => text?.Normalize(NormalizationForm.FormC);
}
