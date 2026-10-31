using System.Globalization;
using System.Text;

namespace Linkubator.Infrastructure.Search;

public static class SearchQueryBuilder
{
    /// <summary>
    /// Convierte el texto de búsqueda en una consulta compatible con la búsqueda de texto completo.
    /// </summary>
    public static string? Build(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        List<string> terms = [];
        StringBuilder currentTerm = new();

        foreach (Rune rune in text.EnumerateRunes())
        {
            UnicodeCategory category = Rune.GetUnicodeCategory(rune);
            if (category is UnicodeCategory.UppercaseLetter
                or UnicodeCategory.LowercaseLetter
                or UnicodeCategory.TitlecaseLetter
                or UnicodeCategory.ModifierLetter
                or UnicodeCategory.OtherLetter
                or UnicodeCategory.DecimalDigitNumber
                or UnicodeCategory.LetterNumber
                or UnicodeCategory.OtherNumber)
            {
                currentTerm.Append(rune);
                continue;
            }

            AddCurrentTerm(terms, currentTerm);
        }

        AddCurrentTerm(terms, currentTerm);

        return terms.Count == 0
            ? null
            : string.Join(' ', terms.Select(static term => $"\"{term}\"*"));
    }

    /// <summary>
    /// Añade el término acumulado a la lista y vacía el búfer.
    /// </summary>
    private static void AddCurrentTerm(List<string> terms, StringBuilder currentTerm)
    {
        if (currentTerm.Length == 0)
        {
            return;
        }

        terms.Add(currentTerm.ToString());
        currentTerm.Clear();
    }
}
