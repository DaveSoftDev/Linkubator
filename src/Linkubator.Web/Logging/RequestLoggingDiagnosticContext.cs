using Microsoft.AspNetCore.Http;
using Serilog;

namespace Linkubator.Web.Logging;

public static class RequestLoggingDiagnosticContext
{
    public static void Enrich(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        string requestPath = httpContext.Request.Path.Value ?? string.Empty;
        string requestQueryString = httpContext.Request.QueryString.Value ?? string.Empty;

        if (IsPrivateListQuery(requestPath, requestQueryString))
        {
            diagnosticContext.Set("RequestPath", requestPath);
            diagnosticContext.Set("RequestQueryString", string.Empty);
            return;
        }

        if (ContainsTokenLikeParameter(requestQueryString) || ContainsSensitivePathToken(requestPath))
        {
            diagnosticContext.Set("RequestPath", "[redacted-token-url]");
            diagnosticContext.Set("RequestQueryString", string.Empty);
            return;
        }

        diagnosticContext.Set("RequestPath", requestPath);
        diagnosticContext.Set("RequestQueryString", requestQueryString);
    }

    public static bool IsPrivateListQuery(string requestPath, string requestQueryString)
        => requestPath.StartsWith("/app", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(requestQueryString)
            && requestQueryString.Contains("q=", StringComparison.OrdinalIgnoreCase);

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
