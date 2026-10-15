using System.Net;
using DomainExceptions = Linkubator.Domain.Exceptions;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política para ajustar y validar URLs de imágenes.
/// </summary>
public static class ImageUrlPolicy
{
    /// <summary>
    /// Ajusta la URL de imagen aplicando las reglas de normalización y validación.
    /// </summary>
    /// <param name="input">URL de imagen a ajustar.</param>
    /// <param name="baseUrl">URL base para resolver referencias relativas.</param>
    /// <returns>URL de imagen ajustada y validada.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si los parámetros son nulos, vacíos o no son válidos.</exception>
    /// <exception cref="DomainExceptions.UrlSchemeNotAllowedException">Si el esquema no está permitido.</exception>
    /// <exception cref="DomainExceptions.UrlHostInvalidException">Si el host no es válido.</exception>
    /// <exception cref="DomainExceptions.ImageHostNotAllowedException">Si el host de la imagen no está permitido.</exception>
    /// <exception cref="DomainExceptions.UrlCredentialsNotAllowedException">Si se incluyen credenciales no permitidas.</exception>
    /// <exception cref="DomainExceptions.UrlPortInvalidException">Si el puerto no es válido.</exception>
    /// <exception cref="DomainExceptions.UrlNotAbsoluteException">Si la URL no es absoluta.</exception>
    public static string Adjust(string input, string baseUrl)
    {
        UserTextPolicy.ThrowIfNullOrWhiteSpace(input);
        UserTextPolicy.ThrowIfNullOrWhiteSpace(baseUrl);

        string trimmedInput = UrlInputPolicy.Prepare(input);

        string candidate = trimmedInput;
        UriAuthority? authority = UriAuthorityPolicy.Analyze(candidate);
        string? scheme = authority?.Scheme;

        if (candidate.StartsWith("//", StringComparison.Ordinal))
        {
            candidate = "https:" + candidate;
        }
        else if (scheme is not null)
        {
            if (!UriAuthorityPolicy.IsAllowedScheme(scheme))
            {
                throw new DomainExceptions.UrlSchemeNotAllowedException();
            }
        }
        else if (UriAuthorityPolicy.IsValidAuthority(authority) && UriAuthorityPolicy.IsHostAndPortForm(authority))
        {
            candidate = "https://" + candidate;
        }
        else
        {
            candidate = ResolveRelative(candidate, baseUrl);
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri? absoluteUri))
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        if (absoluteUri.Scheme.Equals(UrlConstants.HttpScheme, StringComparison.OrdinalIgnoreCase))
        {
            candidate = ReplaceHttpWithHttps(candidate, absoluteUri);
        }

        RejectNotAllowedImageHost(absoluteUri);

        string adjustedUrl = UserTextPolicy.EnsureMaximumLength(candidate, LengthLimitsConstants.LinkImage);
        UrlValidationPolicy.Validate(adjustedUrl);

        return adjustedUrl;
    }

    /// <summary>
    /// Rechaza los hosts que el ajuste de Image no admite, según
    /// specifications.md → «Ajuste de Image»: localhost y las direcciones IP.
    /// </summary>
    /// <param name="absoluteUri">URI absoluta de la imagen.</param>
    /// <exception cref="DomainExceptions.ImageHostNotAllowedException">Si el host no está permitido.</exception>
    private static void RejectNotAllowedImageHost(Uri absoluteUri)
    {
        // IdnHost devuelve el host sin corchetes en IPv6, igual que UrlValidationPolicy al validar,
        // así que la comprobación de direcciones IP no depende de cómo se haya escrito la autoridad.
        string host = absoluteUri.IdnHost;

        if (string.Equals(host, UrlConstants.LocalhostHost, StringComparison.OrdinalIgnoreCase)
            || IPAddress.TryParse(host, out _))
        {
            throw new DomainExceptions.ImageHostNotAllowedException();
        }
    }

    /// <summary>
    /// Cambia el esquema http por https sobre la URL ajustada, sin reserializarla, y
    /// descarta el puerto 80 cuando está escrito. Conserva la ruta, la consulta, el
    /// fragmento y las codificaciones porcentuales tal como quedaron tras el ajuste.
    /// </summary>
    /// <param name="url">URL a modificar.</param>
    /// <param name="uri">Instancia de URI correspondiente.</param>
    /// <returns>URL con esquema https.</returns>
    private static string ReplaceHttpWithHttps(string url, Uri uri)
    {
        const string httpScheme = "http://";
        const string defaultPort = ":80";

        if (!url.StartsWith(httpScheme, StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        int authorityStart = httpScheme.Length;
        int authorityEnd = url.IndexOfAny(['/', '?', '#'], authorityStart);
        if (authorityEnd < 0)
        {
            authorityEnd = url.Length;
        }

        string authority = url[authorityStart..authorityEnd];
        if (uri.IsDefaultPort && authority.EndsWith(defaultPort, StringComparison.Ordinal))
        {
            url = url[..authorityStart] + authority[..^defaultPort.Length] + url[authorityEnd..];
        }

        return "https://" + url[authorityStart..];
    }

    /// <summary>
    /// Resuelve una referencia relativa contra la base con las reglas estándar de resolución URI.
    /// </summary>
    /// <param name="input">Referencia relativa a resolver.</param>
    /// <param name="baseUrl">URL base absoluta.</param>
    /// <returns>URI resultante en formato absoluto.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si la URL base es nula, vacía o no válida.</exception>
    /// <exception cref="DomainExceptions.UrlHostInvalidException">Si la resolución falla o la URL base no es válida.</exception>
    private static string ResolveRelative(string input, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new DomainExceptions.NotValidInputException();
        }

        Uri baseUri;
        try
        {
            baseUri = new Uri(baseUrl.Trim(), UriKind.Absolute);
        }
        catch (UriFormatException)
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        try
        {
            return new Uri(baseUri, input).AbsoluteUri;
        }
        catch (UriFormatException)
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }
    }
}