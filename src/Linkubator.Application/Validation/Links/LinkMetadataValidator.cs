using Linkubator.Application.Results;
using Linkubator.Application.Dtos.Requests;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using LengthLimits = Linkubator.Domain.Constants.LengthLimitsConstants;

namespace Linkubator.Application.Validation.Links;

internal static class LinkMetadataValidator
{
    public static Result Validate(string? title, string? description, string? image, string baseUrl)
    {
        Result titleResult = ValidateField(nameof(LinkUpdateRequest.Title), () => DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(title, LengthLimits.LinkTitle));
        if (!titleResult.IsSuccess)
        {
            return titleResult;
        }

        Result descriptionResult = ValidateField(nameof(LinkUpdateRequest.Description), () => DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(description, LengthLimits.LinkDescription));
        if (!descriptionResult.IsSuccess)
        {
            return descriptionResult;
        }

        return ValidateField(nameof(LinkUpdateRequest.Image), () =>
        {
            string? adjustedImage = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(image, LengthLimits.LinkImage);
            if (adjustedImage is not null)
            {
                DomainPolicies.ImageUrlPolicy.Adjust(adjustedImage, baseUrl);
            }
        });
    }

    private static Result ValidateField(string field, Action validate)
    {
        try
        {
            validate();
            return Result.Success();
        }
        catch (DomainExceptions.DomainException exception)
        {
            return Result.Failure(ApplicationError.FromDomainException(exception, field));
        }
    }
}
