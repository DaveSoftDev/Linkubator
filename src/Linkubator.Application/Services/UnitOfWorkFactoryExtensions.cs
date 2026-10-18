namespace Linkubator.Application.Services;

public static class UnitOfWorkFactoryExtensions
{
    public static async Task<TResult> ExecuteInTransaction<TResult>(this IUnitOfWorkFactory factory, string contextName, Func<IUnitOfWork, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(action);

        await using IUnitOfWork unitOfWork = factory.Get(contextName);
        await unitOfWork.Begin(cancellationToken);
        bool committed = false;

        try
        {
            TResult result = await action(unitOfWork, cancellationToken);
            if (result is Result typedResult && !typedResult.IsSuccess)
            {
                return result;
            }

            await unitOfWork.Commit(cancellationToken);
            committed = true;
            return result;
        }
        finally
        {
            if (!committed)
            {
                await unitOfWork.Rollback(cancellationToken);
            }
        }
    }

    public static async Task ExecuteInTransaction(this IUnitOfWorkFactory factory, string contextName, Func<IUnitOfWork, CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(action);

        await ExecuteInTransaction(factory, contextName, async (unitOfWork, token) => { await action(unitOfWork, token); return true; }, cancellationToken);
    }
}
