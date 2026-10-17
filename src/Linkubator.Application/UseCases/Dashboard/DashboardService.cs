using Linkubator.Application.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Application;

public sealed class DashboardService : IDashboardService
{
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ICurrentUser currentUser, IDashboardRepository dashboardRepository, ILogger<DashboardService>? logger = null)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
        _logger = logger ?? NullLogger<DashboardService>.Instance;
    }

    public async Task<Result<DashboardDto>> GetDashboard(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogWarning("Dashboard request rejected for user {UserId}: not authenticated", _currentUser.UserId);
            return Result<DashboardDto>.Failure(DashboardErrors.GetNotAuthenticated());
        }

        DashboardDto dashboard;
        try
        {
            dashboard = await _dashboardRepository.GetByUser(_currentUser.UserId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError("Dashboard load failed for user {UserId}", _currentUser.UserId);
            throw;
        }
        _logger.LogInformation("Dashboard loaded for user {UserId}", _currentUser.UserId);
        return Result<DashboardDto>.Success(dashboard);
    }
}
