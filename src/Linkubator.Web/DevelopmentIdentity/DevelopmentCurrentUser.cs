using ApplicationContracts = Linkubator.Application.Contracts;

namespace Linkubator.Web.DevelopmentIdentity;

public sealed class DevelopmentCurrentUser : ApplicationContracts.ICurrentUser, IWebUserProfile
{
    public long UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Alias { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public bool IsAuthenticated => UserId > 0;

    internal void SetUserId(long userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        UserId = userId;
    }

    internal void SetProfile(string name, string alias, string email)
    {
        Name = name.Trim();
        Alias = alias.Trim();
        Email = email.Trim();
    }
}