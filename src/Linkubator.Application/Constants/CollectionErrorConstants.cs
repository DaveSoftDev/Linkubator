namespace Linkubator.Application.Constants;

public static class CollectionErrors
{
    public const string CollectionIdField = CommonErrors.CollectionIdField;
    public const string NameField = "Name";

    public const string NameRequiredMessage = "El nombre es obligatorio.";
    public const string NameInUseMessage = "Ya existe una colección con ese nombre";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible para este usuario.";
    public const string CollectionHasLinksMessage = "No se puede eliminar una colección con enlaces.";
    public const string LastCollectionCannotBeDeletedMessage = "No se puede eliminar la última colección del usuario.";
    public const string CollectionIdInvalidMessage = "La colección es obligatoria.";

    private const string CollectionIdInvalidCode = "COLLECTION_ID_INVALID";
    private const string NameRequiredCode = "COLLECTION_NAME_REQUIRED";
    private const string NameInUseCode = "COLLECTION_NAME_IN_USE";
    private const string GetNotAccessibleCode = "COLLECTION_GET_NOT_ACCESSIBLE";
    private const string RenameNotAccessibleCode = "COLLECTION_RENAME_NOT_ACCESSIBLE";
    private const string VisibilityNotAccessibleCode = "COLLECTION_VISIBILITY_NOT_ACCESSIBLE";
    private const string DeleteNotAccessibleCode = "COLLECTION_DELETE_NOT_ACCESSIBLE";
    private const string HasLinksCode = "COLLECTION_HAS_LINKS";
    private const string LastCollectionCannotBeDeletedCode = "LAST_COLLECTION_CANNOT_BE_DELETED";

    public static ApplicationError RequestRequired()
        => CommonErrors.RequestRequired();

    public static ApplicationError CollectionIdInvalid()
        => ApplicationError.Validation(CollectionIdInvalidCode, CollectionIdField, CollectionIdInvalidMessage);

    public static ApplicationError NameRequired()
        => ApplicationError.Validation(NameRequiredCode, NameField, NameRequiredMessage);

    public static ApplicationError NameInUse(string? conflictingCollectionName = null)
    {
        string message = $"{NameInUseMessage}.";
        if (!string.IsNullOrWhiteSpace(conflictingCollectionName))
        {
            message = $"{NameInUseMessage}: {conflictingCollectionName}.";
        }
        return ApplicationError.Domain(NameInUseCode, NameField, message);
    }

    public static ApplicationError GetNotAccessible()
        => ApplicationError.ResourceNotAccessible(GetNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

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
        => CommonErrors.PageInvalid(CommonErrors.PageInvalidCode, CommonErrors.PageField, CommonErrors.PageInvalidMessage);
}

