using ApplicationError = Linkubator.Application.Results.ApplicationError;
using ApplicationErrorCategory = Linkubator.Application.Enums.ApplicationErrorCategory;

namespace Linkubator.Web.Models.Presentation;

public static class PrivateResourceUnavailablePresenter
{
    public const string GenericMessage = "No se ha podido acceder al elemento solicitado.";

    /// <summary>
    /// Crea un modelo para presentar de forma genérica que un recurso privado no está disponible.
    /// </summary>
    public static PrivateResourceUnavailableViewModel? Create(
        ApplicationError error,
        string listRoute,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentException.ThrowIfNullOrWhiteSpace(listRoute);

        if (error.Category != ApplicationErrorCategory.ResourceNotAccessible)
        {
            return null;
        }

        return new PrivateResourceUnavailableViewModel
        {
            Message = string.IsNullOrWhiteSpace(message) ? GenericMessage : message,
            ListRoute = listRoute,
        };
    }
}
