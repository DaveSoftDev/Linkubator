using Microsoft.AspNetCore.Http;
using Serilog;

namespace Linkubator.Web.Logging;

public static class RequestLoggingDiagnosticContext
{
    public static void Enrich(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        string requestPath = httpContext.Request.Path.Value ?? string.Empty;
        string requestQueryString = httpContext.Request.QueryString.Value ?? string.Empty;
        string safeRequestPath = GetSafeRequestPath(requestPath, requestQueryString);
        bool omitQueryString = IsPrivateListQuery(requestPath, requestQueryString)
            || !string.Equals(safeRequestPath, requestPath, StringComparison.Ordinal);

        diagnosticContext.Set("RequestPath", safeRequestPath);
        diagnosticContext.Set("RequestQueryString", omitQueryString ? string.Empty : requestQueryString);
    }

    public static string GetSafeRequestPath(string requestPath, string requestQueryString)
        => ContainsTokenLikeParameter(requestQueryString) || ContainsSensitivePathToken(requestPath)
            ? "[redacted-token-url]"
            : requestPath;

    public static bool IsPrivateListQuery(string requestPath, string requestQueryString)
        => requestPath.StartsWith("/app", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(requestQueryString);

    public static bool ContainsTokenLikeParameter(string requestQueryString)
        => !string.IsNullOrEmpty(requestQueryString)
            && (requestQueryString.Contains("token=", StringComparison.OrdinalIgnoreCase)
                || requestQueryString.Contains("access_token=", StringComparison.OrdinalIgnoreCase)
                || requestQueryString.Contains("refresh_token=", StringComparison.OrdinalIgnoreCase)
                || requestQueryString.Contains("code=", StringComparison.OrdinalIgnoreCase)
                || requestQueryString.Contains("sig=", StringComparison.OrdinalIgnoreCase)
                || requestQueryString.Contains("signature=", StringComparison.OrdinalIgnoreCase)
                || requestQueryString.Contains("api_key=", StringComparison.OrdinalIgnoreCase)
                || requestQueryString.Contains("key=", StringComparison.OrdinalIgnoreCase));

    public static bool ContainsSensitivePathToken(string requestPath)
        => requestPath.Contains("token", StringComparison.OrdinalIgnoreCase)
            || requestPath.Contains("code", StringComparison.OrdinalIgnoreCase)
            || requestPath.Contains("sig", StringComparison.OrdinalIgnoreCase)
            || requestPath.Contains("api_key", StringComparison.OrdinalIgnoreCase)
            || requestPath.Contains("key", StringComparison.OrdinalIgnoreCase);
}
