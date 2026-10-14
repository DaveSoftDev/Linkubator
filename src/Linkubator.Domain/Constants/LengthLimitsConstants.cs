namespace Linkubator.Domain.Constants;

/// <summary>
/// Límites de longitud de los campos de texto del dominio.
/// </summary>
public static class LengthLimitsConstants
{
    /// <summary>
    /// Longitud máxima del nombre de usuario.
    /// </summary>
    public const int UserName = 100;

    /// <summary>
    /// Longitud máxima del correo electrónico.
    /// </summary>
    public const int UserEmail = 250;

    /// <summary>
    /// Longitud mínima del alias de usuario.
    /// </summary>
    public const int UserAliasMinimum = 10;

    /// <summary>
    /// Longitud máxima del alias de usuario.
    /// </summary>
    public const int UserAliasMaximum = 50;

    /// <summary>
    /// Longitud máxima del nombre de colección.
    /// </summary>
    public const int CollectionName = 50;

    /// <summary>
    /// Longitud máxima de la descripción de colección.
    /// </summary>
    public const int CollectionDescription = 250;

    /// <summary>
    /// Rango aplicable al slug de colección y al de etiqueta.
    /// </summary>
    public const int SlugMinimum = 1;

    /// <summary>
    /// Longitud máxima del slug.
    /// </summary>
    public const int SlugMaximum = 150;

    /// <summary>
    /// Longitud máxima de la URL de un enlace.
    /// </summary>
    public const int LinkUrl = 2048;

    /// <summary>
    /// Longitud máxima de la URL de imagen de un enlace.
    /// </summary>
    public const int LinkImage = 2048;

    /// <summary>
    /// Longitud máxima del título de un enlace.
    /// </summary>
    public const int LinkTitle = 300;

    /// <summary>
    /// Longitud máxima de la descripción de un enlace.
    /// </summary>
    public const int LinkDescription = 1000;
}
