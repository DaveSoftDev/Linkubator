namespace Linkubator.Application;

public class Result
{
    protected Result(bool isSuccess, IEnumerable<ApplicationError>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors = (errors ?? Array.Empty<ApplicationError>())
            .Where(error => error is not null)
            .ToArray();
    }

    public bool IsSuccess { get; }

    public IReadOnlyList<ApplicationError> Errors { get; }

    public static Result Success() => new(true, Array.Empty<ApplicationError>());

    public static Result Failure(params ApplicationError[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<ApplicationError> errors) => new(false, errors);
}
