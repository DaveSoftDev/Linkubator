using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política para normalización y validación de texto.
/// </summary>
public static class UserTextPolicy
{
    /// <summary>
    /// Recorta el texto y devuelve <c>null</c> si es nulo, vacío o contiene solo espacios en blanco.
    /// </summary>
    /// <param name="value">Texto a recortar.</param>
    /// <returns>Texto recortado o <c>null</c>.</returns>
    public static string? TrimToNull(string? value)
    {
        string? trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// Lanza una excepción si el texto es nulo, vacío o contiene solo espacios en blanco.
    /// </summary>
    /// <param name="value">Texto a validar.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el texto no es válido.</exception>
    public static void ThrowIfNullOrWhiteSpace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainExceptions.NotValidInputException();
        }
    }

    /// <summary>
    /// Cuenta los puntos de código Unicode del texto.
    /// </summary>
    /// <param name="value">Texto a contar.</param>
    /// <returns>Número de puntos de código.</returns>
    /// <exception cref="ArgumentNullException">Si <paramref name="value"/> es nulo.</exception>
    public static int CountCodePoints(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        int count = 0;
        int index = 0;
        while (index < value.Length)
        {
            count++;

            if (char.IsHighSurrogate(value[index]) && index + 1 < value.Length && char.IsLowSurrogate(value[index + 1]))
            {
                index++;
            }

            index++;
        }

        return count;
    }

    /// <summary>
    /// Determina si el texto está dentro de la longitud máxima permitida.
    /// </summary>
    /// <param name="value">Texto a validar.</param>
    /// <param name="maximum">Longitud máxima permitida.</param>
    /// <returns><c>true</c> si está dentro del límite; de lo contrario, <c>false</c>.</returns>
    public static bool IsWithinMaximumLength(string value, int maximum)
    {
        return CountCodePoints(value) <= maximum;
    }

    /// <summary>
    /// Garantiza que el texto no supera la longitud máxima permitida.
    /// </summary>
    /// <param name="value">Texto a validar.</param>
    /// <param name="maximum">Longitud máxima permitida.</param>
    /// <returns>Texto validado.</returns>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si el texto supera el máximo.</exception>
    public static string EnsureMaximumLength(string value, int maximum)
    {
        if (!IsWithinMaximumLength(value, maximum))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        return value;
    }

    /// <summary>
    /// Normaliza un texto obligatorio: lo recorta, rechaza el vacío o solo espacios y
    /// comprueba que no supere la longitud máxima.
    /// </summary>
    /// <param name="value">Texto a normalizar.</param>
    /// <param name="maximum">Longitud máxima permitida.</param>
    /// <returns>Texto normalizado.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el texto es nulo, vacío o solo espacios.</exception>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si el texto supera el máximo.</exception>
    public static string EnsureRequiredText(string? value, int maximum)
    {
        string trimmed = value?.Trim() ?? string.Empty;
        ThrowIfNullOrWhiteSpace(trimmed);
        return EnsureMaximumLength(trimmed, maximum);
    }

    /// <summary>
    /// Normaliza un texto opcional: recorta a <c>null</c> la entrada nula, vacía o solo
    /// espacios, y en el resto de los casos comprueba la longitud máxima.
    /// </summary>
    /// <param name="value">Texto a normalizar.</param>
    /// <param name="maximum">Longitud máxima permitida.</param>
    /// <returns>Texto normalizado o <c>null</c>.</returns>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si el texto supera el máximo.</exception>
    public static string? EnsureOptionalMaximumLength(string? value, int maximum)
    {
        string? trimmed = TrimToNull(value);
        return trimmed is null ? null : EnsureMaximumLength(trimmed, maximum);
    }

    /// <summary>
    /// Comprueba que la longitud, medida en puntos de código, queda dentro del rango indicado.
    /// </summary>
    /// <param name="value">Texto a validar.</param>
    /// <param name="minimum">Longitud mínima permitida.</param>
    /// <param name="maximum">Longitud máxima permitida.</param>
    /// <exception cref="DomainExceptions.TextBelowMinimumLengthException">Si el texto está por debajo del mínimo.</exception>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si el texto supera el máximo.</exception>
    public static void EnsureWithinRange(string value, int minimum, int maximum)
    {
        if (CountCodePoints(value) < minimum)
        {
            throw new DomainExceptions.TextBelowMinimumLengthException();
        }

        EnsureMaximumLength(value, maximum);
    }
}