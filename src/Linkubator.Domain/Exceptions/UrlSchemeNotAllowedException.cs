namespace Linkubator.Domain.Exceptions;

public sealed class UrlSchemeNotAllowedException : DomainException
{
    public UrlSchemeNotAllowedException()
        : base("UrlSchemeNotAllowed")
    {
    }
}
