namespace Linkubator.Infrastructure.Options;

public sealed class DevelopmentIdentityOptions
{
    public string Email { get; set; } = "user@linkubator.test";

    public string Name { get; set; } = "Demo User";

    public string Alias { get; set; } = "my-demo-user";

    public string PasswordHash { get; set; } = string.Empty;
}
