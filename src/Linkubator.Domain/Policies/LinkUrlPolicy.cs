using DomainExceptions = Linkubator.Domain.Exceptions;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política para ajustar URLs de enlaces.
/// </summary>
public static class LinkUrlPolicy
{
    /// <summary>
    /// Ajusta la URL de enlace aplicando las reglas de normalización y validación.
    /// </summary>
    /// <param name="input">URL a ajustar.</param>
    /// <returns>URL ajustada.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si la URL es nula, vacía o no es válida.</exception>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si la URL supera la longitud máxima.</exception>
    /// <exception cref="DomainExceptions.TextContainsUnsupportedCharactersException">Si la URL contiene caracteres no soportados.</exception>
    /// <exception cref="DomainExceptions.UrlSchemeNotAllowedException">Si el esquema no está permitido.</exception>
    /// <exception cref="DomainExceptions.UrlAuthorityMissingException">Si falta la autoridad.</exception>
    /// <exception cref="DomainExceptions.UrlHostInvalidException">Si el host no es válido.</exception>
    /// <exception cref="DomainExceptions.UrlPortInvalidException">Si el puerto no es válido.</exception>
    public static string Adjust(string input)
    {
        string trimmedInput = UrlInputPolicy.Prepare(input, LengthLimitsConstants.LinkUrl);

        if (trimmedInput.StartsWith("//", StringComparison.Ordinal))
        {
            return UserTextPolicy.EnsureMaximumLength("https://" + trimmedInput[2..], LengthLimitsConstants.LinkUrl);
        }

        UriAuthority? authority = UriAuthorityPolicy.Analyze(trimmedInput);

        if (authority is not null && authority.HasExplicitScheme)
        {
            if (!UriAuthorityPolicy.IsAllowedScheme(authority.Scheme))
            {
                throw new DomainExceptions.UrlSchemeNotAllowedException();
            }
            if (!UriAuthorityPolicy.IsValidAuthority(authority))
            {
                throw AuthorityRejection(authority);
            }

            return UserTextPolicy.EnsureMaximumLength(trimmedInput, LengthLimitsConstants.LinkUrl);
        }

        if (authority is not null && UriAuthorityPolicy.IsValidAuthority(authority) && authority.Host is string host)
        {
            return UserTextPolicy.EnsureMaximumLength(AddDefaultScheme(trimmedInput, host), LengthLimitsConstants.LinkUrl);
        }

        throw AuthorityRejection(authority);
    }

    /// <summary>
    /// Determina el tipo de rechazo de autoridad según la causa.
    /// </summary>
    /// <param name="authority">Autoridad analizada.</param>
    /// <returns>Excepción de dominio correspondiente al tipo de rechazo.</returns>
    private static DomainExceptions.DomainException AuthorityRejection(UriAuthority? authority)
    {
        if (authority is null || authority.Host is null)
        {
            return new DomainExceptions.UrlAuthorityMissingException();
        }

        if (!UriAuthorityPolicy.IsValidHost(authority.Host))
        {
            return new DomainExceptions.UrlHostInvalidException();
        }

        return new DomainExceptions.UrlPortInvalidException();
    }

    /// <summary>
    /// Añade el esquema por defecto según el host.
    /// </summary>
    /// <param name="input">Entrada sin esquema.</param>
    /// <param name="host">Host identificado.</param>
    /// <returns>Entrada con esquema añadido.</returns>
    private static string AddDefaultScheme(string input, string host)
    {
        string scheme = string.Equals(host, UrlConstants.LocalhostHost, StringComparison.OrdinalIgnoreCase) ? "http://" : "https://";
        return scheme + input;
    }
}