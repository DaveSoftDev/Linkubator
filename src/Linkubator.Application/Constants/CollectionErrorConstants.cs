namespace Linkubator.Application.Constants;

public static class CollectionErrors
{
    public const string PageField = CommonErrors.PageField;
    public const string UserIdField = CommonErrors.UserIdField;
    public const string CollectionIdField = "CollectionId";
    public const string NameField = "Name";
    public const string DescriptionField = "Description";
    
    public const string PageInvalidMessage = CommonErrors.PageInvalidMessage;
    public const string NameRequiredMessage = "El nombre es obligatorio.";
    public const string NameTooLongMessage = "El nombre supera la longitud máxima.";
    public const string DescriptionTooLongMessage = "La descripción supera la longitud máxima.";
    public const string NameInUseMessage = "Ya existe una colección con ese nombre";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible para este usuario.";
    public const string CollectionHasLinksMessage = "No se puede eliminar una colección con enlaces.";
    public const string LastCollectionCannotBeDeletedMessage = "No se puede eliminar la última colección del usuario.";

    private const string CreateNotAuthenticatedCode = "COLLECTION_CREATE_NOT_AUTHENTICATED";
    private const string RenameNotAuthenticatedCode = "COLLECTION_RENAME_NOT_AUTHENTICATED";
    private const string VisibilityNotAuthenticatedCode = "COLLECTION_VISIBILITY_NOT_AUTHENTICATED";
    private const string DeleteNotAuthenticatedCode = "COLLECTION_DELETE_NOT_AUTHENTICATED";
    private const string ListNotAuthenticatedCode = "COLLECTION_LIST_NOT_AUTHENTICATED";
    private const string NameRequiredCode = "COLLECTION_NAME_REQUIRED";
    private const string NameTooLongCode = "COLLECTION_NAME_TOO_LONG";
    private const string DescriptionTooLongCode = "COLLECTION_DESCRIPTION_TOO_LONG";
    private const string NameInUseCode = "COLLECTION_NAME_IN_USE";
    private const string RenameNotAccessibleCode = "COLLECTION_RENAME_NOT_ACCESSIBLE";
    private const string VisibilityNotAccessibleCode = "COLLECTION_VISIBILITY_NOT_ACCESSIBLE";
    private const string DeleteNotAccessibleCode = "COLLECTION_DELETE_NOT_ACCESSIBLE";
    private const string HasLinksCode = "COLLECTION_HAS_LINKS";
    private const string LastCollectionCannotBeDeletedCode = "LAST_COLLECTION_CANNOT_BE_DELETED";
    private const string PageInvalidCode = "PAGE_INVALID";

    public static ApplicationError RequestRequired()
        => CommonErrors.RequestRequired();

    public static ApplicationError CreateNotAuthenticated()
        => CommonErrors.NotAuthenticated(CreateNotAuthenticatedCode, UserIdField);

    public static ApplicationError RenameNotAuthenticated()
        => CommonErrors.NotAuthenticated(RenameNotAuthenticatedCode, UserIdField);

    public static ApplicationError VisibilityNotAuthenticated()
        => CommonErrors.NotAuthenticated(VisibilityNotAuthenticatedCode, UserIdField);

    public static ApplicationError DeleteNotAuthenticated()
        => CommonErrors.NotAuthenticated(DeleteNotAuthenticatedCode, UserIdField);

    public static ApplicationError ListNotAuthenticated()
        => CommonErrors.NotAuthenticated(ListNotAuthenticatedCode, UserIdField);

    public static ApplicationError NameRequired()
        => ApplicationError.Validation(NameRequiredCode, NameField, NameRequiredMessage);

    public static ApplicationError NameTooLong()
        => ApplicationError.Validation(NameTooLongCode, NameField, NameTooLongMessage);

    public static ApplicationError DescriptionTooLong()
        => ApplicationError.Validation(DescriptionTooLongCode, DescriptionField, DescriptionTooLongMessage);

    public static ApplicationError NameInUse(string? conflictingCollectionName = null)
    {
        string message = $"{NameInUseMessage}.";
        if (!string.IsNullOrWhiteSpace(conflictingCollectionName))
        {
            message = $"{NameInUseMessage}: {conflictingCollectionName}.";
        }
        return ApplicationError.Domain(NameInUseCode, NameField, message);
    }

    public static ApplicationError RenameNotAccessible()
        => ApplicationError.ResourceNotAccessible(RenameNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError VisibilityNotAccessible()
        => ApplicationError.ResourceNotAccessible(VisibilityNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError DeleteNotAccessible()
        => ApplicationError.ResourceNotAccessible(DeleteNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError HasLinks()
        => ApplicationError.Domain(HasLinksCode, CollectionIdField, CollectionHasLinksMessage);

    public static ApplicationError LastCollectionCannotBeDeleted()
        => ApplicationError.Domain(LastCollectionCannotBeDeletedCode, CollectionIdField, LastCollectionCannotBeDeletedMessage);

    public static ApplicationError PageInvalid()
        => CommonErrors.PageInvalid(PageInvalidCode, PageField, PageInvalidMessage);
}

