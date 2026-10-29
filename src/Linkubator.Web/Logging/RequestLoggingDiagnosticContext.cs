using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace Linkubator.Web.Logging;

public static class RequestLoggingDiagnosticContext
{
    public static void Enrich(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        string requestPath = httpContext.Request.Path.Value ?? string.Empty;
        string requestQueryString = httpContext.Request.QueryString.Value ?? string.Empty;
        string loggedQueryString = requestPath.StartsWith("/app", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : requestQueryString;
        if (IsPrivateList(requestPath)
            && httpContext.Request.Query.TryGetValue("q", out StringValues search)
            && search.Count > 0
            && search[0] is string searchValue)
        {
            loggedQueryString = QueryString.Create("q", searchValue).Value ?? string.Empty;
        }

        diagnosticContext.Set("RequestPath", requestPath);
        diagnosticContext.Set("RequestQueryString", loggedQueryString);
    }

    private static bool IsPrivateList(string requestPath)
        => string.Equals(requestPath.TrimEnd('/'), Linkubator.Web.Constants.AppRoutes.Links, StringComparison.OrdinalIgnoreCase)
            || string.Equals(requestPath.TrimEnd('/'), Linkubator.Web.Constants.AppRoutes.Collections, StringComparison.OrdinalIgnoreCase);
}
