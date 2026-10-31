namespace Linkubator.Application.Contracts;

public interface IDevelopmentIdentityProvider
{
    Task<Linkubator.Application.Models.DevelopmentIdentityProfile> GetProfile(CancellationToken cancellationToken = default);
}
