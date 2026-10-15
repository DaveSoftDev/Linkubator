using DomainExceptions = Linkubator.Domain.Exceptions;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política para generación de slugs a partir de nombres.
/// </summary>
public static class SlugPolicy
{
    /// <summary>
    /// Genera un slug válido a partir del nombre proporcionado.
    /// </summary>
    /// <param name="name">Nombre a partir del cual generar el slug.</param>
    /// <returns>Slug generado y validado.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el nombre es nulo, vacío o solo espacios.</exception>
    /// <exception cref="DomainExceptions.TextBelowMinimumLengthException">Si el slug resultante es demasiado corto.</exception>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si el slug resultante es demasiado largo.</exception>
    public static string Generate(string name)
    {
        UserTextPolicy.ThrowIfNullOrWhiteSpace(name);

        string slug = AsciiTransformationPolicy.Transform(name);

        UserTextPolicy.EnsureWithinRange(slug, LengthLimitsConstants.SlugMinimum, LengthLimitsConstants.SlugMaximum);

        return slug;
    }
}
