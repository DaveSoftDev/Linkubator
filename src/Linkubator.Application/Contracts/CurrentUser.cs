namespace Linkubator.Application;

public sealed record CurrentUser(long UserId) : ICurrentUser
{
    public bool IsAuthenticated => UserId > 0;
}
