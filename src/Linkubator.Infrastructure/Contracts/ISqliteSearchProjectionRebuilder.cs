namespace Linkubator.Infrastructure.Contracts;

public interface ISqliteSearchProjectionRebuilder
{
    Task Rebuild(CancellationToken cancellationToken = default);
}
