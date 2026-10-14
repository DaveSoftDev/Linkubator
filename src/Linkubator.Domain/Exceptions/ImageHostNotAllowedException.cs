namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el host de la URL de imagen no está permitido.
/// </summary>
public sealed class ImageHostNotAllowedException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ImageHostNotAllowedException"/>.
    /// </summary>
    public ImageHostNotAllowedException()
        : base("ImageHostNotAllowed")
    {
    }
}
