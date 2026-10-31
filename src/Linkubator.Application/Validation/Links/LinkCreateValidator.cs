using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Application.Validation.Links;

public sealed class LinkCreateValidator : IValidator<LinkCreateRequest>
{
    /// <summary>
    /// Valida la solicitud indicada y devuelve los errores detectados.
    /// </summary>
    public Result Validate(LinkCreateRequest? request)
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

        string adjustedUrl = string.Empty;
        Result urlResult = ValidateField(nameof(request.Url), () =>
        {
            adjustedUrl = DomainPolicies.LinkUrlPolicy.Adjust(request.Url);
            DomainPolicies.UrlValidationPolicy.Validate(adjustedUrl);
        });
        if (!urlResult.IsSuccess)
        {
            return urlResult;
        }

        return LinkMetadataValidator.Validate(new LinkMetadataValidationRequest(
            request.Title,
            request.Description,
            request.Image,
            adjustedUrl));
    }

    /// <summary>
    /// Ejecuta la validación del campo y convierte las excepciones de dominio en errores de validación.
    /// </summary>
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
