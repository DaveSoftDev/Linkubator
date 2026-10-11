using System.Net;
using System.Net.Sockets;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static class UrlHostPolicy
{
    public static string ExtractHost(string input)
    {
        UserTextPolicy.ThrowIfNullOrWhiteSpace(input);

        if (TryExtractHost(input, out string? host) && !string.IsNullOrEmpty(host))
        {
            return host;
        }

        throw new DomainExceptions.UrlHostInvalidException();
    }

    public static bool IsValidHost(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string candidate = input.Trim();

        if (candidate.StartsWith("[", StringComparison.Ordinal) && candidate.EndsWith("]", StringComparison.Ordinal))
        {
            string literal = candidate[1..^1];
            return IPAddress.TryParse(literal, out IPAddress? ipv6Address)
                && ipv6Address.AddressFamily == AddressFamily.InterNetworkV6;
        }

        if (IPAddress.TryParse(candidate, out IPAddress? parsedAddress))
        {
            return parsedAddress.AddressFamily == AddressFamily.InterNetworkV6
                || (parsedAddress.AddressFamily == AddressFamily.InterNetwork && HasFourDecimalGroups(candidate));
        }

        if (string.Equals(candidate, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (candidate.Contains("..", StringComparison.Ordinal)
            || candidate.Length > 0 && candidate[0] == '.'
            || candidate.Length > 0 && candidate[^1] == '.')
        {
            return false;
        }

        string[] labels = candidate.Split('.', StringSplitOptions.TrimEntries);
        if (labels.Length < 2 || labels.Any(label => string.IsNullOrEmpty(label)))
        {
            return false;
        }

        foreach (string label in labels)
        {
            if (label.Length == 0 || label.StartsWith('-') || label.EndsWith('-'))
            {
                return false;
            }

            foreach (char character in label)
            {
                if (!char.IsLetterOrDigit(character) && character != '-')
                {
                    return false;
                }
            }
        }

        string lastLabel = labels[^1];
        bool isPunycodeTld = lastLabel.StartsWith("xn--", StringComparison.OrdinalIgnoreCase);
        if (!isPunycodeTld && (lastLabel.Length < 2 || !lastLabel.All(char.IsLetter)))
        {
            return false;
        }

        return true;
    }

    private static bool HasFourDecimalGroups(string candidate)
    {
        string[] groups = candidate.Split('.');

        return groups.Length == 4
            && groups.All(group => group.Length is > 0 and <= 3 && group.All(char.IsAsciiDigit));
    }

    public static bool TryExtractValidHost(string input, out string? host)
    {
        host = null;

        if (TryExtractHost(input, out host) && !string.IsNullOrEmpty(host) && IsValidHost(host))
        {
            return true;
        }

        host = null;
        return false;
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
        int delimiterIndex = authority.IndexOfAny(['/', '?', '#']);
        if (schemeIndex >= 0 && (delimiterIndex < 0 || schemeIndex < delimiterIndex))
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

        if (authority.StartsWith(":", StringComparison.Ordinal))
        {
            return false;
        }

        if (authority.StartsWith("[", StringComparison.Ordinal))
        {
            int closingBracketIndex = authority.IndexOf(']');
            if (closingBracketIndex < 0)
            {
                return false;
            }

            string hostCandidate = authority[..(closingBracketIndex + 1)];
            if (authority.Length > closingBracketIndex + 1)
            {
                if (authority[closingBracketIndex + 1] != ':')
                {
                    return false;
                }

                string portPart = authority[(closingBracketIndex + 2)..];
                if (string.IsNullOrEmpty(portPart) || !int.TryParse(portPart, out int port) || port < 1 || port > 65535)
                {
                    return false;
                }
            }

            host = hostCandidate;
            return true;
        }

        int lastColonIndex = authority.LastIndexOf(':');
        if (lastColonIndex > 0)
        {
            string hostPart = authority[..lastColonIndex];
            string portPart = authority[(lastColonIndex + 1)..];

            if (string.IsNullOrEmpty(hostPart) || string.IsNullOrEmpty(portPart))
            {
                return false;
            }

            if (!portPart.All(char.IsDigit))
            {
                return false;
            }

            if (!int.TryParse(portPart, out int port) || port < 1 || port > 65535)
            {
                return false;
            }

            host = hostPart;
            return true;
        }

        host = authority;
        return true;
    }
}
