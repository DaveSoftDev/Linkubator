namespace Linkubator.Application;

public interface IDashboardService
{
    Task<Result<DashboardDto>> GetDashboard(CancellationToken cancellationToken = default);
}
