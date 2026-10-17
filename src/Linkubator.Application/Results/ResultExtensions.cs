using Linkubator.Domain.Exceptions;

namespace Linkubator.Application.Results;

public static class ResultExtensions
{
    public static Result<T> FailDomain<T>(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return Result<T>.Failure(ApplicationError.FromDomainException((DomainException)exception));
    }

    public static Result FailDomain(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return Result.Failure(ApplicationError.FromDomainException((DomainException)exception));
    }
}
