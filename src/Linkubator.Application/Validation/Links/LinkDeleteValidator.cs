using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Links;

public sealed class LinkDeleteValidator : IValidator<LinkDeleteRequest>
{
    public Result Validate(LinkDeleteRequest request)
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

        return Result.Success();
    }
}
