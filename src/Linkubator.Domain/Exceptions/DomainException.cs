namespace Linkubator.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public string Code { get; }

    protected DomainException(string code)
    {
        Code = code;
    }
}