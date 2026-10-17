using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Domain.Constants;
using Linkubator.Domain.Policies;

namespace Linkubator.Application.Validation;

public sealed class LinkCreateValidator : IValidator<CreateLinkRequest>
{
    public Result Validate(CreateLinkRequest request)
    {
        if (request is null)
        {
            return Result.Failure(LinkErrors.RequestRequired());
        }

        List<ApplicationError> errors = [];
        errors.AddRange(ValidateCollectionId(request.CollectionId));
        errors.AddRange(ValidateUrl(request.Url));
        errors.AddRange(ValidateOptionalText(request.Title, LengthLimitsConstants.LinkTitle, LinkErrors.TitleTooLong));
        errors.AddRange(ValidateOptionalText(request.Description, LengthLimitsConstants.LinkDescription, LinkErrors.DescriptionTooLong));
        errors.AddRange(ValidateImage(request.Image));

        return errors.Count == 0 ? Result.Success() : Result.Failure(errors);
    }

    private static List<ApplicationError> ValidateCollectionId(long collectionId)
    {
        if (collectionId <= 0)
        {
            return [LinkErrors.CollectionIdInvalid()];
        }

        return [];
    }

    private static List<ApplicationError> ValidateUrl(string? url)
    {
        string trimmedUrl = url?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedUrl))
        {
            return [LinkErrors.UrlRequired()];
        }

        if (UserTextPolicy.CountCodePoints(trimmedUrl) > LengthLimitsConstants.LinkUrl)
        {
            return [LinkErrors.UrlTooLong()];
        }

        return [];
    }

    private static List<ApplicationError> ValidateOptionalText(string? value, int maxLength, Func<ApplicationError> errorFactory)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        if (UserTextPolicy.CountCodePoints(value.Trim()) > maxLength)
        {
            return [errorFactory()];
        }

        return [];
    }

    private static List<ApplicationError> ValidateImage(string? image)
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            return [];
        }

        string trimmedImage = image.Trim();
        if (UserTextPolicy.CountCodePoints(trimmedImage) > LengthLimitsConstants.LinkImage)
        {
            return [LinkErrors.ImageTooLong()];
        }

        bool isAbsoluteHttpOrHttps = Uri.TryCreate(trimmedImage, UriKind.Absolute, out var absoluteImage)
            && (absoluteImage.Scheme == Uri.UriSchemeHttps || absoluteImage.Scheme == Uri.UriSchemeHttp);
        bool isRelativeReference = Uri.TryCreate(trimmedImage, UriKind.Relative, out _)
            || trimmedImage.StartsWith("//", StringComparison.Ordinal)
            || trimmedImage.StartsWith("/", StringComparison.Ordinal)
            || trimmedImage.StartsWith("./", StringComparison.Ordinal)
            || trimmedImage.StartsWith("../", StringComparison.Ordinal);

        if (!isAbsoluteHttpOrHttps && !isRelativeReference)
        {
            return [LinkErrors.ImageUrlInvalid()];
        }

        return [];
    }
}
