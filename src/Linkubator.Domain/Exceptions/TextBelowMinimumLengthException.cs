namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el texto tiene una longitud inferior al mínimo permitido.
/// </summary>
public sealed class TextBelowMinimumLengthException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="TextBelowMinimumLengthException"/>.
    /// </summary>
    public TextBelowMinimumLengthException()
        : base("TextBelowMinimumLength")
    {
    }
}
