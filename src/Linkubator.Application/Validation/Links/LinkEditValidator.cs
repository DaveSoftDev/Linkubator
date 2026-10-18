using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Links;

public sealed class LinkEditValidator : IValidator<LinkEditRequest>
{
    public Result Validate(LinkEditRequest request)
    {
        if (request is null)
        {
            return Result.Failure(LinkErrors.RequestRequired());
        }

        Result result = LinkValidations.ValidateLinkId(request.LinkId);
        if (!result.IsSuccess)
        {
            return Result.Failure(LinkErrors.LinkIdInvalid());
        }

        result = LinkValidations.ValidateCollectionId(request.CollectionId);
        if (!result.IsSuccess)
        {
            return Result.Failure(LinkErrors.CollectionIdInvalid());
        }

        return Result.Success();
    }
}
