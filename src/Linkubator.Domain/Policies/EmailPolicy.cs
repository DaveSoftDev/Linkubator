using System.Text.RegularExpressions;
using DomainExceptions = Linkubator.Domain.Exceptions;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política para validación y normalización de direcciones de correo electrónico.
/// </summary>
public static partial class EmailPolicy
{
    private static readonly Regex _emailRegex = EmailPattern();

    /// <summary>
    /// Normaliza y valida una dirección de correo electrónico.
    /// </summary>
    /// <param name="email">Correo electrónico a normalizar.</param>
    /// <returns>Correo electrónico normalizado en minúsculas.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el correo es nulo, vacío o solo espacios.</exception>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si el correo supera la longitud máxima.</exception>
    /// <exception cref="DomainExceptions.InvalidEmailException">Si el formato del correo no es válido.</exception>
    public static string Normalize(string? email)
    {
        string? trimmedEmail = email?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedEmail))
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (!UserTextPolicy.IsWithinMaximumLength(trimmedEmail, LengthLimitsConstants.UserEmail))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        if (!_emailRegex.IsMatch(trimmedEmail))
        {
            throw new DomainExceptions.InvalidEmailException();
        }

        return trimmedEmail.ToLowerInvariant();
    }

    /// <summary>
    /// Patrón anclado de direcciones de correo electrónico válidas.
    /// </summary>
    /// <remarks>
    /// Debe coincidir literalmente con la expresión de
    /// specifications.md → «Email»: anclada con <c>\A</c> y <c>\z</c>, no con <c>^</c> y <c>$</c>,
    /// porque en .NET <c>$</c> también admite la posición anterior a un salto de línea final.
    /// </remarks>
    /// <returns>Expresión regular compilada del correo electrónico.</returns>
    [GeneratedRegex(@"\A(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x21\x23-\x5B\x5D-\x7E]|\\[\x09\x20-\x7E])*"")@(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\[[\x21-\x5A\x5E-\x7E]*\])\z")]
    private static partial Regex EmailPattern();
}
