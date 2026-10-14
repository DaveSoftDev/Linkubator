using System.Net;
using System.Net.Sockets;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Formato de host admitido en la autoridad de una URL: IPv6 entre corchetes, IPv4 de
/// cuatro grupos, <c>localhost</c> o dominio con al menos dos etiquetas y una última
/// etiqueta alfabética o de punycode. Colaborador interno de
/// <see cref="UriAuthorityPolicy"/>, que es el único punto público que lo consume y el
/// que aplica la regla de specifications.md → «Validación».
/// </summary>
internal static class UriHostFormat
{
    internal static bool IsValid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string candidate = input.Trim();

        if (IsBracketedIpv6Host(candidate))
        {
            return IsValidBracketedIpv6(candidate);
        }

        if (IPAddress.TryParse(candidate, out IPAddress? parsedAddress))
        {
            return parsedAddress.AddressFamily == AddressFamily.InterNetworkV6
                || (parsedAddress.AddressFamily == AddressFamily.InterNetwork && HasFourDecimalGroups(candidate));
        }

        if (string.Equals(candidate, UrlConstants.LocalhostHost, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IsValidDomainHost(candidate);
    }

    /// <summary>
    /// Determina si el host está entre corchetes (IPv6).
    /// </summary>
    /// <param name="candidate">Host candidato.</param>
    /// <returns><c>true</c> si está entre corchetes; de lo contrario, <c>false</c>.</returns>
    private static bool IsBracketedIpv6Host(string candidate) =>
        candidate.StartsWith("[", StringComparison.Ordinal)
        && candidate.EndsWith("]", StringComparison.Ordinal);

    /// <summary>
    /// Valida un host IPv6 entre corchetes.
    /// </summary>
    /// <param name="candidate">Host IPv6 candidato.</param>
    /// <returns><c>true</c> si es válido; de lo contrario, <c>false</c>.</returns>
    private static bool IsValidBracketedIpv6(string candidate)
    {
        string literal = candidate[1..^1];
        return IPAddress.TryParse(literal, out IPAddress? ipv6Address)
            && ipv6Address.AddressFamily == AddressFamily.InterNetworkV6;
    }

    /// <summary>
    /// Valida un host de dominio.
    /// </summary>
    /// <param name="candidate">Host de dominio candidato.</param>
    /// <returns><c>true</c> si es válido; de lo contrario, <c>false</c>.</returns>
    private static bool IsValidDomainHost(string candidate)
    {
        if (HasInvalidDomainStructure(candidate))
        {
            return false;
        }

        string[] labels = candidate.Split('.', StringSplitOptions.TrimEntries);
        if (labels.Length < 2 || labels.Any(string.IsNullOrEmpty))
        {
            return false;
        }

        return labels.All(IsValidDomainLabel)
            && IsValidDomainTld(labels[^1]);
    }

    /// <summary>
    /// Comprueba si la estructura del dominio es inválida.
    /// </summary>
    /// <param name="candidate">Dominio candidato.</param>
    /// <returns><c>true</c> si es inválida; de lo contrario, <c>false</c>.</returns>
    private static bool HasInvalidDomainStructure(string candidate) =>
        candidate.Contains("..", StringComparison.Ordinal)
        || candidate.StartsWith('.')
        || candidate.EndsWith('.');

    /// <summary>
    /// Valida una etiqueta de dominio.
    /// </summary>
    /// <param name="label">Etiqueta a validar.</param>
    /// <returns><c>true</c> si es válida; de lo contrario, <c>false</c>.</returns>
    private static bool IsValidDomainLabel(string label)
    {
        if (string.IsNullOrEmpty(label) || label.StartsWith('-') || label.EndsWith('-'))
        {
            return false;
        }

        return label.All(character => char.IsLetterOrDigit(character) || character == '-');
    }

    /// <summary>
    /// Valida el TLD del dominio.
    /// </summary>
    /// <param name="lastLabel">Última etiqueta del dominio.</param>
    /// <returns><c>true</c> si es válida; de lo contrario, <c>false</c>.</returns>
    private static bool IsValidDomainTld(string lastLabel)
    {
        if (lastLabel.StartsWith("xn--", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return lastLabel.Length >= 2 && lastLabel.All(char.IsLetter);
    }

    /// <summary>
    /// Comprueba si la cadena tiene cuatro grupos decimales (IPv4).
    /// </summary>
    /// <param name="candidate">Candidato IPv4.</param>
    /// <returns><c>true</c> si tiene cuatro grupos; de lo contrario, <c>false</c>.</returns>
    private static bool HasFourDecimalGroups(string candidate)
    {
        string[] groups = candidate.Split('.');

        return groups.Length == 4
            && groups.All(group => group.Length is > 0 and <= 3 && group.All(char.IsAsciiDigit));
    }
}
