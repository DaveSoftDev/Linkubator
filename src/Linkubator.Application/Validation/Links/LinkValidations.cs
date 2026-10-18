using Linkubator.Application.Constants;

namespace Linkubator.Application.Validation.Links;

public static class LinkValidations
{
    private static string NormalizeName(string? value) => value?.Trim() ?? string.Empty;

    public static Result ValidateLinkId(long linkId)
    {
        if (linkId <= 0)
        {
            return Result.Failure(LinkErrors.LinkIdInvalid());
        }

        return Result.Success();
    }

    public static Result ValidateCollectionId(long collectionId)
    {
        if (collectionId <= 0)
        {
            return Result.Failure(LinkErrors.CollectionIdInvalid());
        }

        return Result.Success();
    }

    public static Result ValidateUrl(string? value)
    {
        string trimmedValue = NormalizeName(value);

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return Result.Failure(LinkErrors.UrlRequired());
        }

        return Result.Success();
    }
}
