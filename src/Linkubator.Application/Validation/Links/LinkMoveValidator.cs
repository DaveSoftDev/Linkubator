using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Links;

public sealed class LinkMoveValidator : IValidator<LinkMoveRequest>
{
    /// <summary>
    /// Valida la solicitud indicada y devuelve los errores detectados.
    /// </summary>
    public Result Validate(LinkMoveRequest? request)
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

        result = LinkValidations.ValidateCollectionId(request.DestinationCollectionId);
        if (!result.IsSuccess)
        {
            return Result.Failure(LinkErrors.DestinationCollectionIdInvalid());
        }

        return Result.Success();
    }
}
