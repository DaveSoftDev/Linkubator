using Linkubator.Application.Contracts;
using Linkubator.Application.Results;
using Linkubator.Application.Services;

namespace Linkubator.Tests.Application;

public sealed class UnitOfWorkFactoryExtensionsTests
{
    [Fact]
    public async Task ExecuteInTransaction_WhenFactoryIsNull_ThrowsArgumentNullException()
    {
        IUnitOfWorkFactory factory = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            UnitOfWorkFactoryExtensions.ExecuteInTransaction(factory, "context", (_, _) => Task.FromResult(42)));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            UnitOfWorkFactoryExtensions.ExecuteInTransaction(factory, "context", (_, _) => Task.CompletedTask));
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenActionIsNull_ThrowsArgumentNullException()
    {
        IUnitOfWorkFactory factory = new RecordingUnitOfWorkFactory();

        Func<IUnitOfWork, CancellationToken, Task<int>> action = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            UnitOfWorkFactoryExtensions.ExecuteInTransaction(factory, "context", action));

        Func<IUnitOfWork, CancellationToken, Task> voidAction = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            UnitOfWorkFactoryExtensions.ExecuteInTransaction(factory, "context", voidAction));
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenGenericActionReturnsFailureResult_RollsBackWithoutCommit()
    {
        RecordingUnitOfWorkFactory factory = new();
        ApplicationError error = ApplicationError.Validation("INVALID");

        Result<int> result = await factory.ExecuteInTransaction("context", (_, _) => Task.FromResult(Result<int>.Failure(error)));

        Assert.False(result.IsSuccess);
        Assert.Equal("context", factory.ContextName);
        Assert.Equal(1, factory.UnitOfWork.BeginCount);
        Assert.Equal(0, factory.UnitOfWork.CommitCount);
        Assert.Equal(1, factory.UnitOfWork.RollbackCount);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenGenericActionThrows_RollsBackWithoutCommit()
    {
        RecordingUnitOfWorkFactory factory = new();
        InvalidOperationException expected = new("boom");

        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            factory.ExecuteInTransaction("context", (_, _) => throw expected));

        Assert.Same(expected, thrown);
        Assert.Equal(1, factory.UnitOfWork.BeginCount);
        Assert.Equal(0, factory.UnitOfWork.CommitCount);
        Assert.Equal(1, factory.UnitOfWork.RollbackCount);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenGenericActionReturnsPlainValue_CommitsAndReturnsValue()
    {
        RecordingUnitOfWorkFactory factory = new();

        int result = await factory.ExecuteInTransaction("context", (_, _) => Task.FromResult(42));

        Assert.Equal(42, result);
        Assert.Equal(1, factory.UnitOfWork.BeginCount);
        Assert.Equal(1, factory.UnitOfWork.CommitCount);
        Assert.Equal(0, factory.UnitOfWork.RollbackCount);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenVoidActionSucceeds_Commits()
    {
        RecordingUnitOfWorkFactory factory = new();

        await factory.ExecuteInTransaction("context", (_, _) => Task.CompletedTask);

        Assert.Equal(1, factory.UnitOfWork.BeginCount);
        Assert.Equal(1, factory.UnitOfWork.CommitCount);
        Assert.Equal(0, factory.UnitOfWork.RollbackCount);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenVoidActionThrows_RollsBackAndDisposes()
    {
        RecordingUnitOfWorkFactory factory = new();
        InvalidOperationException expected = new("boom");

        InvalidOperationException actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            factory.ExecuteInTransaction("context", (_, _) => Task.FromException(expected)));

        Assert.Same(expected, actual);
        Assert.Equal(1, factory.UnitOfWork.RollbackCount);
        Assert.Equal(0, factory.UnitOfWork.CommitCount);
        Assert.Equal(1, factory.UnitOfWork.DisposeCount);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenActionIsCancelled_DisposesWithoutCommit()
    {
        RecordingUnitOfWorkFactory factory = new();
        using CancellationTokenSource cancellation = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => factory.ExecuteInTransaction("context", (_, token) =>
        {
            cancellation.Cancel();
            return Task.FromCanceled(token);
        }, cancellation.Token));

        Assert.Equal(0, factory.UnitOfWork.CommitCount);
        Assert.Equal(1, factory.UnitOfWork.DisposeCount);
    }

    private sealed class RecordingUnitOfWorkFactory : IUnitOfWorkFactory
    {
        public RecordingUnitOfWork UnitOfWork { get; } = new();

        public string? ContextName { get; private set; }

        public IUnitOfWork Get(string contextName)
        {
            ContextName = contextName;
            return UnitOfWork;
        }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int BeginCount { get; private set; }
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }
        public int DisposeCount { get; private set; }

        public TRepository GetRepository<TRepository>() where TRepository : class
            => throw new NotSupportedException();

        public Task Begin(CancellationToken cancellationToken = default)
        {
            BeginCount++;
            return Task.CompletedTask;
        }

        public Task Commit(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.CompletedTask;
        }

        public Task Rollback(CancellationToken cancellationToken = default)
        {
            RollbackCount++;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }
}
