using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Collections;

public sealed class CollectionRenameValidator : IValidator<CollectionRenameRequest>
{
    public Result Validate(CollectionRenameRequest request)
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

        result = CollectionValidations.ValidateName(request.Name);
        if (!result.IsSuccess)
        {
            return Result.Failure(CollectionErrors.NameRequired());
        }

        return Result.Success();
    }
}
