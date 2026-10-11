using System.Globalization;
using System.Net;
using System.Text;

namespace Linkubator.Domain.Policies;

public static class UrlNormalizationPolicy
{
    private static readonly HashSet<string> _marketingParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        "utm_source",
        "utm_medium",
        "utm_campaign",
        "utm_term",
        "utm_content",
        "gclid",
        "wbraid",
        "gbraid",
        "fbclid",
        "msclkid",
        "ttclid",
        "epik",
        "gad_source",
        "gad_campaignid",
        "srsltid",
        "gcs",
        "gcd",
        "at_medium",
        "at_campaign",
        "at_platform",
        "at_creation",
        "at_term"
    };

    public static string Normalize(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Input cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || uri is null)
        {
            throw new ArgumentException("Input must be an absolute URI.", nameof(url));
        }

        string host = uri.IdnHost;
        string port = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
        string path = Uri.UnescapeDataString(uri.AbsolutePath);
        string query = NormalizeQuery(uri.Query);

        string combined = host + port + path + query;
        return Canonicalize(combined);
    }

    private static string NormalizeQuery(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return string.Empty;
        }

        string[] segments = query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return string.Empty;
        }

        List<KeyValuePair<string, string>> parameters = [];

        foreach (string segment in segments)
        {
            int separatorIndex = segment.IndexOf('=');
            string name = separatorIndex >= 0 ? segment[..separatorIndex] : segment;
            string value = separatorIndex >= 0 ? segment[(separatorIndex + 1)..] : string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            string decodedName = WebUtility.UrlDecode(name);
            string decodedValue = WebUtility.UrlDecode(value);

            if (_marketingParameters.Contains(decodedName))
            {
                continue;
            }

            parameters.Add(new KeyValuePair<string, string>(decodedName.ToLowerInvariant(), decodedValue.ToLowerInvariant()));
        }

        parameters.Sort((left, right) =>
        {
            int comparison = StringComparer.Ordinal.Compare(left.Key, right.Key);
            return comparison != 0 ? comparison : StringComparer.Ordinal.Compare(left.Value, right.Value);
        });

        if (parameters.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder queryBuilder = new();
        foreach (KeyValuePair<string, string> parameter in parameters)
        {
            queryBuilder.Append(queryBuilder.Length == 0 ? '?' : '&');
            queryBuilder.Append(parameter.Key);
            if (!string.IsNullOrEmpty(parameter.Value))
            {
                queryBuilder.Append('=').Append(parameter.Value);
            }
        }

        return queryBuilder.ToString();
    }

    private static string Canonicalize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        string normalized = input.ToLowerInvariant();
        normalized = normalized.Replace("ç", "c").Replace("ñ", "n");
        normalized = normalized.Normalize(NormalizationForm.FormD);

        StringBuilder builder = new(normalized.Length);
        foreach (char character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                builder.Append('-');
                continue;
            }

            if (char.IsLetterOrDigit(character) || character == '-')
            {
                builder.Append(character);
            }
        }

        string result = builder.ToString();
        while (result.Contains("--", StringComparison.Ordinal))
        {
            result = result.Replace("--", "-", StringComparison.Ordinal);
        }

        return result.Trim('-');
    }
}
