using DomainExceptions = Linkubator.Domain.Exceptions;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Policies;

/// <summary>
/// Política para generación y validación de alias.
/// </summary>
public static class AliasPolicy
{
    private static readonly HashSet<string> _reservedAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        "linkubator",
        "administrator",
        "administrador",
    };

    /// <summary>
    /// Genera un alias válido a partir de un candidato.
    /// </summary>
    /// <param name="candidate">Candidato a alias.</param>
    /// <returns>Alias generado y validado.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el candidato es nulo, vacío o solo espacios.</exception>
    /// <exception cref="DomainExceptions.TextBelowMinimumLengthException">Si el alias resultante es demasiado corto.</exception>
    /// <exception cref="DomainExceptions.TextExceedsMaximumLengthException">Si el alias resultante es demasiado largo.</exception>
    /// <exception cref="DomainExceptions.AliasReservedException">Si el alias está reservado.</exception>
    public static string Generate(string candidate)
    {
        UserTextPolicy.ThrowIfNullOrWhiteSpace(candidate);

        string alias = AsciiTransformationPolicy.Transform(candidate);

        UserTextPolicy.EnsureWithinRange(alias, LengthLimitsConstants.UserAliasMinimum, LengthLimitsConstants.UserAliasMaximum);

        if (_reservedAliases.Contains(alias))
        {
            throw new DomainExceptions.AliasReservedException();
        }

        return alias;
    }
}
