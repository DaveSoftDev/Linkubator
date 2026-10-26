using Dapper;
using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Domain.Entities;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

namespace Linkubator.Tests.Infrastructure;

public sealed class SqliteUnitOfWorkTests
{
    [Fact]
    public async Task UnitOfWorkFactory_Get_WhenContextIsLinkubator_ReturnsTransactionalRepositorySet()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();
        await InsertUser(databasePath, 42, "dev@example.com", "dev");

        IUnitOfWorkFactory factory = new SqliteUnitOfWorkFactory(connectionFactory);
        await using IUnitOfWork unitOfWork = factory.Get(UnitOfWorkContexts.Linkubator);

        await unitOfWork.Begin();
        ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
        ILinkRepository linkRepository = unitOfWork.GetRepository<ILinkRepository>();

        Collection collection = Collection.Create(42, "My collection", "desc", false);
        await collectionRepository.Insert(collection);
        Link createdLink = Link.Create(42, collection, "https://example.com/alpha", "Alpha", null, null, false);
        await linkRepository.Insert(createdLink);

        await using SqliteConnection observerBeforeCommit = connectionFactory.CreateOpenConnection();
        int uncommittedCollectionCount = await observerBeforeCommit.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE UserId = 42;");

        Assert.Equal(0, uncommittedCollectionCount);

        await unitOfWork.Commit();

        using SqliteConnection connection = connectionFactory.CreateOpenConnection();

        int collectionCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE UserId = 42;");
        int linkCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link WHERE UserId = 42;");

        Assert.Equal(1, collectionCount);
        Assert.Equal(1, linkCount);

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWork_Rollback_LeavesPreviousStateUnchanged()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();
        await InsertUser(databasePath, 99, "owner@example.com", "owner");

        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);
        await unitOfWork.Begin();

        ICollectionRepository collectionRepository = unitOfWork.GetRepository<ICollectionRepository>();
        Collection collection = Collection.Create(99, "Pending", "desc", false);
        await collectionRepository.Insert(collection);

        await unitOfWork.Rollback();

        using SqliteConnection connection = connectionFactory.CreateOpenConnection();
        int count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE UserId = 99;");

        Assert.Equal(0, count);

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWork_Begin_WhenStarted_AcquiresImmediateWriteLock()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();
        await InsertUser(databasePath, 42, "owner@example.com", "owner");
        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);
        await unitOfWork.Begin();
        await using SqliteConnection competingConnection = connectionFactory.CreateOpenConnection();
        await competingConnection.ExecuteAsync("PRAGMA busy_timeout = 0;");

        SqliteException exception = await Assert.ThrowsAsync<SqliteException>(() => competingConnection.ExecuteAsync(
            "INSERT INTO Collection (UserId, Name, Slug) VALUES (42, 'Blocked', 'blocked');"));

        Assert.Equal(5, exception.SqliteErrorCode);
        await unitOfWork.Rollback();
        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWork_DisposeAsync_WhenTransactionIsActive_RollsBackWrites()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();
        await InsertUser(databasePath, 99, "owner@example.com", "owner");
        IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);
        await unitOfWork.Begin();
        ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
        await repository.Insert(Linkubator.Domain.Entities.Collection.Create(99, "Temporary", null));

        await unitOfWork.DisposeAsync();
        await using SqliteConnection observer = connectionFactory.CreateOpenConnection();
        int count = await observer.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE UserId = 99;");

        Assert.Equal(0, count);
        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWorkFactory_Get_WhenContextIsUnsupported_Throws()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        SqliteUnitOfWorkFactory factory = new(connectionFactory);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Task.Run(() => factory.Get("Unsupported")));

        Assert.Contains("Unsupported", exception.Message);
    }

    [Fact]
    public async Task UnitOfWork_GetRepository_WhenRepositoryIsNotRegistered_Throws()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteUnitOfWorkFactory factory = new(new SqliteConnectionFactory($"Data Source={databasePath}"));
        await using IUnitOfWork unitOfWork = factory.Get(UnitOfWorkContexts.Linkubator);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => unitOfWork.GetRepository<IUnknownRepository>());

        Assert.Contains(nameof(IUnknownRepository), exception.Message);
    }

    [Fact]
    public async Task UnitOfWork_Begin_WhenAlreadyStarted_IsIdempotent()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();
        await InsertUser(databasePath, 133, "idempotent@example.com", "idempotent");

        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);

        await unitOfWork.Begin();
        await unitOfWork.Begin();
        ICollectionRepository repository = unitOfWork.GetRepository<ICollectionRepository>();
        await repository.Insert(Collection.Create(133, "Repeat", "desc", false));
        await unitOfWork.Commit();

        using SqliteConnection connection = connectionFactory.CreateOpenConnection();
        int count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE UserId = 133;");
        Assert.Equal(1, count);

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWork_CommitAndRollback_WhenNoTransactionActive_AreNoOps()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();

        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);

        await unitOfWork.Commit();
        await unitOfWork.Rollback();

        using SqliteConnection connection = connectionFactory.CreateOpenConnection();
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT 1;"));

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWork_GetRepository_WhenDisposed_ThrowsObjectDisposedException()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);

        unitOfWork.Dispose();

        Assert.Throws<ObjectDisposedException>(() => unitOfWork.GetRepository<ICollectionRepository>());
        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWork_DisposeAndDisposeAsync_WhenRepeated_AreSafe()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();

        IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);

        await unitOfWork.Begin();
        unitOfWork.Dispose();
        unitOfWork.Dispose();

        await unitOfWork.DisposeAsync();
        await unitOfWork.DisposeAsync();

        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWork_DisposedState_ThrowsOnBeginAndCommit()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();

        IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);
        unitOfWork.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => unitOfWork.Begin());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => unitOfWork.Commit());
        await unitOfWork.Rollback();

        DeleteDatabaseFiles(databasePath);
    }

    private static async Task InsertUser(string databasePath, long userId, string email, string alias)
    {
        await using SqliteConnection connection = new($"Data Source={databasePath}");
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO User (Id, Email, Alias, Name, SecurityStamp, CreatedAt)
            VALUES (@Id, @Email, @Alias, @Name, @SecurityStamp, @CreatedAt);
        ", new
        {
            Id = userId,
            Email = email,
            Alias = alias,
            Name = alias,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow
        });
    }

    private interface IUnknownRepository
    {
    }

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (string filePath in new[]
        {
            databasePath,
            $"{databasePath}-wal",
            $"{databasePath}-shm"
        })
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException)
                {
                    // SQLite can release temporary database files after the connection is closed.
                }
            }
        }
    }
}
