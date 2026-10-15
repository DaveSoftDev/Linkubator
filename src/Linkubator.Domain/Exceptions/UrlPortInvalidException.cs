namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el puerto de la URL no es válido.
/// </summary>
public sealed class UrlPortInvalidException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UrlPortInvalidException"/>.
    /// </summary>
    public UrlPortInvalidException()
        : base("UrlPortInvalid")
    {
    }
}
