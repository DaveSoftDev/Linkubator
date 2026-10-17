namespace Linkubator.Application.Constants;

public static class LinkErrors
{
    public const string UserIdField = CommonErrors.UserIdField;
    public const string PageField = CommonErrors.PageField;
    public const string LinkIdField = "LinkId";
    public const string CollectionIdField = "CollectionId";
    public const string DestinationCollectionIdField = "DestinationCollectionId";
    public const string UrlField = "Url";
    private const string PublicLinkInPrivateCollectionField = "IsPublic";

    public const string PageInvalidMessage = CommonErrors.PageInvalidMessage;
    public const string LinkNotAccessibleMessage = "El enlace no es accesible para este usuario.";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible para este usuario.";
    public const string PublicLinkInPrivateCollectionMessage = "No se puede hacer público un enlace de una colección privada.";
    public const string DuplicateUrlMessage = "Ya existe un enlace con esa URL para este usuario.";
    private const string LinkIdInvalidMessage = "El identificador del enlace no es válido.";
    private const string DestinationCollectionIdInvalidMessage = "La colección de destino no es válida.";
    private const string CollectionIdInvalidMessage = "La colección es obligatoria.";
    private const string DeleteNotAccessibleMessage = "No se ha podido eliminar el enlace.";
    private const string UrlRequiredMessage = "La URL es obligatoria.";
    private const string UrlTooLongMessage = "La URL supera la longitud máxima.";
    private const string TitleTooLongMessage = "El título supera la longitud máxima.";
    private const string DescriptionTooLongMessage = "La descripción supera la longitud máxima.";
    private const string ImageTooLongMessage = "La imagen supera la longitud máxima.";
    private const string ImageUrlInvalidMessage = "La imagen debe ser una URL HTTP/HTTPS válida.";

    private const string CreateNotAuthenticatedCode = "LINK_CREATE_NOT_AUTHENTICATED";
    private const string EditNotAuthenticatedCode = "LINK_EDIT_NOT_AUTHENTICATED";
    private const string MoveNotAuthenticatedCode = "LINK_MOVE_NOT_AUTHENTICATED";
    private const string VisibilityNotAuthenticatedCode = "LINK_VISIBILITY_NOT_AUTHENTICATED";
    private const string DeleteNotAuthenticatedCode = "LINK_DELETE_NOT_AUTHENTICATED";
    private const string ListNotAuthenticatedCode = "LINK_LIST_NOT_AUTHENTICATED";
    private const string LinkIdInvalidCode = "LINK_ID_INVALID";
    private const string DestinationCollectionIdInvalidCode = "LINK_DESTINATION_COLLECTION_ID_INVALID";
    private const string CollectionIdInvalidCode = "LINK_COLLECTION_ID_INVALID";
    private const string CollectionNotAccessibleCode = "LINK_COLLECTION_NOT_ACCESSIBLE";
    private const string LinkNotAccessibleCode = "LINK_NOT_ACCESSIBLE";
    private const string DeleteNotAccessibleCode = "LINK_DELETE_NOT_ACCESSIBLE";
    private const string PublicLinkInPrivateCollectionCode = "LINK_PUBLIC_IN_PRIVATE_COLLECTION";
    private const string DuplicateUrlCode = "LINK_DUPLICATE_URL";
    private const string PageInvalidCode = "PAGE_INVALID";
    private const string UrlRequiredCode = "URL_REQUIRED";
    private const string UrlTooLongCode = "URL_TOO_LONG";
    private const string TitleTooLongCode = "LINK_TITLE_TOO_LONG";
    private const string DescriptionTooLongCode = "LINK_DESCRIPTION_TOO_LONG";
    private const string ImageTooLongCode = "LINK_IMAGE_TOO_LONG";
    private const string ImageUrlInvalidCode = "LINK_IMAGE_URL_INVALID";

    public static ApplicationError RequestRequired()
        => CommonErrors.RequestRequired();

    public static ApplicationError CreateNotAuthenticated()
        => CommonErrors.NotAuthenticated(CreateNotAuthenticatedCode, UserIdField);

    public static ApplicationError EditNotAuthenticated()
        => CommonErrors.NotAuthenticated(EditNotAuthenticatedCode, UserIdField);

    public static ApplicationError MoveNotAuthenticated()
        => CommonErrors.NotAuthenticated(MoveNotAuthenticatedCode, UserIdField);

    public static ApplicationError VisibilityNotAuthenticated()
        => CommonErrors.NotAuthenticated(VisibilityNotAuthenticatedCode, UserIdField);

    public static ApplicationError DeleteNotAuthenticated()
        => CommonErrors.NotAuthenticated(DeleteNotAuthenticatedCode, UserIdField);

    public static ApplicationError ListNotAuthenticated()
        => CommonErrors.NotAuthenticated(ListNotAuthenticatedCode, UserIdField);

    public static ApplicationError LinkIdInvalid()
        => ApplicationError.Validation(LinkIdInvalidCode, LinkIdField, LinkIdInvalidMessage);

    public static ApplicationError DestinationCollectionIdInvalid()
        => ApplicationError.Validation(DestinationCollectionIdInvalidCode, DestinationCollectionIdField, DestinationCollectionIdInvalidMessage);

    public static ApplicationError CollectionIdInvalid()
        => ApplicationError.Validation(CollectionIdInvalidCode, CollectionIdField, CollectionIdInvalidMessage);

    public static ApplicationError UrlRequired()
        => ApplicationError.Validation(UrlRequiredCode, UrlField, UrlRequiredMessage);

    public static ApplicationError UrlTooLong()
        => ApplicationError.Validation(UrlTooLongCode, UrlField, UrlTooLongMessage);

    public static ApplicationError TitleTooLong()
        => ApplicationError.Validation(TitleTooLongCode, "Title", TitleTooLongMessage);

    public static ApplicationError DescriptionTooLong()
        => ApplicationError.Validation(DescriptionTooLongCode, "Description", DescriptionTooLongMessage);

    public static ApplicationError ImageTooLong()
        => ApplicationError.Validation(ImageTooLongCode, "Image", ImageTooLongMessage);

    public static ApplicationError ImageUrlInvalid()
        => ApplicationError.Validation(ImageUrlInvalidCode, "Image", ImageUrlInvalidMessage);

    public static ApplicationError CollectionNotAccessible()
        => ApplicationError.ResourceNotAccessible(CollectionNotAccessibleCode, CollectionIdField, CollectionNotAccessibleMessage);

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
        => CommonErrors.PageInvalid(PageInvalidCode, PageField, PageInvalidMessage);
}
