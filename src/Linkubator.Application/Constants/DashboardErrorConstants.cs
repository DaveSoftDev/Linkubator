namespace Linkubator.Application.Constants;

public static class DashboardErrors
{
    public const string UserIdField = CommonErrors.UserIdField;

    private const string GetNotAuthenticatedCode = "DASHBOARD_GET_NOT_AUTHENTICATED";

    public static ApplicationError GetNotAuthenticated()
        => CommonErrors.NotAuthenticated(GetNotAuthenticatedCode, UserIdField);
}
