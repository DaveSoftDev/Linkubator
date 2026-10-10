namespace Linkubator.Domain.Exceptions;

public sealed class TextExceedsMaximumLengthException : DomainException
{
    public TextExceedsMaximumLengthException()
        : base("TextExceedsMaximumLength")
    {
    }
}