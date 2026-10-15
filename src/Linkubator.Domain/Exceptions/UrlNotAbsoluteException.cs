namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que la URL no es absoluta.
/// </summary>
public sealed class UrlNotAbsoluteException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UrlNotAbsoluteException"/>.
    /// </summary>
    public UrlNotAbsoluteException()
        : base("UrlNotAbsolute")
    {
    }
}
