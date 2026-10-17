using Linkubator.Application.Contracts;

namespace Linkubator.Application.Validation;

public interface IValidator<in TRequest>
{
    Result Validate(TRequest request);
}
