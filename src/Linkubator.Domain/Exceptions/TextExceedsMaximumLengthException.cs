namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el texto supera la longitud máxima permitida.
/// </summary>
public sealed class TextExceedsMaximumLengthException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="TextExceedsMaximumLengthException"/>.
    /// </summary>
    public TextExceedsMaximumLengthException()
        : base("TextExceedsMaximumLength")
    {
    }
}