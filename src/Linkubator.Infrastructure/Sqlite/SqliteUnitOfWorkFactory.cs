using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Contracts;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    /// <summary>
    /// Crea una fábrica de unidades de trabajo con la fábrica de conexiones indicada.
    /// </summary>
    public SqliteUnitOfWorkFactory(ISqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Crea una unidad de trabajo para el contexto solicitado.
    /// </summary>
    public IUnitOfWork Get(string contextName)
    {
        if (!string.Equals(contextName, UnitOfWorkContexts.Linkubator, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unsupported unit of work context '{contextName}'.");
        }

        return new SqliteUnitOfWork(_connectionFactory.CreateOpenConnection());
    }
}
