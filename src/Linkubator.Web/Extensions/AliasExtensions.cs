namespace Linkubator.Web.Extensions;

public static class AliasExtensions
{
    /// <summary>
    /// Obtiene el nombre de presentación a partir del alias indicado.
    /// </summary>
    public static string GetDisplayName(this string alias) => alias.Replace('-', ' ');
}
