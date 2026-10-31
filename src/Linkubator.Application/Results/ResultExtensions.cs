using Linkubator.Domain.Exceptions;

namespace Linkubator.Application.Results;

public static class ResultExtensions
{
    /// <summary>
    /// Convierte una excepción de dominio en un resultado fallido tipado.
    /// </summary>
    public static Result<T> FailDomain<T>(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return Result<T>.Failure(ApplicationError.FromDomainException((DomainException)exception));
    }

    /// <summary>
    /// Convierte una excepción de dominio en un resultado fallido tipado.
    /// </summary>
    public static Result FailDomain(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return Result.Failure(ApplicationError.FromDomainException((DomainException)exception));
    }
}
