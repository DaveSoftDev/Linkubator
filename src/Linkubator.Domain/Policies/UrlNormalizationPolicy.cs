using System.Globalization;
using System.Net;
using System.Text;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static class UrlNormalizationPolicy
{
    private static readonly HashSet<string> _marketingParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        "utm_source",
        "utm_medium",
        "utm_campaign",
        "utm_term",
        "utm_content",
        "gclsrc",
        "gclid",
        "wbraid",
        "gbraid",
        "fbclid",
        "msclkid",
        "ttclid",
        "epik",
        "gad_source",
        "gad_campaignid",
        "srsltid",
        "gcs",
        "gcd",
        "at_medium",
        "at_campaign",
        "at_platform",
        "at_creation",
        "at_term"
    };

    /// <summary>
    /// Normaliza una URL para detectar duplicados.
    /// </summary>
    /// <param name="url">URL a normalizar.</param>
    /// <returns>Clave normalizada.</returns>
    public static string Normalize(string url)
    {
        UserTextPolicy.ThrowIfNullOrWhiteSpace(url);

        Uri uri = ParseAbsoluteUri(url);

        string host = uri.IdnHost;
        string port = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
        string path = Uri.UnescapeDataString(uri.AbsolutePath);
        string query = NormalizeQuery(uri.Query);

        string combined = host + port + path + query;
        return Canonicalize(combined);
    }

    /// <summary>
    /// Analiza la URL como absoluta.
    /// </summary>
    /// <param name="url">URL a analizar.</param>
    /// <returns>Objeto <see cref="Uri"/> absoluto.</returns>
    /// <exception cref="DomainExceptions.UrlNotAbsoluteException">Si la URL no es absoluta.</exception>
    private static Uri ParseAbsoluteUri(string url)
    {
        string parseableUrl = EscapePathUnicode(url);

        if (!Uri.TryCreate(parseableUrl, UriKind.Absolute, out Uri? uri) || uri is null)
        {
            throw new DomainExceptions.UrlNotAbsoluteException();
        }

        return uri;
    }

    /// <summary>
    /// Escapa los caracteres Unicode de la ruta para que Uri pueda analizarlos.
    /// </summary>
    /// <param name="url">URL a procesar.</param>
    /// <returns>URL con caracteres de ruta Unicode escapados.</returns>
    private static string EscapePathUnicode(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        int separatorIndex = url.IndexOfAny(['?', '#']);
        string preSeparator = separatorIndex >= 0 ? url[..separatorIndex] : url;
        string postSeparator = separatorIndex >= 0 ? url[separatorIndex..] : string.Empty;

        int schemeIndex = preSeparator.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex < 0)
        {
            return url;
        }

        int pathStartIndex = preSeparator.IndexOf('/', schemeIndex + 3);
        if (pathStartIndex < 0)
        {
            return url;
        }

        string authority = preSeparator[..pathStartIndex];
        string path = preSeparator[pathStartIndex..];

        StringBuilder escapedPath = new(path.Length);
        for (int index = 0; index < path.Length; index++)
        {
            char character = path[index];
            if (character == '%' && index + 2 < path.Length && char.IsAsciiHexDigit(path[index + 1]) && char.IsAsciiHexDigit(path[index + 2]))
            {
                escapedPath.Append(character);
                continue;
            }

            if (IsPathSafeCharacter(character))
            {
                escapedPath.Append(character);
                continue;
            }

            escapedPath.Append(Uri.EscapeDataString(character.ToString()));
        }

        return authority + escapedPath + postSeparator;
    }

    /// <summary>
    /// Determina si un carácter es seguro para la ruta.
    /// </summary>
    /// <param name="character">Carácter a evaluar.</param>
    /// <returns><c>true</c> si el carácter es seguro; de lo contrario, <c>false</c>.</returns>
    private static bool IsPathSafeCharacter(char character)
    {
        return character switch
        {
            >= 'a' and <= 'z' => true,
            >= 'A' and <= 'Z' => true,
            >= '0' and <= '9' => true,
            '-' or '.' or '_' or '~' or '!' or '$' or '&' or '\'' or '(' or ')' or '*' or '+' or ',' or ';' or '=' or ':' or '@' or '/' => true,
            _ => false
        };
    }

    /// <summary>
    /// Normaliza la cadena de consulta, eliminando parámetros de marketing y ordenando el resto.
    /// </summary>
    /// <param name="query">Consulta a normalizar.</param>
    /// <returns>Consulta normalizada o cadena vacía si no quedan parámetros.</returns>
    private static string NormalizeQuery(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return string.Empty;
        }

        List<KeyValuePair<string, string>> parameters = BuildNormalizedParameters(query);
        if (parameters.Count == 0)
        {
            return string.Empty;
        }

        return BuildQueryString(parameters);
    }

    /// <summary>
    /// Construye la lista de parámetros normalizados a partir de la consulta.
    /// </summary>
    /// <param name="query">Consulta a procesar.</param>
    /// <returns>Lista ordenada de parámetros normalizados.</returns>
    private static List<KeyValuePair<string, string>> BuildNormalizedParameters(string query)
    {
        string[] segments = query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return [];
        }

        List<KeyValuePair<string, string>> parameters = [];

        foreach (string segment in segments)
        {
            if (TryParseParameter(segment, out KeyValuePair<string, string> parameter))
            {
                parameters.Add(parameter);
            }
        }

        parameters.Sort((left, right) =>
        {
            int comparison = StringComparer.Ordinal.Compare(left.Key, right.Key);
            return comparison != 0 ? comparison : StringComparer.Ordinal.Compare(left.Value, right.Value);
        });

        return parameters;
    }

    /// <summary>
    /// Intenta analizar un parámetro de consulta.
    /// </summary>
    /// <param name="segment">Segmento a analizar.</param>
    /// <param name="parameter">Parámetro resultante.</param>
    /// <returns><c>true</c> si el parámetro es válido y no es de marketing; de lo contrario, <c>false</c>.</returns>
    private static bool TryParseParameter(string segment, out KeyValuePair<string, string> parameter)
    {
        parameter = default;

        int separatorIndex = segment.IndexOf('=');
        string name = separatorIndex >= 0 ? segment[..separatorIndex] : segment;
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        string value = separatorIndex >= 0 ? segment[(separatorIndex + 1)..] : string.Empty;
        string decodedName = WebUtility.UrlDecode(name);
        if (_marketingParameters.Contains(decodedName))
        {
            return false;
        }

        string decodedValue = WebUtility.UrlDecode(value);
        parameter = new KeyValuePair<string, string>(decodedName.ToLowerInvariant(), decodedValue.ToLowerInvariant());
        return true;
    }

    /// <summary>
    /// Construye la cadena de consulta a partir de los parámetros normalizados.
    /// </summary>
    /// <param name="parameters">Lista de parámetros.</param>
    /// <returns>Cadena de consulta.</returns>
    private static string BuildQueryString(List<KeyValuePair<string, string>> parameters)
    {
        StringBuilder queryBuilder = new();
        foreach (KeyValuePair<string, string> parameter in parameters)
        {
            queryBuilder.Append(queryBuilder.Length == 0 ? '?' : '&');
            queryBuilder.Append(parameter.Key);
            if (!string.IsNullOrEmpty(parameter.Value))
            {
                queryBuilder.Append('=').Append(parameter.Value);
            }
        }

        return queryBuilder.ToString();
    }

    /// <summary>
    /// Canonicaliza la cadena resultante para obtener la clave de normalización.
    /// </summary>
    /// <param name="input">Entrada a canonicalizar.</param>
    /// <returns>Clave canonicalizada.</returns>
    private static string Canonicalize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        string normalized = input.ToLowerInvariant();
        normalized = normalized.Normalize(NormalizationForm.FormD);

        StringBuilder builder = new(normalized.Length);
        foreach (char character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                builder.Append('-');
                continue;
            }

            if (char.IsLetterOrDigit(character) || character == '-')
            {
                builder.Append(character);
            }
        }

        string result = builder.ToString();
        while (result.Contains("--", StringComparison.Ordinal))
        {
            result = result.Replace("--", "-", StringComparison.Ordinal);
        }

        return result.Trim('-');
    }
}


