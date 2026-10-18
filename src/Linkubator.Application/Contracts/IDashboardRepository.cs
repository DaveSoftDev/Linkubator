using Linkubator.Application.Dtos.Models;

namespace Linkubator.Application.Contracts;

public interface IDashboardRepository
{
    Task<DashboardDto> GetByUser(long userId, CancellationToken cancellationToken = default);
}
