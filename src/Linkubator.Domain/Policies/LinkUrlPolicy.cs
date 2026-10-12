using System.Text;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static class LinkUrlPolicy
{
    private const int MaximumLength = 2048;

    public static string Adjust(string input)
    {
        if (input is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        string trimmedInput = input.Trim();
        UserTextPolicy.ThrowIfNullOrWhiteSpace(trimmedInput);

        if (trimmedInput.Any(char.IsWhiteSpace) || ContainsControls(trimmedInput))
        {
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }

        if (trimmedInput.Length > MaximumLength)
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        try
        {
            _ = trimmedInput.Normalize();
        }
        catch (ArgumentException)
        {
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }

        if (trimmedInput.StartsWith("//", StringComparison.Ordinal))
        {
            return "https://" + trimmedInput[2..];
        }

        int separatorIndex = trimmedInput.IndexOf("://", StringComparison.Ordinal);
        int firstDelimiterIndex = trimmedInput.IndexOfAny(['/', '?', '#']);

        if (separatorIndex >= 0 && (firstDelimiterIndex < 0 || separatorIndex < firstDelimiterIndex))
        {
            int index = separatorIndex;
            string scheme = trimmedInput[..index];
            string remainder = trimmedInput[(index + 3)..];

            if (!IsAllowedScheme(scheme))
            {
                throw new DomainExceptions.UrlSchemeNotAllowedException();
            }

            if (!UrlHostPolicy.TryExtractValidHost(remainder, out string? parsedHost) || string.IsNullOrEmpty(parsedHost))
            {
                throw new DomainExceptions.UrlAuthorityMissingException();
            }

            return trimmedInput;
        }

        int colonIndex = trimmedInput.IndexOf(':');
        if (colonIndex > 0)
        {
            string schemeCandidate = trimmedInput[..colonIndex];
            string afterColon = trimmedInput[(colonIndex + 1)..];
            bool isHostPort = afterColon.Length > 0 && char.IsDigit(afterColon[0]) && UrlHostPolicy.TryExtractValidHost(trimmedInput, out _);

            if (!isHostPort)
            {
                if (schemeCandidate.Length > 0 && schemeCandidate.All(character => char.IsLetterOrDigit(character) || character is '+' or '-' or '.'))
                {
                    if (!IsAllowedScheme(schemeCandidate))
                    {
                        throw new DomainExceptions.UrlSchemeNotAllowedException();
                    }

                    throw new DomainExceptions.UrlAuthorityMissingException();
                }
            }
        }

        if (UrlHostPolicy.TryExtractValidHost(trimmedInput, out string? host))
        {
            return (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ? "http://" : "https://") + trimmedInput;
        }

        throw new DomainExceptions.UrlHostInvalidException();
    }

    private static bool ContainsControls(string input)
    {
        foreach (char character in input)
        {
            if (char.IsControl(character))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAllowedScheme(string scheme)
    {
        return string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase)
            || string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase);
    }
}
