using Linkubator.Application.Contracts;

namespace Linkubator.Application.Constants;

public static class DashboardErrors
{
    public const string UserIdField = "UserId";
    public const string NotAuthenticatedMessage = "La sesión del usuario no es válida.";

    public static ApplicationError GetNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("DASHBOARD_GET_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);
}
