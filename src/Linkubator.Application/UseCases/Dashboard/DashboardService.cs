using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Logging;
using Linkubator.Application.Services;
using Microsoft.Extensions.Logging;

namespace Linkubator.Application.UseCases.Dashboard;

public sealed class DashboardService : ApplicationServiceBase, IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(ILogger<DashboardService> logger, ICurrentUser currentUser, IDashboardRepository dashboardRepository)
        : base(logger, currentUser)
    {
        ArgumentNullException.ThrowIfNull(dashboardRepository);
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Result<DashboardDto>> GetDashboard(CancellationToken cancellationToken = default)
    {
        Result authentication = EnsureAuthenticated();
        if (!authentication.IsSuccess)
        {
            return Result<DashboardDto>.Failure(authentication.Errors);
        }

        Result<DashboardDto> result = await Execute(
            new { UserId = CurrentUser.UserId },
            cancellationToken,
            async (_, token) => Result<DashboardDto>.Success(await _dashboardRepository.GetByUser(CurrentUser.UserId, token)),
            "Dashboard load failed for user");
        if (result.IsSuccess)
        {
            SafeLogger.Log(Logger, LogLevel.Information, null, "Dashboard loaded for user", UserContext);
        }

        return result;
    }
}
