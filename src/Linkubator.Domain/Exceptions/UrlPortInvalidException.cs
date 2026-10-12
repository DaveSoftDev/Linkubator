namespace Linkubator.Domain.Exceptions;

public sealed class UrlPortInvalidException : DomainException
{
    public UrlPortInvalidException()
        : base("UrlPortInvalid")
    {
    }
}
