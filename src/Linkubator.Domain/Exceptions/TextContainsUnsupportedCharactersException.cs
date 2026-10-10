namespace Linkubator.Domain.Exceptions;

public sealed class TextContainsUnsupportedCharactersException : DomainException
{
    public TextContainsUnsupportedCharactersException()
        : base("TextContainsUnsupportedCharacters")
    {
    }
}
