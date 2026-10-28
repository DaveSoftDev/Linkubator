namespace Linkubator.Web.Extensions;

public static class UrlExtensions
{
    public static string GetDomain(this string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ? uri.Host : url;
}
