using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using LengthLimits = Linkubator.Domain.Constants.LengthLimitsConstants;

namespace Linkubator.Application.Validation.Links;

internal static class LinkMetadataValidator
{
    public static Result Validate(LinkMetadataValidationRequest request)
    {
        Result titleResult = ValidateField(nameof(LinkUpdateRequest.Title), () => DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(request.Title, LengthLimits.LinkTitle));
        if (!titleResult.IsSuccess)
        {
            return titleResult;
        }

        Result descriptionResult = ValidateField(nameof(LinkUpdateRequest.Description), () => DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(request.Description, LengthLimits.LinkDescription));
        if (!descriptionResult.IsSuccess)
        {
            return descriptionResult;
        }

        return ValidateField(nameof(LinkUpdateRequest.Image), () =>
        {
            string? adjustedImage = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(request.Image, LengthLimits.LinkImage);
            if (adjustedImage is not null)
            {
                DomainPolicies.ImageUrlPolicy.Adjust(adjustedImage, request.BaseUrl);
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
