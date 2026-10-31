using System.Globalization;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Formato de puerto admitido en la autoridad de una URL: un puerto ausente se considera
/// válido y, escrito, debe estar formado solo por dígitos y quedar entre 1 y 65535.
/// Colaborador interno de <see cref="UriAuthorityPolicy"/>, que es el único punto público
/// que lo consume y el que aplica la regla de specifications.md → «Detección de esquema URI».
/// </summary>
internal static class UriPortFormat
{
    /// <summary>
    /// Determina si el puerto está ausente o tiene un valor numérico válido.
    /// </summary>
    /// <param name="port">Puerto que se va a validar, o <c>null</c> si está ausente.</param>
    /// <returns><c>true</c> si el puerto es válido; de lo contrario, <c>false</c>.</returns>
    internal static bool IsValid(string? port)
    {
        if (port is null)
        {
            return true;
        }

        if (string.IsNullOrEmpty(port))
        {
            return false;
        }

        return int.TryParse(port, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
            && value >= 1
            && value <= 65535;
    }
}
