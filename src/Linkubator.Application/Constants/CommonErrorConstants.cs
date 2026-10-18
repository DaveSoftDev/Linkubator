namespace Linkubator.Application.Constants;

public static class CommonErrors
{
    public const string RequestField = "request";
    public const string UserIdField = "UserId";
    public const string CollectionIdField = "CollectionId";
    public const string PageField = "page";

    public const string RequestRequiredMessage = "La solicitud es obligatoria.";
    public const string NotAuthenticatedMessage = "La sesión del usuario no es válida.";
    public const string PageInvalidMessage = "La página debe ser mayor que cero.";

    private const string RequestRequiredCode = "REQUEST_REQUIRED";
    private const string NotAuthenticatedCode = "NOT_AUTHENTICATED";
    private const string PageInvalidCode = "PAGE_INVALID";

    public static ApplicationError RequestRequired(string? field = RequestField, string? message = RequestRequiredMessage)
        => ApplicationError.Validation(RequestRequiredCode, field, message);

    public static ApplicationError NotAuthenticated(string code = NotAuthenticatedCode, string field = UserIdField, string? message = NotAuthenticatedMessage)
        => ApplicationError.ResourceNotAccessible(code, field, message);

    public static ApplicationError PageInvalid(string code = PageInvalidCode, string field = PageField, string? message = PageInvalidMessage)
        => ApplicationError.Validation(code, field, message);
}
