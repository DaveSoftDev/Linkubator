using Linkubator.Application.Constants;

namespace Linkubator.Application.Validation.Collections;

public static class CollectionValidations
{
    private static string NormalizeName(string? value) => value?.Trim() ?? string.Empty;

    public static Result ValidateCollectionId(long collectionId)
    {
        if (collectionId <= 0)
        {
            return Result.Failure(CollectionErrors.CollectionIdInvalid());
        }

        return Result.Success();
    }

    public static Result ValidateName(string? value)
    {
        string trimmedValue = NormalizeName(value);

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return Result.Failure(CollectionErrors.NameRequired());
        }

        return Result.Success();
    }

    public static Result ValidateDescription(string? value)
    {
        return value is not null && !Linkubator.Domain.Policies.UserTextPolicy.IsWithinMaximumLength(value.Trim(), Linkubator.Domain.Constants.LengthLimitsConstants.CollectionDescription)
            ? Result.Failure(CollectionErrors.DescriptionTooLong())
            : Result.Success();
    }
}
