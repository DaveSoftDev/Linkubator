using System.Data;
using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Sqlite;

public sealed class SqliteUnitOfWork : IUnitOfWork
{
    private readonly SqliteConnection _connection;
    private readonly Dictionary<Type, object> _repositories;
    private bool _disposed;
    private bool _transactionActive;

    /// <summary>
    /// Crea una unidad de trabajo y sus repositorios sobre la conexión SQLite indicada.
    /// </summary>
    public SqliteUnitOfWork(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _repositories = new Dictionary<Type, object>
        {
            [typeof(ICollectionRepository)] = new SqliteCollectionRepository(_connection),
            [typeof(ILinkRepository)] = new SqliteLinkRepository(_connection),
            [typeof(IDashboardRepository)] = new SqliteDashboardRepository(_connection),
            [typeof(IPublicReadRepository)] = new SqlitePublicReadRepository(_connection),
        };
    }

    /// <summary>
    /// Obtiene el repositorio registrado del tipo solicitado.
    /// </summary>
    public TRepository GetRepository<TRepository>() where TRepository : class
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SqliteUnitOfWork));
        }

        Type repositoryType = typeof(TRepository);
        if (!_repositories.TryGetValue(repositoryType, out object? repository))
        {
            throw new InvalidOperationException($"Repository type '{repositoryType.Name}' is not registered for this unit of work.");
        }

        return (TRepository)repository;
    }

    /// <summary>
    /// Inicia una transacción de escritura si no hay otra activa.
    /// </summary>
    public async Task Begin(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SqliteUnitOfWork));
        }

        if (_transactionActive)
        {
            return;
        }

        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }

        await _connection.ExecuteAsync(new CommandDefinition("BEGIN IMMEDIATE;", cancellationToken: cancellationToken));
        _transactionActive = true;
    }

    /// <summary>
    /// Confirma la transacción activa, si existe.
    /// </summary>
    public async Task Commit(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SqliteUnitOfWork));
        }

        if (!_transactionActive)
        {
            return;
        }

        await _connection.ExecuteAsync(new CommandDefinition("COMMIT;", cancellationToken: cancellationToken));
        _transactionActive = false;
    }

    /// <summary>
    /// Revierte la transacción activa, si existe.
    /// </summary>
    public async Task Rollback(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_disposed)
        {
            return;
        }

        if (!_transactionActive)
        {
            return;
        }

        await _connection.ExecuteAsync(new CommandDefinition("ROLLBACK;", cancellationToken: cancellationToken));
        _transactionActive = false;
    }

    /// <summary>
    /// Revierte cualquier transacción activa y libera sincrónicamente la conexión.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_transactionActive)
        {
            try
            {
                _connection.Execute("ROLLBACK;");
            }
            catch
            {
                // Best effort cleanup for disposed unit of work.
            }
            _transactionActive = false;
        }

        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
        }

        _disposed = true;
    }

    /// <summary>
    /// Revierte cualquier transacción activa y libera asincrónicamente la conexión.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_transactionActive)
        {
            try
            {
                await _connection.ExecuteAsync(new CommandDefinition("ROLLBACK;"));
            }
            catch
            {
                // Best effort cleanup for disposed unit of work.
            }
            _transactionActive = false;
        }

        if (_connection.State != ConnectionState.Closed)
        {
            await _connection.CloseAsync();
        }

        _disposed = true;
    }
}
