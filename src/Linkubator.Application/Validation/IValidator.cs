namespace Linkubator.Application;

public interface IValidator<in TRequest>
{
    Result Validate(TRequest request);
}
