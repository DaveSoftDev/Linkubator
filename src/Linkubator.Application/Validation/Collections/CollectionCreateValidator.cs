using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Collections;

public sealed class CollectionCreateValidator : IValidator<CollectionCreateRequest>
{
    public Result Validate(CollectionCreateRequest? request)
    {
        if (request is null)
        {
            return Result.Failure(CollectionErrors.RequestRequired());
        }

        Result result = CollectionValidations.ValidateName(request.Name);
        if (!result.IsSuccess)
        {
            return Result.Failure(CollectionErrors.NameRequired());
        }

        result = CollectionValidations.ValidateNameLength(request.Name);
        if (!result.IsSuccess)
        {
            return Result.Failure(CollectionErrors.NameTooLong());
        }

        result = CollectionValidations.ValidateDescription(request.Description);
        if (!result.IsSuccess)
        {
            return result;
        }

        return Result.Success();
    }
}
