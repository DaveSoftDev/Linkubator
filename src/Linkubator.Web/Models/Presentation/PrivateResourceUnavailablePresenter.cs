using ApplicationError = Linkubator.Application.Results.ApplicationError;
using ApplicationErrorCategory = Linkubator.Application.Enums.ApplicationErrorCategory;

namespace Linkubator.Web.Models.Presentation;

public static class PrivateResourceUnavailablePresenter
{
    private const string GenericMessage = "No se ha podido acceder al elemento solicitado.";

    public static PrivateResourceUnavailableViewModel? Create(
        ApplicationError error,
        string listRoute)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentException.ThrowIfNullOrWhiteSpace(listRoute);

        if (error.Category != ApplicationErrorCategory.ResourceNotAccessible)
        {
            return null;
        }

        return new PrivateResourceUnavailableViewModel
        {
            Message = GenericMessage,
            ListRoute = listRoute,
        };
    }
}
