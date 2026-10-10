using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Genera el slug de <c>Collection.Slug</c> y de <c>Tag.Slug</c> a partir del nombre.
/// Es el único punto de entrada para ambos: no distingue el origen de la llamada.
/// </summary>
public static class SlugPolicy
{
    private const int MinimumLength = 1;
    private const int MaximumLength = 150;

    public static string Generate(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        string slug = AsciiTransformationPolicy.Transform(name);

        if (slug.Length < MinimumLength)
        {
            throw new DomainExceptions.TextBelowMinimumLengthException();
        }

        if (!UserTextPolicy.IsWithinMaximumLength(slug, MaximumLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        return slug;
    }
}
