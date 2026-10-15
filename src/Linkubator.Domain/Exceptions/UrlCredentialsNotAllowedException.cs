namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que las credenciales no están permitidas en la URL.
/// </summary>
public sealed class UrlCredentialsNotAllowedException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UrlCredentialsNotAllowedException"/>.
    /// </summary>
    public UrlCredentialsNotAllowedException()
        : base("UrlCredentialsNotAllowed")
    {
    }
}
