namespace Linkubator.Domain.Policies;

/// <summary>
/// Resultado del análisis sintáctico de una entrada que puede contener una autoridad URI.
/// </summary>
public sealed record UriAuthority(
    string? Scheme,
    string? Host,
    string? Port,
    bool IsProtocolRelative)
{
    public bool HasExplicitScheme => Scheme is not null;
    public bool HasPort => Port is not null;
    public bool HasHost => Host is not null;
}
