namespace Linkubator.Application;

public interface ICurrentUser
{
    long UserId { get; }

    bool IsAuthenticated { get; }
}

public interface IUserContext : ICurrentUser
{
}
