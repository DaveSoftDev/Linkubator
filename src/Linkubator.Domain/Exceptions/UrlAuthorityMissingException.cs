namespace Linkubator.Domain.Exceptions;

public sealed class UrlAuthorityMissingException : DomainException
{
    public UrlAuthorityMissingException()
        : base("UrlAuthorityMissing")
    {
    }
}
