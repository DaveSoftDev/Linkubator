namespace Linkubator.Domain.Exceptions;

public sealed class ImageHostNotAllowedException : DomainException
{
    public ImageHostNotAllowedException()
        : base("ImageHostNotAllowed")
    {
    }
}
