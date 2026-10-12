using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static class UrlValidationPolicy
{
    public static void Validate(string input)
    {
        UserTextPolicy.ThrowIfNullOrWhiteSpace(input);

        if (!Uri.TryCreate(input, UriKind.Absolute, out Uri? uri) || uri is null)
        {
            throw new DomainExceptions.UrlNotAbsoluteException();
        }

        if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainExceptions.UrlSchemeNotAllowedException();
        }

        if (string.IsNullOrEmpty(uri.Host))
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        string host = uri.IdnHost;
        if (uri.HostNameType == UriHostNameType.IPv4
            && (!UrlHostPolicy.TryExtractHost(input, out string? writtenHost)
                || writtenHost is null
                || !UrlHostPolicy.IsValidHost(writtenHost)))
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        if (!UrlHostPolicy.IsValidHost(host) && !UrlHostPolicy.IsValidHost($"[{host}]") && !UrlHostPolicy.IsValidHost($"[{uri.Host}]") )
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new DomainExceptions.UrlCredentialsNotAllowedException();
        }

        if (HasInvalidPort(input))
        {
            throw new DomainExceptions.UrlPortInvalidException();
        }
    }

    private static bool HasInvalidPort(string input)
    {
        string candidate = input.Trim();

        if (candidate.StartsWith("//", StringComparison.Ordinal))
        {
            candidate = candidate[2..];
        }

        int schemeIndex = candidate.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex >= 0)
        {
            candidate = candidate[(schemeIndex + 3)..];
        }

        int pathIndex = candidate.IndexOfAny(['/', '?', '#']);
        if (pathIndex >= 0)
        {
            candidate = candidate[..pathIndex];
        }

        if (string.IsNullOrEmpty(candidate))
        {
            return false;
        }

        if (candidate.StartsWith("[", StringComparison.Ordinal))
        {
            int closingBracket = candidate.IndexOf(']');
            if (closingBracket < 0)
            {
                return false;
            }

            if (candidate.Length <= closingBracket + 1)
            {
                return false;
            }

            if (candidate[closingBracket + 1] != ':')
            {
                return false;
            }

            string portText = candidate[(closingBracket + 2)..];
            bool invalidBracketPort = !TryParsePort(portText, out int bracketPort)
                || bracketPort < 1
                || bracketPort > 65535;

            return invalidBracketPort;
        }

        int lastColonIndex = candidate.LastIndexOf(':');
        if (lastColonIndex <= 0)
        {
            return false;
        }

        string portTextCandidate = candidate[(lastColonIndex + 1)..];
        bool invalidStandardPort = !TryParsePort(portTextCandidate, out int standardPort)
            || standardPort < 1
            || standardPort > 65535;

        return invalidStandardPort;
    }

    private static bool TryParsePort(string? portText, out int port)
    {
        port = 0;

        if (string.IsNullOrWhiteSpace(portText) || !portText.All(char.IsDigit))
        {
            return false;
        }

        return int.TryParse(portText, out port);
    }
}
