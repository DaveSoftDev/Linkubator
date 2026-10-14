namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el esquema de la URL no está permitido.
/// </summary>
public sealed class UrlSchemeNotAllowedException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UrlSchemeNotAllowedException"/>.
    /// </summary>
    public UrlSchemeNotAllowedException()
        : base("UrlSchemeNotAllowed")
    {
    }
}
