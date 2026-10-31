using System.Text.RegularExpressions;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Análisis sintáctico de una entrada de URL.
/// </summary>
internal static partial class UriAuthoritySyntax
{
    /// <summary>
    /// Proporciona la expresión regular generada que valida la sintaxis de un nombre de esquema URI.
    /// </summary>
    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9+\.\-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex SchemeNameRegex();

    /// <summary>
    /// Analiza la sintaxis de una entrada de URL para extraer la autoridad.
    /// </summary>
    /// <param name="input">Entrada a analizar.</param>
    /// <returns>Autoridad analizada o <c>null</c> si no se reconoce.</returns>
    internal static UriAuthority? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string candidate = input.Trim();
        bool isProtocolRelative = candidate.StartsWith("//", StringComparison.Ordinal);
        string text = isProtocolRelative ? candidate[2..] : candidate;

        if (text.Length == 0)
        {
            return null;
        }

        return HandleExplicitScheme(text, isProtocolRelative)
            ?? HandleHostPortForm(text, isProtocolRelative)
            ?? HandleSchemeWithoutSeparator(text, isProtocolRelative)
            ?? HandleBareAuthority(text, isProtocolRelative);
    }

    /// <summary>
    /// Analiza una entrada con esquema explícito del tipo scheme://...
    /// </summary>
    /// <param name="text">Texto a analizar.</param>
    /// <param name="isProtocolRelative">Indica si la entrada es protocol-relative.</param>
    /// <returns>Autoridad analizada o <c>null</c> si no es válida.</returns>
    private static UriAuthority? HandleExplicitScheme(string text, bool isProtocolRelative)
    {
        int separatorIndex = text.IndexOf("://", StringComparison.Ordinal);
        int firstDelimiter = text.IndexOfAny(['/', '?', '#']);
        if (separatorIndex < 0 || (firstDelimiter >= 0 && separatorIndex >= firstDelimiter))
        {
            return null;
        }

        string scheme = text[..separatorIndex];
        string authorityText = text[(separatorIndex + 3)..];
        if (IsEmptyAuthorityAfterScheme(authorityText))
        {
            return null;
        }

        string? host = ExtractHostFromAuthority(authorityText, out string? port);
        return new UriAuthority(scheme, host, host is null ? null : port, isProtocolRelative);
    }

    /// <summary>
    /// Comprueba si la autoridad está vacía tras el separador ://.
    /// </summary>
    /// <param name="authorityText">Texto de autoridad a comprobar.</param>
    /// <returns><c>true</c> si está vacío o empieza con delimitador; de lo contrario, <c>false</c>.</returns>
    private static bool IsEmptyAuthorityAfterScheme(string authorityText)
    {
        return string.IsNullOrEmpty(authorityText)
            || authorityText[0] is ':' or '/' or '?' or '#';
    }

    /// <summary>
    /// Analiza una forma host:puerto.
    /// </summary>
    /// <param name="text">Texto a analizar.</param>
    /// <param name="isProtocolRelative">Indica si la entrada es protocol-relative.</param>
    /// <returns>Autoridad analizada o <c>null</c> si no es válida.</returns>
    private static UriAuthority? HandleHostPortForm(string text, bool isProtocolRelative)
    {
        if (!TryParseHostPort(text, out string? host, out string? port))
        {
            return null;
        }

        return new UriAuthority(null, host, port, isProtocolRelative);
    }

    /// <summary>
    /// Analiza una entrada con esquema pero sin separador :// (caso no admitido para autoridad completa).
    /// </summary>
    /// <param name="text">Texto a analizar.</param>
    /// <param name="isProtocolRelative">Indica si la entrada es protocol-relative.</param>
    /// <returns>Autoridad analizada o <c>null</c> si no es válida.</returns>
    private static UriAuthority? HandleSchemeWithoutSeparator(string text, bool isProtocolRelative)
    {
        int colonIndex = text.IndexOf(':');
        if (colonIndex <= 0 || colonIndex + 1 >= text.Length)
        {
            return null;
        }

        string schemeCandidate = text[..colonIndex];
        if (!SchemeNameRegex().IsMatch(schemeCandidate))
        {
            return null;
        }

        string afterColon = text[(colonIndex + 1)..];
        if (afterColon.Length > 0 && char.IsAsciiDigit(afterColon[0]) && TryParseHostPort(afterColon, out _, out _))
        {
            return null;
        }

        return new UriAuthority(schemeCandidate, null, null, isProtocolRelative);
    }

    /// <summary>
    /// Analiza una autoridad desnuda (sin esquema).
    /// </summary>
    /// <param name="text">Texto a analizar.</param>
    /// <param name="isProtocolRelative">Indica si la entrada es protocol-relative.</param>
    /// <returns>Autoridad analizada.</returns>
    private static UriAuthority? HandleBareAuthority(string text, bool isProtocolRelative)
    {
        string? bareHost = ExtractHostFromAuthority(text, out string? barePort);
        return new UriAuthority(null, bareHost, barePort, isProtocolRelative);
    }

    /// <summary>
    /// Intenta analizar una forma host:puerto.
    /// </summary>
    /// <param name="text">Texto a analizar.</param>
    /// <param name="host">Host extraído.</param>
    /// <param name="port">Puerto extraído.</param>
    /// <returns><c>true</c> si se pudo analizar correctamente; de lo contrario, <c>false</c>.</returns>
    private static bool TryParseHostPort(string text, out string? host, out string? port)
    {
        host = null;
        port = null;
        string truncated = text;
        int delim = text.IndexOfAny(['/', '?', '#']);
        if (delim >= 0)
        {
            truncated = text[..delim];
        }
        if (truncated.StartsWith('['))
        {
            int bracket = truncated.IndexOf(']');
            if (bracket < 0 || bracket == truncated.Length - 1)
            {
                return false;
            }
            if (truncated[bracket + 1] != ':')
            {
                return false;
            }
            string portPart = truncated[(bracket + 2)..];
            if (!UriPortFormat.IsValid(portPart))
            {
                return false;
            }
            host = truncated[..(bracket + 1)];
            port = portPart;
            return true;
        }
        int lastColon = truncated.LastIndexOf(':');
        if (lastColon <= 0)
        {
            return false;
        }
        string hostPart = truncated[..lastColon];
        string pPart = truncated[(lastColon + 1)..];
        if (!UriPortFormat.IsValid(pPart))
        {
            return false;
        }
        if (!UriHostFormat.IsValid(hostPart))
        {
            return false;
        }
        host = hostPart;
        port = pPart;
        return true;
    }

    /// <summary>
    /// Extrae el host y puerto de la autoridad, eliminando la información de usuario si existe.
    /// </summary>
    /// <param name="authorityText">Texto de autoridad a analizar.</param>
    /// <param name="port">Puerto extraído.</param>
    /// <returns>Host extraído o <c>null</c> si no es válido.</returns>
    private static string? ExtractHostFromAuthority(string authorityText, out string? port)
    {
        port = null;
        string truncated = authorityText;
        int delim = authorityText.IndexOfAny(['/', '?', '#']);
        if (delim >= 0)
        {
            truncated = authorityText[..delim];
        }

        if (!TryStripUserInfo(truncated, out truncated))
        {
            return null;
        }

        if (string.IsNullOrEmpty(truncated) || truncated.StartsWith(':'))
        {
            return null;
        }

        if (truncated.StartsWith('['))
        {
            return ExtractBracketedHost(truncated, out port);
        }

        int lastColon = truncated.LastIndexOf(':');
        if (lastColon <= 0)
        {
            return truncated;
        }

        port = truncated[(lastColon + 1)..];
        return truncated[..lastColon];
    }

    /// <summary>
    /// Intenta eliminar la información de usuario (@user:pass) del texto de autoridad.
    /// </summary>
    /// <param name="authorityText">Texto de autoridad.</param>
    /// <param name="truncated">Texto resultante sin información de usuario.</param>
    /// <returns><c>true</c> si la eliminación fue exitosa; de lo contrario, <c>false</c>.</returns>
    private static bool TryStripUserInfo(string authorityText, out string truncated)
    {
        truncated = authorityText;
        if (!truncated.Contains('@'))
        {
            return true;
        }

        int at = truncated.LastIndexOf('@');
        if (at < 0 || at >= truncated.Length - 1)
        {
            return false;
        }

        truncated = truncated[(at + 1)..];
        return true;
    }

    /// <summary>
    /// Extrae el host de una autoridad que empieza por corchete.
    /// </summary>
    /// <param name="truncated">Texto de autoridad a analizar.</param>
    /// <param name="port">Puerto extraído.</param>
    /// <returns>Host extraído o <c>null</c> si la forma entre corchetes está mal formada.</returns>
    private static string? ExtractBracketedHost(string truncated, out string? port)
    {
        port = null;

        int bracket = truncated.IndexOf(']');
        if (bracket < 0)
        {
            return null;
        }

        if (bracket == truncated.Length - 1)
        {
            return truncated;
        }

        if (truncated[bracket + 1] != ':')
        {
            return null;
        }

        port = truncated[(bracket + 2)..];
        return truncated[..(bracket + 1)];
    }
}
