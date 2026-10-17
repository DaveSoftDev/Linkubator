using Linkubator.Application.Contracts;

namespace Linkubator.Application.Models;

public sealed record CurrentUser(long UserId) : ICurrentUser
{
    public bool IsAuthenticated => UserId > 0;
}
