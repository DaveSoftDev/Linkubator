using Linkubator.Application.Dtos.Models;

namespace Linkubator.Application.Contracts;

public interface IDashboardService
{
    Task<Result<DashboardDto>> GetDashboard(CancellationToken cancellationToken = default);
}
