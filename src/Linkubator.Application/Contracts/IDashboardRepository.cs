namespace Linkubator.Application;

public interface IDashboardRepository
{
    Task<DashboardDto> GetByUser(long userId, CancellationToken cancellationToken = default);
}
