using ApplicationContracts = Linkubator.Application.Contracts;

namespace Linkubator.Web.DevelopmentIdentity;

public sealed class DevelopmentCurrentUser : ApplicationContracts.ICurrentUser
{
    public long UserId { get; private set; }

    public bool IsAuthenticated => UserId > 0;

    internal void SetUserId(long userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        UserId = userId;
    }
}