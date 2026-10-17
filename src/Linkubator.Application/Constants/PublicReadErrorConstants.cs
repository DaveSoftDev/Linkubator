namespace Linkubator.Application.Constants;

public static class PublicReadErrors
{
    public const string AliasField = "Alias";
    public const string SlugField = "Slug";
    public const string PageField = "page";

    public const string AliasInvalidMessage = "El alias es obligatorio.";
    public const string SlugInvalidMessage = "El slug es obligatorio.";
    public const string UserNotAccessibleMessage = "El usuario no es accesible.";
    public const string CollectionsNotAccessibleMessage = "Las colecciones públicas no están disponibles.";
    public const string CollectionNotAccessibleMessage = "La colección no es accesible.";
    public const string PageInvalidMessage = "La página debe ser mayor que cero.";

    public static ApplicationError AliasInvalid()
        => ApplicationError.Validation("PUBLIC_READ_ALIAS_INVALID", AliasField, AliasInvalidMessage);

    public static ApplicationError SlugInvalid()
        => ApplicationError.Validation("PUBLIC_READ_COLLECTION_SLUG_INVALID", SlugField, SlugInvalidMessage);

    public static ApplicationError UserNotAccessible()
        => ApplicationError.ResourceNotAccessible("PUBLIC_READ_USER_NOT_ACCESSIBLE", AliasField, UserNotAccessibleMessage);

    public static ApplicationError CollectionsNotAccessible()
        => ApplicationError.ResourceNotAccessible("PUBLIC_READ_COLLECTIONS_NOT_ACCESSIBLE", AliasField, CollectionsNotAccessibleMessage);

    public static ApplicationError CollectionNotAccessible()
        => ApplicationError.ResourceNotAccessible("PUBLIC_READ_COLLECTION_NOT_ACCESSIBLE", SlugField, CollectionNotAccessibleMessage);

    public static ApplicationError PageInvalid()
        => ApplicationError.Validation("PUBLIC_READ_PAGE_INVALID", PageField, PageInvalidMessage);
}
