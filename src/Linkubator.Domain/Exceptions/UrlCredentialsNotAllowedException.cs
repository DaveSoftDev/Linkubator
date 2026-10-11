namespace Linkubator.Domain.Exceptions;

public sealed class UrlCredentialsNotAllowedException : DomainException
{
    public UrlCredentialsNotAllowedException()
        : base("UrlCredentialsNotAllowed")
    {
    }
}
