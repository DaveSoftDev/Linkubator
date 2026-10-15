using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Único punto del dominio que analiza la sintaxis de autoridad de una entrada.
/// </summary>
public static class UriAuthorityPolicy
{
    /// <summary>
    /// Comprueba si el esquema es uno de los que el producto admite, sin distinguir mayúsculas.
    /// </summary>
    /// <param name="scheme">Esquema a comprobar.</param>
    /// <returns><c>true</c> si el esquema está permitido; de lo contrario, <c>false</c>.</returns>
    public static bool IsAllowedScheme(string? scheme)
    {
        return string.Equals(scheme, UrlConstants.HttpScheme, StringComparison.OrdinalIgnoreCase)
            || string.Equals(scheme, UrlConstants.HttpsScheme, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Analiza la sintaxis de autoridad de la entrada.
    /// </summary>
    /// <param name="input">Entrada a analizar.</param>
    /// <returns>Objeto <see cref="UriAuthority"/> con el análisis o <c>null</c> si no se reconoce.</returns>
    public static UriAuthority? Analyze(string input)
    {
        return UriAuthoritySyntax.Parse(input);
    }

    /// <summary>
    /// Comprueba el formato de host admitido.
    /// </summary>
    /// <param name="input">Host a validar.</param>
    /// <returns><c>true</c> si el host es válido; de lo contrario, <c>false</c>.</returns>
    public static bool IsValidHost(string input)
    {
        return UriHostFormat.IsValid(input);
    }

    /// <summary>
    /// Comprueba el formato del puerto.
    /// </summary>
    /// <param name="port">Puerto a validar.</param>
    /// <returns><c>true</c> si el puerto es válido; de lo contrario, <c>false</c>.</returns>
    public static bool IsValidPort(string? port)
    {
        return UriPortFormat.IsValid(port);
    }

    /// <summary>
    /// Intenta extraer el host de la entrada.
    /// </summary>
    /// <param name="input">Entrada a analizar.</param>
    /// <param name="host">Host extraído.</param>
    /// <returns><c>true</c> si se pudo extraer el host; de lo contrario, <c>false</c>.</returns>
    public static bool TryExtractHost(string input, out string? host)
    {
        host = null;

        if (Analyze(input) is not { } authority)
        {
            return false;
        }

        if (!IsExtractableHost(authority))
        {
            return false;
        }

        host = authority.Host;
        return true;
    }

    /// <summary>
    /// Comprueba que del análisis se puede extraer un host válido.
    /// </summary>
    /// <param name="authority">Autoridad analizada.</param>
    /// <returns><c>true</c> si se puede extraer el host; de lo contrario, <c>false</c>.</returns>
    private static bool IsExtractableHost(UriAuthority authority)
    {
        string? candidate = authority.Host;

        if (string.IsNullOrEmpty(candidate))
        {
            return false;
        }

        if (authority.HasPort && !IsNonEmptyValidPort(authority.Port))
        {
            return false;
        }

        return candidate.StartsWith('[')
            ? candidate.EndsWith(']')
            : !candidate.Contains(':');
    }

    /// <summary>
    /// Comprueba si el puerto no está vacío y es válido.
    /// </summary>
    /// <param name="port">Puerto a comprobar.</param>
    /// <returns><c>true</c> si es válido; de lo contrario, <c>false</c>.</returns>
    private static bool IsNonEmptyValidPort(string? port)
    {
        return !string.IsNullOrEmpty(port) && IsValidPort(port);
    }

    /// <summary>
    /// Comprueba si la autoridad es válida (tiene host válido y puerto válido).
    /// </summary>
    /// <param name="authority">Autoridad a validar.</param>
    /// <returns><c>true</c> si es válida; de lo contrario, <c>false</c>.</returns>
    public static bool IsValidAuthority(UriAuthority? authority)
    {
        if (authority is null || authority.Host is null)
        {
            return false;
        }

        if (!IsValidHost(authority.Host))
        {
            return false;
        }

        return IsValidPort(authority.Port);
    }

    /// <summary>
    /// Comprueba si la autoridad tiene la forma host:puerto sin esquema y sin protocol-relative.
    /// </summary>
    /// <param name="authority">Autoridad a comprobar.</param>
    /// <returns><c>true</c> si tiene forma host:puerto; de lo contrario, <c>false</c>.</returns>
    public static bool IsHostAndPortForm(UriAuthority? authority)
    {
        return authority is not null
            && !authority.HasExplicitScheme
            && !authority.IsProtocolRelative
            && authority.HasHost
            && authority.HasPort;
    }
}
