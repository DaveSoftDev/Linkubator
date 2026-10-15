using System.Globalization;
using System.Text;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política de transformación ASCII para alias y slugs.
/// </summary>
public static class AsciiTransformationPolicy
{
    private const char Hyphen = '-';

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

    /// <summary>
    /// Transforma el texto a un formato ASCII apto para alias y slugs.
    /// </summary>
    /// <param name="text">Texto a transformar.</param>
    /// <returns>Texto transformado.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el texto es nulo, vacío o solo espacios.</exception>
    /// <exception cref="DomainExceptions.TextContainsUnsupportedCharactersException">Si el texto contiene caracteres no soportados.</exception>
    public static string Transform(string text)
    {
        UserTextPolicy.ThrowIfNullOrWhiteSpace(text);

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

    /// <summary>
    /// Normaliza el texto utilizando la forma de normalización Unicode FormKD.
    /// </summary>
    /// <param name="text">Texto a normalizar.</param>
    /// <returns>Texto normalizado.</returns>
    /// <exception cref="DomainExceptions.TextContainsUnsupportedCharactersException">Si el texto contiene caracteres Unicode no válidos.</exception>
    private static string Normalize(string text)
    {
        try
        {
            return text.Normalize(NormalizationForm.FormKD);
        }
        catch (ArgumentException)
        {
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }
    }

    /// <summary>
    /// Rechaza caracteres de control y separadores no permitidos en el texto normalizado.
    /// </summary>
    /// <param name="normalized">Texto normalizado a validar.</param>
    /// <exception cref="DomainExceptions.TextContainsUnsupportedCharactersException">Si se detecta un carácter no permitido.</exception>
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

    /// <summary>
    /// Añade el rune correspondiente al constructor de resultado según su categoría Unicode.
    /// </summary>
    /// <param name="builder">Constructor del resultado.</param>
    /// <param name="rune">Rune a procesar.</param>
    /// <param name="category">Categoría Unicode del rune.</param>
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
    }

    /// <summary>
    /// Determina si la categoría Unicode corresponde a una letra o número.
    /// </summary>
    /// <param name="category">Categoría Unicode a evaluar.</param>
    /// <returns><c>true</c> si es letra o número; de lo contrario, <c>false</c>.</returns>
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

    /// <summary>
    /// Añade un guion medio si el último carácter del constructor no es un guion.
    /// </summary>
    /// <param name="builder">Constructor del resultado.</param>
    private static void AppendHyphen(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] != Hyphen)
        {
            builder.Append(Hyphen);
        }
    }

    /// <summary>
    /// Elimina el guion medio final si está presente.
    /// </summary>
    /// <param name="builder">Constructor del resultado.</param>
    private static void TrimTrailingHyphen(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] == Hyphen)
        {
            builder.Length--;
        }
    }
}
