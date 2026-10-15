namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el texto contiene caracteres no soportados.
/// </summary>
public sealed class TextContainsUnsupportedCharactersException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="TextContainsUnsupportedCharactersException"/>.
    /// </summary>
    public TextContainsUnsupportedCharactersException()
        : base("TextContainsUnsupportedCharacters")
    {
    }
}
