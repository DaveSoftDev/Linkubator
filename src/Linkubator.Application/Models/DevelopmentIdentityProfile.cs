namespace Linkubator.Application.Models;

public sealed record DevelopmentIdentityProfile(long UserId, string Name, string Alias, string Email)
{
    public bool IsValid => UserId > 0 && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Alias)
        && !string.IsNullOrWhiteSpace(Email);
}