namespace Linkubator.Domain.Exceptions;

public sealed class UrlHostInvalidException : DomainException
{
    public UrlHostInvalidException()
        : base("UrlHostInvalid")
    {
    }
}
