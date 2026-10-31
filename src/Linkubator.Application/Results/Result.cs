namespace Linkubator.Application.Results;

public class Result
{
    /// <summary>
    /// Inicializa Result con las dependencias necesarias.
    /// </summary>
    protected Result(bool isSuccess, IEnumerable<ApplicationError>? errors = null)
    {
        ApplicationError[] materializedErrors = (errors ?? Array.Empty<ApplicationError>())
            .Where(error => error is not null)
            .ToArray();

        if (isSuccess && materializedErrors.Length != 0)
        {
            throw new ArgumentException("A successful result cannot contain errors.", nameof(errors));
        }

        if (!isSuccess && materializedErrors.Length == 0)
        {
            throw new ArgumentException("A failed result must contain at least one error.", nameof(errors));
        }

        IsSuccess = isSuccess;
        Errors = Array.AsReadOnly(materializedErrors);
    }

    public bool IsSuccess { get; }

    public IReadOnlyList<ApplicationError> Errors { get; }

    /// <summary>
    /// Crea un resultado satisfactorio.
    /// </summary>
    public static Result Success() => new(true, Array.Empty<ApplicationError>());

    /// <summary>
    /// Registra el rechazo de la operación y crea un resultado fallido con el error indicado.
    /// </summary>
    public static Result Failure(params ApplicationError[] errors) => new(false, errors);

    /// <summary>
    /// Registra el rechazo de la operación y crea un resultado fallido con el error indicado.
    /// </summary>
    public static Result Failure(IEnumerable<ApplicationError> errors) => new(false, errors);
}
