namespace Linkubator.Web.Options;

public sealed class PublicOriginOptions
{
    public const string ConfigurationKey = "PublicOrigin";

    public string Origin { get; set; } = string.Empty;
}
