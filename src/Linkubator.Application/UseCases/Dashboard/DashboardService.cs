using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Application.UseCases.Dashboard;

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
            SafeLogger.Log(_logger, LogLevel.Warning, null, "Dashboard request rejected for user; not authenticated", new { UserId = _currentUser.UserId });
            return Result<DashboardDto>.Failure(DashboardErrors.GetNotAuthenticated());
        }

        DashboardDto dashboard;
        try
        {
            dashboard = await _dashboardRepository.GetByUser(_currentUser.UserId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SafeLogger.Log(_logger, LogLevel.Error, exception, "Dashboard load failed for user", new { UserId = _currentUser.UserId });
            throw;
        }
        SafeLogger.Log(_logger, LogLevel.Information, null, "Dashboard loaded for user", new { UserId = _currentUser.UserId });
        return Result<DashboardDto>.Success(dashboard);
    }
}
