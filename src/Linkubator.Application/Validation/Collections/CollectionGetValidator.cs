using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Collections;

public sealed class CollectionGetValidator : IValidator<CollectionGetRequest>
{
    public Result Validate(CollectionGetRequest? request)
    {
        if (request is null)
        {
            return Result.Failure(CollectionErrors.RequestRequired());
        }

        Result result = CollectionValidations.ValidateCollectionId(request.CollectionId);
        if (!result.IsSuccess)
        {
            return Result.Failure(CollectionErrors.CollectionIdInvalid());
        }

        return Result.Success();
    }
}
