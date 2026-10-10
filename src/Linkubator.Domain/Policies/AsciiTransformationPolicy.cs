using System.Globalization;
using System.Text;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Transformación común a ASCII de <c>User.Alias</c>, <c>Collection.Slug</c> y <c>Tag.Slug</c>.
/// No recorta el texto ni valida longitud o palabras reservadas: cada política añade las suyas.
/// </summary>
public static class AsciiTransformationPolicy
{
    private const char Hyphen = '-';

    // Se aplican tras NFKD, minúsculas y eliminación de marcas diacríticas.
    private static readonly Dictionary<int, string> _symbolWords = new()
    {
        ['+'] = "plus",
        ['#'] = "sharp",
        ['&'] = "and",
        ['@'] = "at",
        ['%'] = "percent",
        ['$'] = "dollar",
        ['€'] = "euro",
        ['£'] = "pound",
    };

    private static readonly HashSet<int> _separators = new()
    {
        '.',
        '/',
        '_',
        '\u2044',
    };

    private static readonly Dictionary<int, string> _latinLetters = new()
    {
        ['ß'] = "ss",
        ['æ'] = "ae",
        ['œ'] = "oe",
        ['ø'] = "o",
        ['đ'] = "d",
        ['ł'] = "l",
        ['ð'] = "d",
        ['þ'] = "th",
        ['ħ'] = "h",
        ['ı'] = "i",
        ['ĸ'] = "k",
        ['ŋ'] = "ng",
        ['ŧ'] = "t",
    };

    public static string Transform(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string normalized = Normalize(text);
        RejectControlsAndSeparators(normalized);

        string lowercase = normalized.ToLowerInvariant();

        StringBuilder builder = new(lowercase.Length);
        foreach (Rune rune in lowercase.EnumerateRunes())
        {
            UnicodeCategory category = Rune.GetUnicodeCategory(rune);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            Append(builder, rune, category);
        }

        TrimTrailingHyphen(builder);

        return builder.ToString();
    }

    private static string Normalize(string text)
    {
        try
        {
            return text.Normalize(NormalizationForm.FormKD);
        }
        catch (ArgumentException)
        {
            // Texto con puntos de código Unicode no válidos (por ejemplo, un sustituto suelto).
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }
    }

    private static void RejectControlsAndSeparators(string normalized)
    {
        foreach (Rune rune in normalized.EnumerateRunes())
        {
            UnicodeCategory category = Rune.GetUnicodeCategory(rune);
            bool isControl = category == UnicodeCategory.Control;
            bool isSeparator = category is UnicodeCategory.SpaceSeparator
                or UnicodeCategory.LineSeparator
                or UnicodeCategory.ParagraphSeparator;

            if (isControl || (isSeparator && rune.Value != ' '))
            {
                throw new DomainExceptions.TextContainsUnsupportedCharactersException();
            }
        }
    }

    private static void Append(StringBuilder builder, Rune rune, UnicodeCategory category)
    {
        int value = rune.Value;

        if (value == ' ' || _separators.Contains(value))
        {
            AppendHyphen(builder);
            return;
        }

        if (_symbolWords.TryGetValue(value, out string? word))
        {
            AppendHyphen(builder);
            builder.Append(word);
            AppendHyphen(builder);
            return;
        }

        if (_latinLetters.TryGetValue(value, out string? replacement))
        {
            builder.Append(replacement);
            return;
        }

        if ((value >= 'a' && value <= 'z') || (value >= '0' && value <= '9'))
        {
            builder.Append((char)value);
            return;
        }

        if (value == Hyphen)
        {
            AppendHyphen(builder);
            return;
        }

        if (IsLetterOrNumber(category))
        {
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }

        // Emojis, puntuación y símbolos fuera de la tabla: se eliminan sin conversión.
    }

    private static bool IsLetterOrNumber(UnicodeCategory category)
    {
        return category is UnicodeCategory.UppercaseLetter
            or UnicodeCategory.LowercaseLetter
            or UnicodeCategory.TitlecaseLetter
            or UnicodeCategory.ModifierLetter
            or UnicodeCategory.OtherLetter
            or UnicodeCategory.DecimalDigitNumber
            or UnicodeCategory.LetterNumber
            or UnicodeCategory.OtherNumber;
    }

    private static void AppendHyphen(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] != Hyphen)
        {
            builder.Append(Hyphen);
        }
    }

    private static void TrimTrailingHyphen(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] == Hyphen)
        {
            builder.Length--;
        }
    }
}
