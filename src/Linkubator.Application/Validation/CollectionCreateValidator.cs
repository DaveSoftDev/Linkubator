using Linkubator.Domain.Constants;
using Linkubator.Domain.Policies;

namespace Linkubator.Application;

public sealed class CollectionCreateValidator : IValidator<CreateCollectionRequest>
{
    public Result Validate(CreateCollectionRequest request)
    {
        List<ApplicationError> errors = [];

        if (request is null)
        {
            return Result.Failure(ApplicationError.Validation("REQUEST_REQUIRED", "request", "La solicitud es obligatoria."));
        }

        string trimmedName = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            errors.Add(ApplicationError.Validation("COLLECTION_NAME_REQUIRED", "Name", "El nombre es obligatorio."));
        }
        else if (UserTextPolicy.CountCodePoints(trimmedName) > LengthLimitsConstants.CollectionName)
        {
            errors.Add(ApplicationError.Validation("COLLECTION_NAME_TOO_LONG", "Name", "El nombre supera la longitud máxima."));
        }

        if (!string.IsNullOrWhiteSpace(request.Description) &&
            UserTextPolicy.CountCodePoints(request.Description.Trim()) > LengthLimitsConstants.CollectionDescription)
        {
            errors.Add(ApplicationError.Validation("COLLECTION_DESCRIPTION_TOO_LONG", "Description", "La descripción supera la longitud máxima."));
        }

        return errors.Count == 0 ? Result.Success() : Result.Failure(errors);
    }
}
