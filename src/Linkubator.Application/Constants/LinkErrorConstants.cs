namespace Linkubator.Application.Constants;

public static class LinkErrors
{
    public const string LinkIdField = "LinkId";
    public const string DestinationCollectionIdField = "DestinationCollectionId";
    public const string UrlField = "Url";
    private const string PublicLinkInPrivateCollectionField = "IsPublic";

    public const string LinkNotAccessibleMessage = "El enlace no es accesible para este usuario.";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible para este usuario.";
    public const string PublicLinkInPrivateCollectionMessage = "No se puede hacer público un enlace de una colección privada.";
    public const string DuplicateUrlMessage = "Ya existe un enlace con esa URL para este usuario.";
    private const string LinkIdInvalidMessage = "El identificador del enlace no es válido.";
    private const string DestinationCollectionIdInvalidMessage = "La colección de destino no es válida.";
    private const string CollectionIdInvalidMessage = "La colección es obligatoria.";
    private const string CollectionSelectionInvalidMessage = "La colección seleccionada no es válida.";
    private const string DeleteNotAccessibleMessage = "No se ha podido eliminar el enlace.";
    private const string UrlRequiredMessage = "La URL es obligatoria.";

    private const string LinkIdInvalidCode = "LINK_ID_INVALID";
    private const string DestinationCollectionIdInvalidCode = "LINK_DESTINATION_COLLECTION_ID_INVALID";
    private const string CollectionIdInvalidCode = "LINK_COLLECTION_ID_INVALID";
    private const string CollectionSelectionInvalidCode = "LINK_COLLECTION_SELECTION_INVALID";
    private const string CollectionNotAccessibleCode = "LINK_COLLECTION_NOT_ACCESSIBLE";
    private const string LinkNotAccessibleCode = "LINK_NOT_ACCESSIBLE";
    private const string DeleteNotAccessibleCode = "LINK_DELETE_NOT_ACCESSIBLE";
    private const string PublicLinkInPrivateCollectionCode = "LINK_PUBLIC_IN_PRIVATE_COLLECTION";
    private const string DuplicateUrlCode = "LINK_DUPLICATE_URL";
    private const string UrlRequiredCode = "URL_REQUIRED";

    /// <summary>
    /// Crea el error que indica que la solicitud es obligatoria.
    /// </summary>
    public static ApplicationError RequestRequired()
        => CommonErrors.RequestRequired();

    /// <summary>
    /// Crea el error que indica que el identificador del enlace no es válido.
    /// </summary>
    public static ApplicationError LinkIdInvalid()
        => ApplicationError.Validation(LinkIdInvalidCode, LinkIdField, LinkIdInvalidMessage);

    /// <summary>
    /// Crea el error que indica que el identificador de la colección de destino no es válido.
    /// </summary>
    public static ApplicationError DestinationCollectionIdInvalid()
        => ApplicationError.Validation(DestinationCollectionIdInvalidCode, DestinationCollectionIdField, DestinationCollectionIdInvalidMessage);

    /// <summary>
    /// Crea el error que indica que el identificador de la colección no es válido.
    /// </summary>
    public static ApplicationError CollectionIdInvalid()
        => ApplicationError.Validation(CollectionIdInvalidCode, CommonErrors.CollectionIdField, CollectionIdInvalidMessage);

    /// <summary>
    /// Crea el error que indica que la colección seleccionada no es válida.
    /// </summary>
    public static ApplicationError CollectionSelectionInvalid()
        => ApplicationError.Validation(CollectionSelectionInvalidCode, CommonErrors.CollectionIdField, CollectionSelectionInvalidMessage);

    /// <summary>
    /// Crea el error que indica que la URL es obligatoria.
    /// </summary>
    public static ApplicationError UrlRequired()
        => ApplicationError.Validation(UrlRequiredCode, UrlField, UrlRequiredMessage);

    /// <summary>
    /// Crea el error que indica que la colección pública no está disponible.
    /// </summary>
    public static ApplicationError CollectionNotAccessible()
        => ApplicationError.ResourceNotAccessible(CollectionNotAccessibleCode, CommonErrors.CollectionIdField, CollectionNotAccessibleMessage);

    /// <summary>
    /// Crea el error que indica que el enlace no es accesible para el usuario.
    /// </summary>
    public static ApplicationError LinkNotAccessible()
        => ApplicationError.ResourceNotAccessible(LinkNotAccessibleCode, LinkIdField, LinkNotAccessibleMessage);

    /// <summary>
    /// Crea el error que indica que el enlace no se puede eliminar.
    /// </summary>
    public static ApplicationError DeleteNotAccessible()
        => ApplicationError.ResourceNotAccessible(DeleteNotAccessibleCode, LinkIdField, DeleteNotAccessibleMessage);

    /// <summary>
    /// Crea el error que indica que no se puede hacer público un enlace de una colección privada.
    /// </summary>
    public static ApplicationError PublicLinkInPrivateCollection()
        => ApplicationError.Domain(PublicLinkInPrivateCollectionCode, PublicLinkInPrivateCollectionField, PublicLinkInPrivateCollectionMessage);

    /// <summary>
    /// Crea el error que indica que la URL ya está guardada por el usuario.
    /// </summary>
    public static ApplicationError DuplicateUrl(string? existingCollectionName = null)
    {
        string message = DuplicateUrlMessage;
        if (!string.IsNullOrWhiteSpace(existingCollectionName))
        {
            message = $"{DuplicateUrlMessage} La colección asociada es '{existingCollectionName}'.";
        }

        return ApplicationError.Domain(DuplicateUrlCode, UrlField, message);
    }

    /// <summary>
    /// Crea el error que indica que el número de página no es válido.
    /// </summary>
    public static ApplicationError PageInvalid()
        => CommonErrors.PageInvalid(CommonErrors.PageInvalidCode, CommonErrors.PageField, CommonErrors.PageInvalidMessage);
}
