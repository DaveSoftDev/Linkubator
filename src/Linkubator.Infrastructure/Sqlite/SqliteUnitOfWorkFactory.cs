using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Contracts;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteUnitOfWorkFactory(ISqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public IUnitOfWork Get(string contextName)
    {
        if (!string.Equals(contextName, UnitOfWorkContexts.Linkubator, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unsupported unit of work context '{contextName}'.");
        }

        return new SqliteUnitOfWork(_connectionFactory.CreateOpenConnection());
    }
}
