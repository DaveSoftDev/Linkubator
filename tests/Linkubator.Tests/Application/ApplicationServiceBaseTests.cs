using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Application.Enums;
using Linkubator.Application.Models;
using Linkubator.Application.Results;
using Linkubator.Application.Services;
using Linkubator.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Linkubator.Tests.Application;

public class ApplicationServiceBaseTests
{
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TestApplicationService(null!));
        Assert.Throws<ArgumentNullException>(() => new TestApplicationService(null!, new CurrentUser(7)));
        Assert.Throws<ArgumentNullException>(() => new TestApplicationService(null!, new CurrentUser(7), new TrackingUnitOfWorkFactory()));
    }

    [Fact]
    public void Constructor_WhenCurrentUserIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TestApplicationService(NullLogger.Instance, null!));
        Assert.Throws<ArgumentNullException>(() => new TestApplicationService(NullLogger.Instance, null!, new TrackingUnitOfWorkFactory()));
    }

    [Fact]
    public void Constructor_WhenUnitOfWorkFactoryIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TestApplicationService(NullLogger.Instance, new CurrentUser(7), null!));
    }

    [Fact]
    public void CurrentUser_WhenNoCurrentUserExists_ThrowsInvalidOperationException()
    {
        TestApplicationService service = new(NullLogger.Instance);

        Assert.Throws<InvalidOperationException>(() => _ = service.GetCurrentUser());
    }

    [Fact]
    public void UserContext_WhenUserIsMissing_ReturnsEmptyAnonymousObject()
    {
        TestApplicationService service = new(NullLogger.Instance);

        object context = service.GetUserContext();

        Assert.NotNull(context);
        Assert.Empty(context.GetType().GetProperties());
    }

    [Fact]
    public void UserContext_WhenUserExists_IncludesUserId()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        object context = service.GetUserContext();

        Assert.Equal(7L, context.GetType().GetProperty("UserId")!.GetValue(context));
    }

    [Fact]
    public void EnsureAuthenticated_WhenUserIsAuthenticated_ReturnsSuccess()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        Result result = service.CallEnsureAuthenticated();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void EnsureAuthenticated_WhenUserIsUnauthenticated_ReturnsNotAuthenticated()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(0));

        Result result = service.CallEnsureAuthenticated();

        Assert.False(result.IsSuccess);
        Assert.Equal(CommonErrors.NotAuthenticatedCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenOperationSucceeds_CommitsAndReturnsSuccess()
    {
        TrackingUnitOfWorkFactory factory = new();
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7), factory);

        Result<string> result = await service.RunExecuteInTransaction("request", cancellationToken: default, (request, work, token) => Task.FromResult(Result<string>.Success(request)), "failure");

        Assert.True(result.IsSuccess);
        Assert.Equal("request", result.Value);
        Assert.Equal(1, factory.UnitOfWork.BeginCalls);
        Assert.Equal(1, factory.UnitOfWork.CommitCalls);
        Assert.Equal(new[] { "Begin", "Commit", "Dispose" }, factory.UnitOfWork.Events);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenOperationReturnsFailure_DoesNotCommitAndReturnsFailure()
    {
        TrackingUnitOfWorkFactory factory = new();
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7), factory);

        Result<string> result = await service.RunExecuteInTransaction<string, string>(
            "request",
            cancellationToken: default,
            (request, work, token) => Task.FromResult(Result<string>.Failure(new ApplicationError("ERR", ApplicationErrorCategory.Validation))),
            "failure");

        Assert.False(result.IsSuccess);
        Assert.Equal("ERR", Assert.Single(result.Errors).Code);
        Assert.Equal(1, factory.UnitOfWork.BeginCalls);
        Assert.Equal(0, factory.UnitOfWork.CommitCalls);
        Assert.Equal(new[] { "Begin", "Dispose" }, factory.UnitOfWork.Events);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenDomainExceptionOccurs_ReturnsDomainFailure()
    {
        TrackingUnitOfWorkFactory factory = new();
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7), factory);

        Result<string> result = await service.RunExecuteInTransaction<string, string>(
            "request",
            cancellationToken: default,
            (_, _, _) => throw new TestDomainException("TEST_DOMAIN"),
            "failure");

        Assert.False(result.IsSuccess);
        Assert.Equal("TEST_DOMAIN", result.Errors[0].Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, result.Errors[0].Category);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenUnexpectedExceptionOccurs_Rethrows()
    {
        TrackingUnitOfWorkFactory factory = new();
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7), factory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunExecuteInTransaction<string, string>(
            "request",
            default,
            (_, _, _) => throw new InvalidOperationException("boom"),
            "failure"));
        Assert.Equal(new[] { "Begin", "Dispose" }, factory.UnitOfWork.Events);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenFactoryIsMissing_ThrowsInvalidOperationException()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunExecuteInTransaction<string>(
            "request",
            default,
            (_, _, _) => Task.FromResult(Result.Success()),
            "failure"));
    }

    [Fact]
    public async Task Execute_WhenOperationSucceeds_ReturnsSuccess()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        Result<int> result = await service.RunExecute<string, int>(
            "request",
            default,
            (_, _) => Task.FromResult(Result<int>.Success(42)),
            "failure");

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task Execute_WhenDomainExceptionOccurs_ReturnsFailure()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        Result<int> result = await service.RunExecute<string, int>(
            "request",
            default,
            (_, _) => throw new TestDomainException("THROWN"),
            "failure");

        Assert.False(result.IsSuccess);
        Assert.Equal("THROWN", result.Errors[0].Code);
    }

    [Fact]
    public async Task Execute_WhenUnexpectedExceptionOccurs_Rethrows()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunExecute<string, int>(
            "request",
            default,
            (_, _) => throw new InvalidOperationException("boom"),
            "failure"));
    }

    [Fact]
    public async Task ExecuteNonGeneric_WhenOperationSucceeds_ReturnsSuccess()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        Result result = await service.RunExecuteNonGeneric(
            "request",
            default,
            (_, _) => Task.FromResult(Result.Success()),
            "failure");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteNonGeneric_WhenDomainExceptionOccurs_ReturnsFailure()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        Result result = await service.RunExecuteNonGeneric(
            "request",
            default,
            (_, _) => throw new TestDomainException("NON_GENERIC"),
            "failure");

        Assert.False(result.IsSuccess);
        Assert.Equal("NON_GENERIC", result.Errors[0].Code);
    }

    [Fact]
    public async Task ExecuteNonGeneric_WhenUnexpectedExceptionOccurs_Rethrows()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunExecuteNonGeneric(
            "request",
            default,
            (_, _) => throw new InvalidOperationException("boom"),
            "failure"));
    }

    [Fact]
    public void Failure_WhenCalled_ReturnsWarningFailure()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        Result result = service.CallFailure(new ApplicationError("ERR", ApplicationErrorCategory.Validation), "message", new { Id = 1 });

        Assert.False(result.IsSuccess);
        Assert.Equal("ERR", result.Errors[0].Code);
    }

    [Fact]
    public void FailureOfT_WhenCalled_ReturnsWarningFailure()
    {
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7));

        Result<int> result = service.CallFailureOfT(42, new ApplicationError("ERR", ApplicationErrorCategory.Validation), "message", new { Id = 1 });

        Assert.False(result.IsSuccess);
        Assert.Equal("ERR", result.Errors[0].Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteInTransactionNonGeneric_WhenOperationCompletes_DisposesAndCommitsOnlySuccess(bool success)
    {
        TrackingUnitOfWorkFactory factory = new();
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7), factory);

        Result result = await service.RunExecuteInTransaction("request", default,
            (_, _, _) => Task.FromResult(success ? Result.Success() : Result.Failure(ApplicationError.Validation("ERR"))), "failure");

        Assert.Equal(success, result.IsSuccess);
        Assert.Equal(success ? new[] { "Begin", "Commit", "Dispose" } : new[] { "Begin", "Dispose" }, factory.UnitOfWork.Events);
        if (!success)
        {
            Assert.Equal("ERR", Assert.Single(result.Errors).Code);
        }
    }

    [Fact]
    public async Task ExecuteInTransactionNonGeneric_WhenOperationThrows_DisposesWithoutCommit()
    {
        TrackingUnitOfWorkFactory factory = new();
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7), factory);
        InvalidOperationException expected = new("boom");

        InvalidOperationException actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RunExecuteInTransaction("request", default, (_, _, _) => throw expected, "failure"));

        Assert.Same(expected, actual);
        Assert.Equal(new[] { "Begin", "Dispose" }, factory.UnitOfWork.Events);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteWithUnitOfWork_WhenOperationCompletes_DisposesWithoutTransaction(bool success)
    {
        TrackingUnitOfWorkFactory factory = new();
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7), factory);

        Result<string> result = await service.RunExecuteWithUnitOfWork("request", default,
            (request, _, _) => Task.FromResult(success ? Result<string>.Success(request) : Result<string>.Failure(ApplicationError.Validation("ERR"))), "failure");

        Assert.Equal(success, result.IsSuccess);
        Assert.Equal(new[] { "Dispose" }, factory.UnitOfWork.Events);
        if (!success)
        {
            Assert.Equal("ERR", Assert.Single(result.Errors).Code);
        }
    }

    [Fact]
    public async Task ExecuteWithUnitOfWork_WhenOperationThrows_DisposesAndPropagates()
    {
        TrackingUnitOfWorkFactory factory = new();
        TestApplicationService service = new(NullLogger.Instance, new CurrentUser(7), factory);
        InvalidOperationException expected = new("boom");

        InvalidOperationException actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RunExecuteWithUnitOfWork<string, string>("request", default, (_, _, _) => throw expected, "failure"));

        Assert.Same(expected, actual);
        Assert.Equal(new[] { "Dispose" }, factory.UnitOfWork.Events);
    }

    private sealed class TestApplicationService : ApplicationServiceBase
    {
        public TestApplicationService(ILogger logger)
            : base(logger)
        {
        }

        public TestApplicationService(ILogger logger, ICurrentUser currentUser)
            : base(logger, currentUser)
        {
        }

        public TestApplicationService(ILogger logger, ICurrentUser currentUser, IUnitOfWorkFactory unitOfWorkFactory)
            : base(logger, currentUser, unitOfWorkFactory)
        {
        }

        public ICurrentUser CurrentUserProperty => CurrentUser;

        public ICurrentUser GetCurrentUser() => CurrentUser;

        public object GetUserContext() => UserContext;

        public Result CallEnsureAuthenticated() => EnsureAuthenticated();

        public Task<Result<TResponse>> RunExecuteInTransaction<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, IUnitOfWork, CancellationToken, Task<Result<TResponse>>> operation,
            string failureMessage)
            => ExecuteInTransaction(request, cancellationToken, operation, failureMessage);

        public Task<Result> RunExecuteInTransaction<TRequest>(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, IUnitOfWork, CancellationToken, Task<Result>> operation,
            string failureMessage)
            => ExecuteInTransaction(request, cancellationToken, operation, failureMessage);

        public Task<Result<TResponse>> RunExecuteWithUnitOfWork<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, IUnitOfWork, CancellationToken, Task<Result<TResponse>>> operation,
            string failureMessage)
            => Execute(request, cancellationToken, operation, failureMessage);

        public Task<Result<TResponse>> RunExecute<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, CancellationToken, Task<Result<TResponse>>> operation,
            string failureMessage)
            => Execute(request, cancellationToken, operation, failureMessage);

        public Task<Result> RunExecute<TRequest>(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, CancellationToken, Task<Result>> operation,
            string failureMessage)
            => Execute(request, cancellationToken, operation, failureMessage);

        public Task<Result> RunExecuteNonGeneric(
            string request,
            CancellationToken cancellationToken,
            Func<string, CancellationToken, Task<Result>> operation,
            string failureMessage)
            => Execute(request, cancellationToken, operation, failureMessage);

        public Result CallFailure(ApplicationError error, string message, object? request = null)
            => Failure(error, message, request);

        public Result<int> CallFailureOfT(int value, ApplicationError error, string message, object? request = null)
            => Failure<int>(error, message, request);
    }

    private sealed class TrackingUnitOfWorkFactory : IUnitOfWorkFactory
    {
        public TrackingUnitOfWork UnitOfWork { get; } = new();

        public IUnitOfWork Get(string contextName) => UnitOfWork;
    }

    private sealed class TrackingUnitOfWork : IUnitOfWork
    {
        public int BeginCalls { get; private set; }
        public int CommitCalls { get; private set; }
        public int RollbackCalls { get; private set; }
        public List<string> Events { get; } = [];

        public TRepository GetRepository<TRepository>() where TRepository : class
            => throw new NotSupportedException();

        public Task Begin(CancellationToken cancellationToken = default)
        {
            BeginCalls++;
            Events.Add("Begin");
            return Task.CompletedTask;
        }

        public Task Commit(CancellationToken cancellationToken = default)
        {
            CommitCalls++;
            Events.Add("Commit");
            return Task.CompletedTask;
        }

        public Task Rollback(CancellationToken cancellationToken = default)
        {
            RollbackCalls++;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Events.Add("Dispose");
            return ValueTask.CompletedTask;
        }

        public void Dispose() => Events.Add("Dispose");
    }

    private sealed class TestDomainException : DomainException
    {
        public TestDomainException(string code)
            : base(code)
        {
        }
    }
}
