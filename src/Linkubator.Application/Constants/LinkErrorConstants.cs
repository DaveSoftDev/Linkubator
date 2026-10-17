namespace Linkubator.Application.Constants;

public static class LinkErrors
{
    public const string UserIdField = "UserId";
    public const string LinkIdField = "LinkId";
    public const string CollectionIdField = "CollectionId";
    public const string DestinationCollectionIdField = "DestinationCollectionId";
    public const string UrlField = "Url";
    public const string PageField = "page";
    public const string NotAuthenticatedMessage = "La sesión del usuario no es válida.";
    public const string LinkNotAccessibleMessage = "El enlace no es accesible para este usuario.";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible para este usuario.";
    public const string PublicLinkInPrivateCollectionMessage = "No se puede hacer público un enlace de una colección privada.";
    public const string DuplicateUrlMessage = "Ya existe un enlace con esa URL para este usuario.";
    public const string PageInvalidMessage = "La página debe ser mayor que cero.";

    public static ApplicationError RequestRequired()
        => ApplicationError.Validation("REQUEST_REQUIRED", "request", "La solicitud es obligatoria.");

    public static ApplicationError CreateNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("LINK_CREATE_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError EditNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("LINK_EDIT_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError MoveNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("LINK_MOVE_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError VisibilityNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("LINK_VISIBILITY_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError DeleteNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("LINK_DELETE_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError ListNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("LINK_LIST_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError LinkIdInvalid()
        => ApplicationError.Validation("LINK_ID_INVALID", LinkIdField, "El identificador del enlace no es válido.");

    public static ApplicationError DestinationCollectionIdInvalid()
        => ApplicationError.Validation("LINK_DESTINATION_COLLECTION_ID_INVALID", DestinationCollectionIdField, "La colección de destino no es válida.");

    public static ApplicationError CollectionIdInvalid()
        => ApplicationError.Validation("LINK_COLLECTION_ID_INVALID", CollectionIdField, "La colección es obligatoria.");

    public static ApplicationError CollectionNotAccessible()
        => ApplicationError.ResourceNotAccessible("LINK_COLLECTION_NOT_ACCESSIBLE", CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError LinkNotAccessible()
        => ApplicationError.ResourceNotAccessible("LINK_NOT_ACCESSIBLE", LinkIdField, LinkNotAccessibleMessage);

    public static ApplicationError DeleteNotAccessible()
        => ApplicationError.ResourceNotAccessible("LINK_DELETE_NOT_ACCESSIBLE", LinkIdField, "No se ha podido eliminar el enlace.");

    public static ApplicationError PublicLinkInPrivateCollection()
        => ApplicationError.Domain("LINK_PUBLIC_IN_PRIVATE_COLLECTION", "IsPublic", PublicLinkInPrivateCollectionMessage);

    public static ApplicationError DuplicateUrl(string? existingCollectionName = null)
    {
        string message = DuplicateUrlMessage;
        if (!string.IsNullOrWhiteSpace(existingCollectionName))
        {
            message = $"{DuplicateUrlMessage} La colección asociada es '{existingCollectionName}'.";
        }

        return ApplicationError.Domain("LINK_DUPLICATE_URL", UrlField, message);
    }

    public static ApplicationError PageInvalid()
        => ApplicationError.Validation("PAGE_INVALID", PageField, PageInvalidMessage);
}
