namespace Linkubator.Application.Contracts;

public interface ICurrentUser
{
    long UserId { get; }

    bool IsAuthenticated { get; }
}

public interface IUserContext : ICurrentUser
{
}
