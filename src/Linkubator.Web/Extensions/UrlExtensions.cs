namespace Linkubator.Web.Extensions;

public static class UrlExtensions
{
    /// <summary>
    /// Obtiene el dominio de una URL para mostrarlo en la interfaz.
    /// </summary>
    public static string GetDomain(this string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ? uri.Host : url;
}
