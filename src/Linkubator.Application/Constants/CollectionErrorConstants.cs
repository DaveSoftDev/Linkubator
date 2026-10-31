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

    /// <summary>
    /// Crea el error que indica que la solicitud es obligatoria.
    /// </summary>
    public static ApplicationError RequestRequired()
        => CommonErrors.RequestRequired();

    /// <summary>
    /// Crea el error que indica que el identificador de la colección no es válido.
    /// </summary>
    public static ApplicationError CollectionIdInvalid()
        => ApplicationError.Validation(CollectionIdInvalidCode, CollectionIdField, CollectionIdInvalidMessage);

    /// <summary>
    /// Crea el error que indica que el nombre de la colección es obligatorio.
    /// </summary>
    public static ApplicationError NameRequired()
        => ApplicationError.Validation(NameRequiredCode, NameField, NameRequiredMessage);

    /// <summary>
    /// Crea el error que indica que el nombre de la colección supera la longitud permitida.
    /// </summary>
    public static ApplicationError NameTooLong()
        => ApplicationError.Validation(NameTooLongCode, NameField, NameTooLongMessage);

    /// <summary>
    /// Crea el error que indica que la descripción de la colección supera la longitud permitida.
    /// </summary>
    public static ApplicationError DescriptionTooLong()
        => ApplicationError.Validation(DescriptionTooLongCode, DescriptionField, DescriptionTooLongMessage);

    /// <summary>
    /// Crea el error que indica que el nombre de la colección ya está en uso.
    /// </summary>
    public static ApplicationError NameInUse(string? conflictingCollectionName = null)
    {
        string message = $"{NameInUseMessage}.";
        if (!string.IsNullOrWhiteSpace(conflictingCollectionName))
        {
            message = $"{NameInUseMessage}: {conflictingCollectionName}.";
        }
        return ApplicationError.Domain(NameInUseCode, NameField, message);
    }

    /// <summary>
    /// Crea el error que indica que la colección solicitada no es accesible.
    /// </summary>
    public static ApplicationError GetNotAccessible()
        => ApplicationError.ResourceNotAccessible(GetNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    /// <summary>
    /// Crea el error que indica que la colección que se quiere actualizar no es accesible.
    /// </summary>
    public static ApplicationError UpdateNotAccessible()
        => ApplicationError.ResourceNotAccessible(UpdateNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    /// <summary>
    /// Crea el error que indica que no se puede cambiar el nombre de la colección.
    /// </summary>
    public static ApplicationError RenameNotAccessible()
        => ApplicationError.ResourceNotAccessible(RenameNotAccessibleCode, CollectionIdField, RenameNotAccessibleMessage);

    /// <summary>
    /// Crea el error que indica que no se puede cambiar la privacidad de la colección.
    /// </summary>
    public static ApplicationError VisibilityNotAccessible()
        => ApplicationError.ResourceNotAccessible(VisibilityNotAccessibleCode, CollectionIdField, VisibilityNotAccessibleMessage);

    /// <summary>
    /// Crea el error que indica que el enlace no se puede eliminar.
    /// </summary>
    public static ApplicationError DeleteNotAccessible()
        => ApplicationError.ResourceNotAccessible(DeleteNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

    /// <summary>
    /// Crea el error que indica que no se puede eliminar una colección que contiene enlaces.
    /// </summary>
    public static ApplicationError HasLinks()
        => ApplicationError.Domain(HasLinksCode, CollectionIdField, CollectionHasLinksMessage);

    /// <summary>
    /// Crea el error que indica que no se puede eliminar la última colección del usuario.
    /// </summary>
    public static ApplicationError LastCollectionCannotBeDeleted()
        => ApplicationError.Domain(LastCollectionCannotBeDeletedCode, CollectionIdField, LastCollectionCannotBeDeletedMessage);

    /// <summary>
    /// Crea el error que indica que el número de página no es válido.
    /// </summary>
    public static ApplicationError PageInvalid()
        => CommonErrors.PageInvalid(CommonErrors.PageInvalidCode, CommonErrors.PageField, CommonErrors.PageInvalidMessage);
}

