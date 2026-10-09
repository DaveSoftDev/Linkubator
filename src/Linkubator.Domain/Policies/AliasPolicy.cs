using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static class AliasPolicy
{
    private const int MinimumLength = 10;
    private const int MaximumLength = 50;

    private static readonly HashSet<string> _reservedAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        "linkubator",
        "administrator",
        "administrador",
    };

    public static string Generate(string candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        string alias = AsciiTransformationPolicy.Transform(candidate);

        if (alias.Length < MinimumLength)
        {
            throw new DomainExceptions.TextBelowMinimumLengthException();
        }

        if (!UserTextPolicy.IsWithinMaximumLength(alias, MaximumLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        if (_reservedAliases.Contains(alias))
        {
            throw new DomainExceptions.AliasReservedException();
        }

        return alias;
    }
}
