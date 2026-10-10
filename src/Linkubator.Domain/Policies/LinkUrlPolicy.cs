using System.Text;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static class LinkUrlPolicy
{
    private const int MaximumLength = 2048;

    public static string Adjust(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        if (input.Any(char.IsWhiteSpace) || ContainsControls(input))
        {
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }

        if (input.Length > MaximumLength)
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        try
        {
            _ = input.Normalize();
        }
        catch (ArgumentException)
        {
            throw new DomainExceptions.TextContainsUnsupportedCharactersException();
        }

        if (input.StartsWith("//", StringComparison.Ordinal))
        {
            return "https://" + input[2..];
        }

        if (input.Contains("://", StringComparison.Ordinal))
        {
            int index = input.IndexOf("://", StringComparison.Ordinal);
            string scheme = input[..index];
            string remainder = input[(index + 3)..];

            if (!IsAllowedScheme(scheme))
            {
                throw new DomainExceptions.UrlSchemeNotAllowedException();
            }

            if (!UrlHostPolicy.TryExtractHost(remainder, out string? parsedHost) || string.IsNullOrEmpty(parsedHost))
            {
                throw new DomainExceptions.UrlAuthorityMissingException();
            }

            return input;
        }

        int colonIndex = input.IndexOf(':');
        if (colonIndex > 0)
        {
            string schemeCandidate = input[..colonIndex];
            string afterColon = input[(colonIndex + 1)..];
            bool isHostPort = afterColon.Length > 0 && char.IsDigit(afterColon[0]) && UrlHostPolicy.TryExtractHost(input, out _);

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

        if (UrlHostPolicy.TryExtractHost(input, out string? host))
        {
            return "https://" + input;
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
