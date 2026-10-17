using Linkubator.Domain.Exceptions;

namespace Linkubator.Application.Results;

public sealed class ApplicationError
{
    public ApplicationError(string code, ApplicationErrorCategory category, string? field = null, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("A code is required.", nameof(code));
        }

        Code = code;
        Category = category;
        Field = field;
        Message = message;
    }

    public string Code { get; }

    public ApplicationErrorCategory Category { get; }

    public string? Field { get; }

    public string? Message { get; }

    public static ApplicationError Validation(string code, string? field = null, string? message = null)
        => new(code, ApplicationErrorCategory.Validation, field, message);

    public static ApplicationError Domain(string code, string? field = null, string? message = null)
        => new(code, ApplicationErrorCategory.DomainRule, field, message);

    public static ApplicationError ResourceNotAccessible(string code, string? field = null, string? message = null)
        => new(code, ApplicationErrorCategory.ResourceNotAccessible, field, message);

    public static ApplicationError FromDomainException(DomainException exception, string? field = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new ApplicationError(exception.Code, ApplicationErrorCategory.DomainRule, field, exception.Message);
    }
}
