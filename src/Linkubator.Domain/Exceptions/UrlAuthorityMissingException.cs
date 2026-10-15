namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que falta la autoridad en la URL.
/// </summary>
public sealed class UrlAuthorityMissingException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UrlAuthorityMissingException"/>.
    /// </summary>
    public UrlAuthorityMissingException()
        : base("UrlAuthorityMissing")
    {
    }
}
