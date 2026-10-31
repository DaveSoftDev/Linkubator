using Linkubator.Application.Constants;

namespace Linkubator.Application.Validation.Collections;

public static class CollectionValidations
{
    /// <summary>
    /// Recorta el valor del nombre y devuelve una cadena vacía si es nulo.
    /// </summary>
    private static string NormalizeName(string? value) => value?.Trim() ?? string.Empty;

    /// <summary>
    /// Valida que el identificador de la colección sea positivo.
    /// </summary>
    public static Result ValidateCollectionId(long collectionId)
    {
        if (collectionId <= 0)
        {
            return Result.Failure(CollectionErrors.CollectionIdInvalid());
        }

        return Result.Success();
    }

    /// <summary>
    /// Valida que el nombre de la colección no esté vacío.
    /// </summary>
    public static Result ValidateName(string? value)
    {
        string trimmedValue = NormalizeName(value);

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return Result.Failure(CollectionErrors.NameRequired());
        }

        return Result.Success();
    }

    /// <summary>
    /// Valida que el nombre de la colección no supere la longitud máxima.
    /// </summary>
    public static Result ValidateNameLength(string? value)
    {
        string trimmedValue = NormalizeName(value);

        if (!Linkubator.Domain.Policies.UserTextPolicy.IsWithinMaximumLength(trimmedValue, Linkubator.Domain.Constants.LengthLimitsConstants.CollectionName))
        {
            return Result.Failure(CollectionErrors.NameTooLong());
        }

        return Result.Success();
    }

    /// <summary>
    /// Valida que la descripción no supere la longitud máxima.
    /// </summary>
    public static Result ValidateDescription(string? value)
    {
        return value is not null && !Linkubator.Domain.Policies.UserTextPolicy.IsWithinMaximumLength(value.Trim(), Linkubator.Domain.Constants.LengthLimitsConstants.CollectionDescription)
            ? Result.Failure(CollectionErrors.DescriptionTooLong())
            : Result.Success();
    }
}
