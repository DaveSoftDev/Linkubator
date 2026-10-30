namespace Linkubator.Application.Results;

public class Result
{
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

    public static Result Success() => new(true, Array.Empty<ApplicationError>());

    public static Result Failure(params ApplicationError[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<ApplicationError> errors) => new(false, errors);
}
