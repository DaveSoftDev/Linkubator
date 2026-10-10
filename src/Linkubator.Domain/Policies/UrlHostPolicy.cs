using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static class UrlHostPolicy
{
    public static string ExtractHost(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (TryExtractHost(input, out string? host) && !string.IsNullOrEmpty(host))
        {
            return host;
        }

        throw new DomainExceptions.UrlHostInvalidException();
    }

    public static bool TryExtractHost(string input, out string? host)
    {
        host = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string candidate = input.Trim();
        string authority = candidate;

        if (authority.StartsWith("//", StringComparison.Ordinal))
        {
            authority = authority[2..];
        }

        int schemeIndex = authority.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex >= 0)
        {
            authority = authority[(schemeIndex + 3)..];
        }

        if (string.IsNullOrEmpty(authority))
        {
            return false;
        }

        int pathIndex = authority.IndexOfAny(['/', '?', '#']);
        if (pathIndex >= 0)
        {
            authority = authority[..pathIndex];
        }

        int credentialsIndex = authority.LastIndexOf('@');
        if (credentialsIndex >= 0)
        {
            authority = authority[(credentialsIndex + 1)..];
        }

        if (string.IsNullOrEmpty(authority))
        {
            return false;
        }

        if (authority.IndexOf(':') is int portIndex && portIndex > 0)
        {
            string hostPart = authority[..portIndex];
            string portPart = authority[(portIndex + 1)..];

            if (string.IsNullOrEmpty(hostPart) || string.IsNullOrEmpty(portPart) || !int.TryParse(portPart, out int port) || port < 1 || port > 65535)
            {
                return false;
            }

            if (!TryNormalizeHost(hostPart, out string? normalizedHost))
            {
                return false;
            }

            host = normalizedHost;
            return true;
        }

        if (!TryNormalizeHost(authority, out string? normalizedHostWithoutPort))
        {
            return false;
        }

        host = normalizedHostWithoutPort;
        return true;
    }

    private static bool TryNormalizeHost(string candidate, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        string host = candidate.Trim();
        if (host.StartsWith('.') || host.EndsWith('.') || host.StartsWith('-') || host.EndsWith('-'))
        {
            return false;
        }

        foreach (char character in host)
        {
            if (char.IsLetterOrDigit(character) || character == '.' || character == '-')
            {
                continue;
            }

            return false;
        }

        string[] labels = host.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (labels.Length == 0)
        {
            return false;
        }

        foreach (string label in labels)
        {
            if (label.Length == 0 || label.StartsWith('-') || label.EndsWith('-'))
            {
                return false;
            }
        }

        normalized = host;
        return true;
    }
}
