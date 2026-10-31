namespace Linkubator.Application.Results;

public class Result<T> : Result
{
    /// <summary>
    /// Inicializa Result con las dependencias necesarias.
    /// </summary>
    private Result(bool isSuccess, T? value, IEnumerable<ApplicationError>? errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    /// <summary>
    /// Intenta obtener el valor del resultado y devuelve si la operación fue satisfactoria.
    /// </summary>
    public bool TryGetValue(out T? value)
    {
        value = Value;
        return IsSuccess;
    }

    /// <summary>
    /// Crea un resultado satisfactorio.
    /// </summary>
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(true, value, Array.Empty<ApplicationError>());
    }

    /// <summary>
    /// Registra el rechazo de la operación y crea un resultado fallido con el error indicado.
    /// </summary>
    public new static Result<T> Failure(params ApplicationError[] errors) => new(false, default, errors);

    /// <summary>
    /// Registra el rechazo de la operación y crea un resultado fallido con el error indicado.
    /// </summary>
    public new static Result<T> Failure(IEnumerable<ApplicationError> errors) => new(false, default, errors);
}
