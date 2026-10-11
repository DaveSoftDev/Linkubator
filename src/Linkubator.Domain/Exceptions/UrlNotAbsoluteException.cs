namespace Linkubator.Domain.Exceptions;

public sealed class UrlNotAbsoluteException : DomainException
{
    public UrlNotAbsoluteException()
        : base("UrlNotAbsolute")
    {
    }
}
