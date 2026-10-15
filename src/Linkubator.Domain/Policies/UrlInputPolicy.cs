using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política para la higiene común de la entrada de texto de una URL antes de ajustarla.
/// </summary>
public static class UrlInputPolicy
{
    /// <summary>
    /// Prepara la entrada de URL realizando recorte, validación y comprobación de longitud.
    /// </summary>
    /// <param name="input">Entrada de URL a preparar.</param>
    /// <param name="maximumLength">Longitud máxima opcional permitida.</param>
    /// <returns>Entrada preparada.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si la entrada es nula, vacía o solo espacios.</exception>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si la entrada supera la longitud máxima.</exception>
    /// <exception cref="DomainExceptions.TextContainsUnsupportedCharactersException">Si la entrada contiene espacios interiores, caracteres de control o puntos de código no válidos.</exception>
    public static string Prepare(string? input, int? maximumLength = null)
    {
        if (input is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        string trimmed = input.Trim();
        UserTextPolicy.ThrowIfNullOrWhiteSpace(trimmed);

        if (maximumLength is int maximum && !UserTextPolicy.IsWithinMaximumLength(trimmed, maximum))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        if (trimmed.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)))
        {
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }

        try
        {
            _ = trimmed.Normalize();
        }
        catch (ArgumentException)
        {
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }

        return trimmed;
    }
}
