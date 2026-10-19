using Microsoft.AspNetCore.Http;
using ApplicationContracts = Linkubator.Application.Contracts;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;

namespace Linkubator.Tests.Integration;

public sealed class DevelopmentIdentityMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenRequestIsPrivate_SetsCurrentUserFromProvider()
    {
        const long expectedUserId = 42;
        bool nextCalled = false;
        WebDevelopmentIdentity.DevelopmentIdentityMiddleware middleware = new(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new();
        context.Request.Path = "/app/dashboard";
        context.Request.QueryString = new QueryString("?userId=99");
        WebDevelopmentIdentity.DevelopmentCurrentUser currentUser = new();
        FakeDevelopmentIdentityProvider identityProvider = new(expectedUserId);

        await middleware.InvokeAsync(context, identityProvider, currentUser);

        Assert.True(nextCalled);
        Assert.Equal(expectedUserId, currentUser.UserId);
        Assert.True(currentUser.IsAuthenticated);
    }

    [Fact]
    public async Task InvokeAsync_WhenRequestIsPublic_DoesNotResolveDevelopmentIdentity()
    {
        bool nextCalled = false;
        WebDevelopmentIdentity.DevelopmentIdentityMiddleware middleware = new(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new();
        context.Request.Path = "/";
        WebDevelopmentIdentity.DevelopmentCurrentUser currentUser = new();
        FakeDevelopmentIdentityProvider identityProvider = new(42);

        await middleware.InvokeAsync(context, identityProvider, currentUser);

        Assert.True(nextCalled);
        Assert.False(currentUser.IsAuthenticated);
        Assert.Equal(0, identityProvider.CallCount);
    }

    private sealed class FakeDevelopmentIdentityProvider(long userId) : ApplicationContracts.IDevelopmentIdentityProvider
    {
        public int CallCount { get; private set; }

        public Task<long> GetUserId(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(userId);
        }
    }
}