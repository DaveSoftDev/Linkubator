namespace Linkubator.Application;

public class Result<T> : Result
{
    private Result(bool isSuccess, T? value, IEnumerable<ApplicationError>? errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public bool TryGetValue(out T? value)
    {
        value = Value;
        return IsSuccess;
    }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<ApplicationError>());

    public new static Result<T> Failure(params ApplicationError[] errors) => new(false, default, errors);

    public new static Result<T> Failure(IEnumerable<ApplicationError> errors) => new(false, default, errors);
}
