namespace Linkubator.Application;

public interface IDevelopmentIdentityProvider
{
    Task<long> GetUserId(CancellationToken cancellationToken = default);
}
