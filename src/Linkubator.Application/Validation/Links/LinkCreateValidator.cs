using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Links;

public sealed class LinkCreateValidator : IValidator<LinkCreateRequest>
{
    public Result Validate(LinkCreateRequest request)
    {
        if (request is null)
        {
            return Result.Failure(LinkErrors.RequestRequired());
        }

        Result result = LinkValidations.ValidateCollectionId(request.CollectionId);
        if (!result.IsSuccess)
        {
            return Result.Failure(LinkErrors.CollectionIdInvalid());
        }

        result = LinkValidations.ValidateUrl(request.Url);
        if (!result.IsSuccess)
        {
            return Result.Failure(LinkErrors.UrlRequired());
        }

        return Result.Success();
    }
}
