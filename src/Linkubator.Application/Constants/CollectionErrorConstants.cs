namespace Linkubator.Application.Constants;

public static class CollectionErrors
{
    public const string CollectionIdField = CommonErrors.CollectionIdField;
    public const string NameField = "Name";
    public const string DescriptionField = "Description";

    public const string NameRequiredMessage = "El nombre es obligatorio.";
    public const string NameInUseMessage = "Ya existe una colección con ese nombre";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible para este usuario.";
    public const string CollectionHasLinksMessage = "No se puede eliminar una colección con enlaces.";
    public const string LastCollectionCannotBeDeletedMessage = "No se puede eliminar la última colección del usuario.";
    public const string CollectionIdInvalidMessage = "La colección es obligatoria.";
    public const string DescriptionTooLongMessage = "La descripción es demasiado larga.";
    public const string NameTooLongMessage = "El nombre es demasiado largo.";
    public const string RenameNotAccessibleMessage = "No se puede editar el nombre de la colección.";
    public const string VisibilityNotAccessibleMessage = "No se puede cambiar la visibilidad de la colección.";

    private const string CollectionIdInvalidCode = "COLLECTION_ID_INVALID";
    private const string NameRequiredCode = "COLLECTION_NAME_REQUIRED";
    private const string NameTooLongCode = "COLLECTION_NAME_TOO_LONG";
    private const string DescriptionTooLongCode = "COLLECTION_DESCRIPTION_TOO_LONG";
    private const string NameInUseCode = "COLLECTION_NAME_IN_USE";
    private const string GetNotAccessibleCode = "COLLECTION_GET_NOT_ACCESSIBLE";
    private const string UpdateNotAccessibleCode = "COLLECTION_UPDATE_NOT_ACCESSIBLE";
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

    public static ApplicationError GetNotAccessible()
        => ApplicationError.ResourceNotAccessible(GetNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError UpdateNotAccessible()
        => ApplicationError.ResourceNotAccessible(UpdateNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError RenameNotAccessible()
        => ApplicationError.ResourceNotAccessible(RenameNotAccessibleCode, CollectionIdField, RenameNotAccessibleMessage);

    public static ApplicationError VisibilityNotAccessible()
        => ApplicationError.ResourceNotAccessible(VisibilityNotAccessibleCode, CollectionIdField, VisibilityNotAccessibleMessage);

    public static ApplicationError DeleteNotAccessible()
        => ApplicationError.ResourceNotAccessible(DeleteNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError HasLinks()
        => ApplicationError.Domain(HasLinksCode, CollectionIdField, CollectionHasLinksMessage);

    public static ApplicationError LastCollectionCannotBeDeleted()
        => ApplicationError.Domain(LastCollectionCannotBeDeletedCode, CollectionIdField, LastCollectionCannotBeDeletedMessage);

    public static ApplicationError PageInvalid()
        => CommonErrors.PageInvalid(CommonErrors.PageInvalidCode, CommonErrors.PageField, CommonErrors.PageInvalidMessage);
}

