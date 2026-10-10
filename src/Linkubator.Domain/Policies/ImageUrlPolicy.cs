using System.Net;
using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Domain.Policies;

public static class ImageUrlPolicy
{
    private const int MaximumLength = 2048;

    public static string Adjust(string input, string baseUrl)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);

        string trimmedInput = input.Trim();
        if (string.IsNullOrEmpty(trimmedInput))
        {
            return string.Empty;
        }

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

        string candidate = trimmedInput;
        var detection = UriSchemePolicy.Detect(candidate);

        if (candidate.StartsWith("//", StringComparison.Ordinal))
        {
            candidate = "https:" + candidate;
        }

        if (detection.Scheme is not null)
        {
            if (!IsAllowedScheme(detection.Scheme))
            {
                throw new DomainExceptions.UrlSchemeNotAllowedException();
            }

            if (string.Equals(detection.Scheme, "http", StringComparison.OrdinalIgnoreCase))
            {
                int schemeSeparatorIndex = candidate.IndexOf("://", StringComparison.Ordinal);
                candidate = "https://" + candidate[(schemeSeparatorIndex + 3)..];
            }
        }
        else if (detection.HasHostAndPort)
        {
            candidate = "https://" + candidate;
        }
        else
        {
            candidate = ResolveRelative(candidate, baseUrl);
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri? absoluteUri))
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        if (absoluteUri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
        {
            absoluteUri = new UriBuilder(absoluteUri)
            {
                Scheme = "https"
            }.Uri;
        }

        if (string.Equals(absoluteUri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || IPAddress.TryParse(absoluteUri.Host, out _))
        {
            throw new DomainExceptions.ImageHostNotAllowedException();
        }

        return absoluteUri.ToString();
    }

    private static string ResolveRelative(string input, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or whitespace.", nameof(baseUrl));
        }

        Uri baseUri;
        try
        {
            baseUri = new Uri(baseUrl.Trim(), UriKind.Absolute);
        }
        catch (UriFormatException)
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }

        try
        {
            return new Uri(baseUri, input).ToString();
        }
        catch (UriFormatException)
        {
            throw new DomainExceptions.UrlHostInvalidException();
        }
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
