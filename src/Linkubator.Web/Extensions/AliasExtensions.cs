namespace Linkubator.Web.Extensions;

public static class AliasExtensions
{
    public static string GetDisplayName(this string alias) => alias.Replace('-', ' ');
}
