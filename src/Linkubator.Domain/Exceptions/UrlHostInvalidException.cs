namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el host de la URL no es válido.
/// </summary>
public sealed class UrlHostInvalidException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UrlHostInvalidException"/>.
    /// </summary>
    public UrlHostInvalidException()
        : base("UrlHostInvalid")
    {
    }
}
