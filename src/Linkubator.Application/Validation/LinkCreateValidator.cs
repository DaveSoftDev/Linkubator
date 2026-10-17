using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Validation;
using Linkubator.Domain.Constants;
using Linkubator.Domain.Policies;

namespace Linkubator.Application.Validation;

public sealed class LinkCreateValidator : IValidator<CreateLinkRequest>
{
    public Result Validate(CreateLinkRequest request)
    {
        List<ApplicationError> errors = [];

        if (request is null)
        {
            return Result.Failure(ApplicationError.Validation("REQUEST_REQUIRED", "request", "La solicitud es obligatoria."));
        }

        if (request.CollectionId <= 0)
        {
            errors.Add(ApplicationError.Validation("COLLECTION_ID_INVALID", "CollectionId", "La colección es obligatoria."));
        }

        string trimmedUrl = request.Url?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedUrl))
        {
            errors.Add(ApplicationError.Validation("URL_REQUIRED", "Url", "La URL es obligatoria."));
        }
        else if (UserTextPolicy.CountCodePoints(trimmedUrl) > LengthLimitsConstants.LinkUrl)
        {
            errors.Add(ApplicationError.Validation("URL_TOO_LONG", "Url", "La URL supera la longitud máxima."));
        }

        if (!string.IsNullOrWhiteSpace(request.Title) &&
            UserTextPolicy.CountCodePoints(request.Title.Trim()) > LengthLimitsConstants.LinkTitle)
        {
            errors.Add(ApplicationError.Validation("LINK_TITLE_TOO_LONG", "Title", "El título supera la longitud máxima."));
        }

        if (!string.IsNullOrWhiteSpace(request.Description) &&
            UserTextPolicy.CountCodePoints(request.Description.Trim()) > LengthLimitsConstants.LinkDescription)
        {
            errors.Add(ApplicationError.Validation("LINK_DESCRIPTION_TOO_LONG", "Description", "La descripción supera la longitud máxima."));
        }

        if (!string.IsNullOrWhiteSpace(request.Image))
        {
            string trimmedImage = request.Image.Trim();
            if (UserTextPolicy.CountCodePoints(trimmedImage) > LengthLimitsConstants.LinkImage)
            {
                errors.Add(ApplicationError.Validation("LINK_IMAGE_TOO_LONG", "Image", "La imagen supera la longitud máxima."));
            }
            else
            {
                bool isAbsoluteHttpOrHttps = Uri.TryCreate(trimmedImage, UriKind.Absolute, out var absoluteImage)
                    && (absoluteImage.Scheme == Uri.UriSchemeHttps || absoluteImage.Scheme == Uri.UriSchemeHttp);
                bool isRelativeReference = Uri.TryCreate(trimmedImage, UriKind.Relative, out _)
                    || trimmedImage.StartsWith("//", StringComparison.Ordinal)
                    || trimmedImage.StartsWith("/", StringComparison.Ordinal)
                    || trimmedImage.StartsWith("./", StringComparison.Ordinal)
                    || trimmedImage.StartsWith("../", StringComparison.Ordinal);

                if (!isAbsoluteHttpOrHttps && !isRelativeReference)
                {
                    errors.Add(ApplicationError.Validation("LINK_IMAGE_URL_INVALID", "Image", "La imagen debe ser una URL HTTP/HTTPS válida."));
                }
            }
        }

        return errors.Count == 0 ? Result.Success() : Result.Failure(errors);
    }
}
