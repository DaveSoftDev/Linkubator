using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Linkubator.Application.Enums;
using Linkubator.Application.Models;
using Linkubator.Application.Results;
using Linkubator.Application.UseCases.Dashboard;
using Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Tests.Application;

public class DashboardServiceTests
{
    [Fact]
    public void Constructor_WhenDependenciesAreNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DashboardService(null!, new CurrentUser(7), new FakeDashboardRepository()));
        Assert.Throws<ArgumentNullException>(() => new DashboardService(NullLogger<DashboardService>.Instance, null!, new FakeDashboardRepository()));
        Assert.Throws<ArgumentNullException>(() => new DashboardService(NullLogger<DashboardService>.Instance, new CurrentUser(7), null!));
    }

    [Fact]
    public async Task GetDashboard_WhenUserIsUnauthenticated_ReturnsResourceNotAccessible()
    {
        DashboardService service = new(NullLogger<DashboardService>.Instance, new CurrentUser(0), new FakeDashboardRepository());

        Result<DashboardDto> result = await service.GetDashboard();

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, result.Errors[0].Category);
        Assert.Equal("DASHBOARD_GET_NOT_AUTHENTICATED", result.Errors[0].Code);
        Assert.Equal(DashboardErrors.UserIdField, result.Errors[0].Field);
    }

    [Fact]
    public async Task GetDashboard_WhenUserIsAuthenticated_ReturnsDashboardData()
    {
        FakeDashboardRepository repository = new();
        DashboardService service = new(NullLogger<DashboardService>.Instance, new CurrentUser(7), repository);
        CollectionDto collection = new(11, "My collection", "my-collection", "desc", true, 2, new DateTime(2026, 10, 1));
        LinkDto recentLink = new(21, 11, "https://example.com/path", "https://example.com/path", "Example", "Preview", "https://example.com/icon.png", true, new DateTime(2026, 10, 2));
        repository.Dashboard = new DashboardDto(1, 1, new[] { collection }, new[] { recentLink });

        Result<DashboardDto> result = await service.GetDashboard();

        Assert.True(result.IsSuccess);
        Assert.Equal(7, repository.LastUserId);
        Assert.Equal(1, result.Value!.CollectionCount);
        Assert.Equal(1, result.Value.LinkCount);
        Assert.Single(result.Value.Collections);
        Assert.Single(result.Value.RecentLinks);
    }

    [Fact]
    public async Task GetDashboard_WhenRepositoryThrowsDomainException_ReturnsDomainFailure()
    {
        FakeDashboardRepository repository = new()
        {
            Throw = new NotValidInputException()
        };
        DashboardService service = new(NullLogger<DashboardService>.Instance, new CurrentUser(7), repository);

        Result<DashboardDto> result = await service.GetDashboard();

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
        Assert.Equal("NotValidInput", result.Errors[0].Code);
    }

    private sealed class FakeDashboardRepository : IDashboardRepository
    {
        public Exception? Throw { get; set; }

        public DashboardDto Dashboard { get; set; } = new DashboardDto(0, 0, Array.Empty<CollectionDto>(), Array.Empty<LinkDto>());

        public long LastUserId { get; private set; }

        public Task<DashboardDto> GetByUser(long userId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            if (Throw is not null)
            {
                return Task.FromException<DashboardDto>(Throw);
            }

            return Task.FromResult(Dashboard);
        }
    }
}
