using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Logging;
using Linkubator.Application.Services;
using DomainExceptions = Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.UseCases.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ILogger<DashboardService> logger, ICurrentUser currentUser, IDashboardRepository dashboardRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(dashboardRepository);

        _logger = logger;
        _currentUser = currentUser;
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Result<DashboardDto>> GetDashboard(CancellationToken cancellationToken = default)
    {
        if (!CurrentUserGuard.EnsureAuthenticated(_currentUser, _logger, DashboardErrors.GetNotAuthenticated, "Dashboard request rejected for user; not authenticated", out Result<DashboardDto> authenticationFailure))
        {
            return authenticationFailure;
        }

        DashboardDto dashboard;
        try
        {
            dashboard = await _dashboardRepository.GetByUser(_currentUser.UserId, cancellationToken);
        }
        catch (DomainExceptions.DomainException exception)
        {
            SafeLogger.Log(_logger, LogLevel.Warning, exception, "Dashboard load failed for user; domain validation issue", new { UserId = _currentUser.UserId });
            return exception.FailDomain<DashboardDto>();
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
