namespace Linkubator.Domain.Policies;

public sealed record UriSchemeDetection(string? Scheme, string? Host, bool HasExplicitScheme, bool HasHostAndPort);

public static class UriSchemePolicy
{
    public static UriSchemeDetection Detect(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));
        }

        string candidate = input.Trim();

        if (candidate.Contains("://", StringComparison.Ordinal))
        {
            int schemeIndex = candidate.IndexOf("://", StringComparison.Ordinal);
            string scheme = candidate[..schemeIndex];
            string rest = candidate[(schemeIndex + 3)..];
            bool hostFound = UrlHostPolicy.TryExtractHost(rest, out string? explicitHost);
            return new UriSchemeDetection(scheme, explicitHost, true, hostFound);
        }

        int colonIndex = candidate.IndexOf(':');
        if (colonIndex > 0 && candidate[(colonIndex + 1)..].Length > 0)
        {
            string scheme = candidate[..colonIndex];
            string remainder = candidate[(colonIndex + 1)..];
            bool looksLikeHostPort = remainder.Length > 0 && char.IsDigit(remainder[0]) && UrlHostPolicy.TryExtractHost(candidate, out _);

            if (looksLikeHostPort)
            {
                bool validHost = UrlHostPolicy.TryExtractHost(candidate, out string? hostFromHostPort);
                return new UriSchemeDetection(null, validHost ? hostFromHostPort : null, false, validHost);
            }

            if (scheme.All(character => char.IsLetterOrDigit(character) || character is '+' or '-' or '.'))
            {
                return new UriSchemeDetection(scheme, null, true, false);
            }
        }

        if (UrlHostPolicy.TryExtractHost(candidate, out string? bareHost))
        {
            return new UriSchemeDetection(null, bareHost, false, candidate.Contains(':'));
        }

        return new UriSchemeDetection(null, null, false, false);
    }
}
