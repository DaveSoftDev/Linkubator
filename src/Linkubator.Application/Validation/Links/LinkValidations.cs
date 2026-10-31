using Linkubator.Application.Constants;

namespace Linkubator.Application.Validation.Links;

public static class LinkValidations
{
    /// <summary>
    /// Recorta el valor del nombre y devuelve una cadena vacía si es nulo.
    /// </summary>
    private static string NormalizeName(string? value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Valida que el identificador del enlace sea positivo.
    /// </summary>
    public static Result ValidateLinkId(long linkId)
    {
        if (linkId <= 0)
        {
            return Result.Failure(LinkErrors.LinkIdInvalid());
        }

        return Result.Success();
    }

    /// <summary>
    /// Valida que el identificador de la colección sea positivo.
    /// </summary>
    public static Result ValidateCollectionId(long collectionId)
    {
        if (collectionId <= 0)
        {
            return Result.Failure(LinkErrors.CollectionIdInvalid());
        }

        return Result.Success();
    }

    /// <summary>
    /// Valida que la URL esté presente.
    /// </summary>
    public static Result ValidateUrl(string? value)
    {
        string trimmedValue = NormalizeName(value);

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return Result.Failure(LinkErrors.UrlRequired());
        }

        return Result.Success();
    }
}
