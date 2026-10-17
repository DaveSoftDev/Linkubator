namespace Linkubator.Application.Constants;

public static class CollectionErrors
{
    public const string RequestField = "request";
    public const string UserIdField = "UserId";
    public const string CollectionIdField = "CollectionId";
    public const string NameField = "Name";
    public const string DescriptionField = "Description";
    public const string PageField = "page";
    public const string NotAuthenticatedMessage = "La sesión del usuario no es válida.";
    public const string NameRequiredMessage = "El nombre es obligatorio.";
    public const string NameTooLongMessage = "El nombre supera la longitud máxima.";
    public const string DescriptionTooLongMessage = "La descripción supera la longitud máxima.";
    public const string NameInUseMessage = "Ya existe una colección con ese nombre";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible para este usuario.";
    public const string CollectionHasLinksMessage = "No se puede eliminar una colección con enlaces.";
    public const string LastCollectionCannotBeDeletedMessage = "No se puede eliminar la última colección del usuario.";
    public const string PageInvalidMessage = "La página debe ser mayor que cero.";

    public static ApplicationError RequestRequired()
        => ApplicationError.Validation("REQUEST_REQUIRED", RequestField, "La solicitud es obligatoria.");

    public static ApplicationError CreateNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("COLLECTION_CREATE_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError RenameNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("COLLECTION_RENAME_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError VisibilityNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("COLLECTION_VISIBILITY_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError DeleteNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("COLLECTION_DELETE_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError ListNotAuthenticated()
        => ApplicationError.ResourceNotAccessible("COLLECTION_LIST_NOT_AUTHENTICATED", UserIdField, NotAuthenticatedMessage);

    public static ApplicationError NameRequired()
        => ApplicationError.Validation("COLLECTION_NAME_REQUIRED", NameField, NameRequiredMessage);

    public static ApplicationError NameTooLong()
        => ApplicationError.Validation("COLLECTION_NAME_TOO_LONG", NameField, NameTooLongMessage);

    public static ApplicationError DescriptionTooLong()
        => ApplicationError.Validation("COLLECTION_DESCRIPTION_TOO_LONG", DescriptionField, DescriptionTooLongMessage);

    public static ApplicationError NameInUse(string? conflictingCollectionName = null)
    {
        string message = $"{NameInUseMessage}.";
        if (!string.IsNullOrWhiteSpace(conflictingCollectionName))
        {
            message = $"{NameInUseMessage}: {conflictingCollectionName}.";
        }
        return ApplicationError.Domain("COLLECTION_NAME_IN_USE", NameField, message);
    }

    public static ApplicationError RenameNotAccessible()
        => ApplicationError.ResourceNotAccessible("COLLECTION_RENAME_NOT_ACCESSIBLE", CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError VisibilityNotAccessible()
        => ApplicationError.ResourceNotAccessible("COLLECTION_VISIBILITY_NOT_ACCESSIBLE", CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError DeleteNotAccessible()
        => ApplicationError.ResourceNotAccessible("COLLECTION_DELETE_NOT_ACCESSIBLE", CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError HasLinks()
        => ApplicationError.Domain("COLLECTION_HAS_LINKS", CollectionIdField, CollectionHasLinksMessage);

    public static ApplicationError LastCollectionCannotBeDeleted()
        => ApplicationError.Domain("LAST_COLLECTION_CANNOT_BE_DELETED", CollectionIdField, LastCollectionCannotBeDeletedMessage);

    public static ApplicationError PageInvalid()
        => ApplicationError.Validation("PAGE_INVALID", PageField, PageInvalidMessage);
}

