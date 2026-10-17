namespace Linkubator.Application.Contracts;

public interface IDevelopmentIdentityProvider
{
    Task<long> GetUserId(CancellationToken cancellationToken = default);
}
