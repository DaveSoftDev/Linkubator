namespace Linkubator.Domain.Exceptions;

public sealed class TextBelowMinimumLengthException : DomainException
{
    public TextBelowMinimumLengthException()
        : base("TextBelowMinimumLength")
    {
    }
}
