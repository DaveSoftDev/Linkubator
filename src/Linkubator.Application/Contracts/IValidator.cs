namespace Linkubator.Application.Contracts;

public interface IValidator<in TRequest>
{
    Result Validate(TRequest? request);
}
