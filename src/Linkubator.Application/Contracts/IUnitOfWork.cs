namespace Linkubator.Application.Contracts;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    TRepository GetRepository<TRepository>() where TRepository : class;

    Task Begin(CancellationToken cancellationToken = default);

    Task Commit(CancellationToken cancellationToken = default);

    Task Rollback(CancellationToken cancellationToken = default);
}
