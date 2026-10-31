using Linkubator.Domain.Exceptions;

namespace Linkubator.Application.Results;

public sealed class ApplicationError
{
    /// <summary>
    /// Inicializa ApplicationError con las dependencias necesarias.
    /// </summary>
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

    /// <summary>
    /// Crea un error de validación.
    /// </summary>
    public static ApplicationError Validation(string code, string? field = null, string? message = null)
        => new(code, ApplicationErrorCategory.Validation, field, message);

    /// <summary>
    /// Crea un error que representa una regla de dominio.
    /// </summary>
    public static ApplicationError Domain(string code, string? field = null, string? message = null)
        => new(code, ApplicationErrorCategory.DomainRule, field, message);

    /// <summary>
    /// Crea un error que representa un recurso no accesible.
    /// </summary>
    public static ApplicationError ResourceNotAccessible(string code, string? field = null, string? message = null)
        => new(code, ApplicationErrorCategory.ResourceNotAccessible, field, message);

    /// <summary>
    /// Convierte una excepción de dominio en un error de aplicación.
    /// </summary>
    public static ApplicationError FromDomainException(DomainException exception, string? field = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new ApplicationError(exception.Code, ApplicationErrorCategory.DomainRule, field, exception.Message);
    }
}
