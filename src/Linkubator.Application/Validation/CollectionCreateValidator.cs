using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Domain.Constants;
using Linkubator.Domain.Policies;

namespace Linkubator.Application.Validation;

public sealed class CollectionCreateValidator : IValidator<CreateCollectionRequest>
{
    public Result Validate(CreateCollectionRequest request)
    {
        List<ApplicationError> errors = [];

        if (request is null)
        {
            return Result.Failure(CollectionErrors.RequestRequired());
        }

        string trimmedName = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            errors.Add(CollectionErrors.NameRequired());
        }
        else if (UserTextPolicy.CountCodePoints(trimmedName) > LengthLimitsConstants.CollectionName)
        {
            errors.Add(CollectionErrors.NameTooLong());
        }

        if (!string.IsNullOrWhiteSpace(request.Description) &&
            UserTextPolicy.CountCodePoints(request.Description.Trim()) > LengthLimitsConstants.CollectionDescription)
        {
            errors.Add(CollectionErrors.DescriptionTooLong());
        }

        return errors.Count == 0 ? Result.Success() : Result.Failure(errors);
    }
}
