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
    private const string DeleteNotAccessibleMessage = "No se ha podido eliminar el enlace.";
    private const string UrlRequiredMessage = "La URL es obligatoria.";

    private const string LinkIdInvalidCode = "LINK_ID_INVALID";
    private const string DestinationCollectionIdInvalidCode = "LINK_DESTINATION_COLLECTION_ID_INVALID";
    private const string CollectionIdInvalidCode = "LINK_COLLECTION_ID_INVALID";
    private const string CollectionNotAccessibleCode = "LINK_COLLECTION_NOT_ACCESSIBLE";
    private const string LinkNotAccessibleCode = "LINK_NOT_ACCESSIBLE";
    private const string DeleteNotAccessibleCode = "LINK_DELETE_NOT_ACCESSIBLE";
    private const string PublicLinkInPrivateCollectionCode = "LINK_PUBLIC_IN_PRIVATE_COLLECTION";
    private const string DuplicateUrlCode = "LINK_DUPLICATE_URL";
    private const string UrlRequiredCode = "URL_REQUIRED";

    public static ApplicationError RequestRequired()
        => CommonErrors.RequestRequired();

    public static ApplicationError LinkIdInvalid()
        => ApplicationError.Validation(LinkIdInvalidCode, LinkIdField, LinkIdInvalidMessage);

    public static ApplicationError DestinationCollectionIdInvalid()
        => ApplicationError.Validation(DestinationCollectionIdInvalidCode, DestinationCollectionIdField, DestinationCollectionIdInvalidMessage);

    public static ApplicationError CollectionIdInvalid()
        => ApplicationError.Validation(CollectionIdInvalidCode, CommonErrors.CollectionIdField, CollectionIdInvalidMessage);

    public static ApplicationError UrlRequired()
        => ApplicationError.Validation(UrlRequiredCode, UrlField, UrlRequiredMessage);

    public static ApplicationError CollectionNotAccessible()
        => ApplicationError.ResourceNotAccessible(CollectionNotAccessibleCode, CommonErrors.CollectionIdField, CollectionNotAccessibleMessage);

    public static ApplicationError LinkNotAccessible()
        => ApplicationError.ResourceNotAccessible(LinkNotAccessibleCode, LinkIdField, LinkNotAccessibleMessage);

    public static ApplicationError DeleteNotAccessible()
        => ApplicationError.ResourceNotAccessible(DeleteNotAccessibleCode, LinkIdField, DeleteNotAccessibleMessage);

    public static ApplicationError PublicLinkInPrivateCollection()
        => ApplicationError.Domain(PublicLinkInPrivateCollectionCode, PublicLinkInPrivateCollectionField, PublicLinkInPrivateCollectionMessage);

    public static ApplicationError DuplicateUrl(string? existingCollectionName = null)
    {
        string message = DuplicateUrlMessage;
        if (!string.IsNullOrWhiteSpace(existingCollectionName))
        {
            message = $"{DuplicateUrlMessage} La colección asociada es '{existingCollectionName}'.";
        }

        return ApplicationError.Domain(DuplicateUrlCode, UrlField, message);
    }

    public static ApplicationError PageInvalid()
        => CommonErrors.PageInvalid(CommonErrors.PageInvalidCode, CommonErrors.PageField, CommonErrors.PageInvalidMessage);
}
