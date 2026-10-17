using Linkubator.Application.Constants;

namespace Linkubator.Application;

public sealed class DashboardService : IDashboardService
{
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(ICurrentUser currentUser, IDashboardRepository dashboardRepository)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
    }

    public async Task<Result<DashboardDto>> GetDashboard(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<DashboardDto>.Failure(DashboardErrors.GetNotAuthenticated());
        }

        DashboardDto dashboard = await _dashboardRepository.GetByUser(_currentUser.UserId, cancellationToken);
        return Result<DashboardDto>.Success(dashboard);
    }
}
