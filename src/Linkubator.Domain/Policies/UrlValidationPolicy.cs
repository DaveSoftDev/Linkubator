using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política para validación de URLs de enlaces.
/// </summary>
public static class UrlValidationPolicy
{
    /// <summary>
    /// Valida que la URL cumple con las restricciones requeridas.
    /// </summary>
    /// <param name="input">URL a validar.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si la URL es nula, vacía o solo espacios.</exception>
    /// <exception cref="DomainExceptions.UrlNotAbsoluteException">Si la URL no es absoluta.</exception>
    /// <exception cref="DomainExceptions.UrlSchemeNotAllowedException">Si el esquema no está permitido.</exception>
    /// <exception cref="DomainExceptions.UrlHostInvalidException">Si el host no es válido.</exception>
    /// <exception cref="DomainExceptions.UrlCredentialsNotAllowedException">Si se incluyen credenciales en la URL.</exception>
    /// <exception cref="DomainExceptions.UrlPortInvalidException">Si el puerto no es válido.</exception>
    public static void Validate(string input)
    {
        UserTextPolicy.ThrowIfNullOrWhiteSpace(input);

        if (!Uri.TryCreate(input, UriKind.Absolute, out Uri? uri) || uri is null)
        {
            throw new DomainExceptions.UrlNotAbsoluteException();
        }

        if (!UriAuthorityPolicy.IsAllowedScheme(uri.Scheme))
        {
            throw new DomainExceptions.UrlSchemeNotAllowedException();
        }

        string host = uri.IdnHost;
        if (uri.HostNameType == UriHostNameType.IPv4
            && (!UriAuthorityPolicy.TryExtractHost(input, out string? writtenHost)
                || writtenHost is null
                || !UriAuthorityPolicy.IsValidHost(writtenHost)))
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        if (!UriAuthorityPolicy.IsValidHost(host))
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new DomainExceptions.UrlCredentialsNotAllowedException();
        }

        if (HasInvalidPort(input))
        {
            throw new DomainExceptions.UrlPortInvalidException();
        }
    }

    /// <summary>
    /// Determina si el puerto en la URL es inválido.
    /// </summary>
    /// <param name="input">URL a analizar.</param>
    /// <returns><c>true</c> si el puerto es inválido; de lo contrario, <c>false</c>.</returns>
    private static bool HasInvalidPort(string input)
    {
        if (UriAuthorityPolicy.Analyze(input) is not { } authority)
        {
            return false;
        }

        return authority.Port is string portText && !UriAuthorityPolicy.IsValidPort(portText);
    }
}