using System.Text.RegularExpressions;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static partial class EmailPolicy
{
    private const int MaximumLength = 250;

    private static readonly Regex _emailRegex = EmailPattern();

    public static string Normalize(string? email)
    {
        string? trimmedEmail = email?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedEmail))
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (!UserTextPolicy.IsWithinMaximumLength(trimmedEmail, MaximumLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        if (!_emailRegex.IsMatch(trimmedEmail))
        {
            throw new DomainExceptions.InvalidEmailException();
        }

        return trimmedEmail.ToLowerInvariant();
    }

    [GeneratedRegex(@"^(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x21\x23-\x5B\x5D-\x7E]|\\[\x09\x20-\x7E])*"")@(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\[[\x21-\x5A\x5E-\x7E]*\])$")]
    private static partial Regex EmailPattern();
}
