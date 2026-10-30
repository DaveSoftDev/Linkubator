using Microsoft.AspNetCore.Http;
using ApplicationContracts = Linkubator.Application.Contracts;
using WebDevelopmentIdentity = Linkubator.Web.DevelopmentIdentity;

namespace Linkubator.Tests.Integration;

public sealed class DevelopmentIdentityMiddlewareTests
{
    [Theory]
    [InlineData("/app")]
    [InlineData("/app/dashboard")]
    public async Task InvokeAsync_WhenRequestIsPrivate_SetsCurrentUserFromProvider(string path)
    {
        const long expectedUserId = 42;
        bool nextCalled = false;
        WebDevelopmentIdentity.DevelopmentIdentityMiddleware middleware = new(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new();
        context.Request.Path = path;
        context.Request.QueryString = new QueryString("?userId=99");
        WebDevelopmentIdentity.DevelopmentCurrentUser currentUser = new();
        FakeDevelopmentIdentityProvider identityProvider = new(expectedUserId);

        await middleware.InvokeAsync(context, identityProvider, currentUser);

        Assert.True(nextCalled);
        Assert.Equal(expectedUserId, currentUser.UserId);
        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(1, identityProvider.CallCount);
        Assert.Equal(("Ana Pérez", "ana-perez", "ana@example.test"), (currentUser.Name, currentUser.Alias, currentUser.Email));
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/application")]
    [InlineData("/app-public")]
    public async Task InvokeAsync_WhenRequestIsPublic_DoesNotResolveDevelopmentIdentity(string path)
    {
        bool nextCalled = false;
        WebDevelopmentIdentity.DevelopmentIdentityMiddleware middleware = new(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new();
        context.Request.Path = path;
        WebDevelopmentIdentity.DevelopmentCurrentUser currentUser = new();
        FakeDevelopmentIdentityProvider identityProvider = new(42);

        await middleware.InvokeAsync(context, identityProvider, currentUser);

        Assert.True(nextCalled);
        Assert.False(currentUser.IsAuthenticated);
        Assert.Equal(0, identityProvider.CallCount);
    }

    [Fact]
    public async Task InvokeAsync_WhenRequestIsPrivate_ForwardsRequestCancellation()
    {
        using CancellationTokenSource cancellation = new();
        DefaultHttpContext context = new();
        context.Request.Path = "/app";
        context.RequestAborted = cancellation.Token;
        FakeDevelopmentIdentityProvider provider = new(42);
        WebDevelopmentIdentity.DevelopmentIdentityMiddleware middleware = new(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, provider, new WebDevelopmentIdentity.DevelopmentCurrentUser());

        Assert.Equal(cancellation.Token, provider.ReceivedCancellationToken);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task InvokeAsync_WhenProviderFails_DoesNotInvokeNext()
    {
        bool nextCalled = false;
        WebDevelopmentIdentity.DevelopmentIdentityMiddleware middleware = new(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new();
        context.Request.Path = "/app";
        WebDevelopmentIdentity.DevelopmentCurrentUser user = new();
        FailingDevelopmentIdentityProvider provider = new();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(context, provider, user));

        Assert.Equal("Identity unavailable", exception.Message);
        Assert.False(nextCalled);
        Assert.False(user.IsAuthenticated);
    }

    private sealed class FailingDevelopmentIdentityProvider : ApplicationContracts.IDevelopmentIdentityProvider
    {
        public Task<Linkubator.Application.Models.DevelopmentIdentityProfile> GetProfile(CancellationToken cancellationToken = default)
            => Task.FromException<Linkubator.Application.Models.DevelopmentIdentityProfile>(new InvalidOperationException("Identity unavailable"));
    }

    private sealed class FakeDevelopmentIdentityProvider(long userId) : ApplicationContracts.IDevelopmentIdentityProvider
    {
        public int CallCount { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<Linkubator.Application.Models.DevelopmentIdentityProfile> GetProfile(CancellationToken cancellationToken = default)
        {
            CallCount++;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(new Linkubator.Application.Models.DevelopmentIdentityProfile(userId, "Ana Pérez", "ana-perez", "ana@example.test"));
        }
    }
}