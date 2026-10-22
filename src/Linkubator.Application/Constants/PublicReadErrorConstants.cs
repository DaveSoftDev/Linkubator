namespace Linkubator.Application.Constants;

public static class PublicReadErrors
{
    public const string AliasField = "Alias";
    public const string SlugField = "Slug";

    public const string AliasInvalidMessage = "El alias es obligatorio.";
    public const string SlugInvalidMessage = "El slug es obligatorio.";
    public const string UserNotAccessibleMessage = "El usuario no es accesible.";
    public const string CollectionsNotAccessibleMessage = "Las colecciones públicas no están disponibles.";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible.";

    private const string AliasInvalidCode = "PUBLIC_READ_ALIAS_INVALID";
    private const string SlugInvalidCode = "PUBLIC_READ_COLLECTION_SLUG_INVALID";
    private const string UserNotAccessibleCode = "PUBLIC_READ_USER_NOT_ACCESSIBLE";
    private const string CollectionsNotAccessibleCode = "PUBLIC_READ_COLLECTIONS_NOT_ACCESSIBLE";
    private const string CollectionNotAccessibleCode = "PUBLIC_READ_COLLECTION_NOT_ACCESSIBLE";

    public static ApplicationError AliasInvalid()
        => ApplicationError.Validation(AliasInvalidCode, AliasField, AliasInvalidMessage);

    public static ApplicationError SlugInvalid()
        => ApplicationError.Validation(SlugInvalidCode, SlugField, SlugInvalidMessage);

    public static ApplicationError UserNotAccessible()
        => ApplicationError.ResourceNotAccessible(UserNotAccessibleCode, AliasField, UserNotAccessibleMessage);

    public static ApplicationError CollectionsNotAccessible()
        => ApplicationError.ResourceNotAccessible(CollectionsNotAccessibleCode, AliasField, CollectionsNotAccessibleMessage);

    public static ApplicationError CollectionNotAccessible()
        => ApplicationError.ResourceNotAccessible(CollectionNotAccessibleCode, SlugField, CollectionNotAccessibleMessage);

    public static ApplicationError PageInvalid()
        => CommonErrors.PageInvalid(CommonErrors.PageInvalidCode, CommonErrors.PageField, CommonErrors.PageInvalidMessage);
}
