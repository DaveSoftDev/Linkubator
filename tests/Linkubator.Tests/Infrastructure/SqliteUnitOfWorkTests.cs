using Dapper;
using Linkubator.Application.Constants;
using Linkubator.Application.Contracts;
using Linkubator.Domain.Entities;
using Linkubator.Infrastructure.Sqlite;
using Linkubator.Application.Services;
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
        Assert.Equal(0, await observerBeforeCommit.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE UserId = 42;"));
        Assert.Equal(0, await observerBeforeCommit.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link WHERE UserId = 42;"));
        Assert.Equal(0, await observerBeforeCommit.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collection.Id }));
        Assert.Equal(0, await observerBeforeCommit.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM LinkSearch WHERE rowid = @Id;", new { Id = createdLink.Id }));

        await unitOfWork.Commit();

        using SqliteConnection connection = connectionFactory.CreateOpenConnection();

        int collectionCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE UserId = 42;");
        int linkCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Link WHERE UserId = 42;");

        Assert.Equal(1, collectionCount);
        Assert.Equal(1, linkCount);
        Assert.Equal(("My collection", "desc"), await connection.QuerySingleAsync<(string, string)>("SELECT Name, Description FROM Collection WHERE Id = @Id;", new { Id = collection.Id }));
        Assert.Equal(("https://example.com/alpha", "Alpha"), await connection.QuerySingleAsync<(string, string)>("SELECT UrlOriginal, Title FROM Link WHERE Id = @Id;", new { Id = createdLink.Id }));
        Assert.Equal(("My collection", "desc"), await connection.QuerySingleAsync<(string, string)>("SELECT Name, Description FROM CollectionSearch WHERE rowid = @Id;", new { Id = collection.Id }));
        Assert.Equal(("https://example.com/alpha", "Alpha", "My collection"), await connection.QuerySingleAsync<(string, string, string)>("SELECT UrlOriginal, Title, CollectionName FROM LinkSearch WHERE rowid = @Id;", new { Id = createdLink.Id }));

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
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collection.Id }));

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
        Collection collection = Collection.Create(99, "Temporary", null);
        await repository.Insert(collection);

        await unitOfWork.DisposeAsync();
        await using SqliteConnection observer = connectionFactory.CreateOpenConnection();
        int count = await observer.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE UserId = 99;");

        Assert.Equal(0, count);
        Assert.Equal(0, await observer.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collection.Id }));
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
        await InsertUser(databasePath, 99, "owner@example.com", "owner");

        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);

        await unitOfWork.Commit();
        await unitOfWork.Rollback();
        await unitOfWork.Begin();
        Collection collection = Collection.Create(99, "After no-op", null);
        await unitOfWork.GetRepository<ICollectionRepository>().Insert(collection);
        await unitOfWork.Commit();

        using SqliteConnection connection = connectionFactory.CreateOpenConnection();
        Assert.Equal("After no-op", await connection.QuerySingleAsync<string>("SELECT Name FROM Collection WHERE Id = @Id;", new { Id = collection.Id }));

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
        await InsertUser(databasePath, 99, "owner@example.com", "owner");

        IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(connectionFactory).Get(UnitOfWorkContexts.Linkubator);

        await unitOfWork.Begin();
        Collection collection = Collection.Create(99, "Pending", null);
        await unitOfWork.GetRepository<ICollectionRepository>().Insert(collection);
        unitOfWork.Dispose();
        unitOfWork.Dispose();

        await unitOfWork.DisposeAsync();
        await unitOfWork.DisposeAsync();
        await using SqliteConnection observer = connectionFactory.CreateOpenConnection();
        await observer.ExecuteAsync("PRAGMA busy_timeout = 0;");
        Assert.Equal(0, await observer.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE Id = @Id;", new { Id = collection.Id }));
        Assert.Equal(0, await observer.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collection.Id }));
        Assert.Equal(1, await observer.ExecuteAsync("INSERT INTO Collection (UserId, Name, Slug) VALUES (99, 'After dispose', 'after-dispose');"));

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UnitOfWork_WhenTransactionFinishes_ReleasesWriteLock(bool commit)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(factory).ApplyPendingMigrations();
        await InsertUser(databasePath, 99, "owner@example.com", "owner");
        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(factory).Get(UnitOfWorkContexts.Linkubator);
        await unitOfWork.Begin();
        await using SqliteConnection competing = factory.CreateOpenConnection();
        await competing.ExecuteAsync("PRAGMA busy_timeout = 0;");

        SqliteException blocked = await Assert.ThrowsAsync<SqliteException>(() => competing.ExecuteAsync(new CommandDefinition("INSERT INTO Collection (UserId, Name, Slug) VALUES (99, 'Other', 'other');", commandTimeout: 1)));
        Assert.Equal(5, blocked.SqliteErrorCode);
        if (commit)
        {
            await unitOfWork.Commit();
        }
        else
        {
            await unitOfWork.Rollback();
        }
        Assert.Equal(1, await competing.ExecuteAsync("INSERT INTO Collection (UserId, Name, Slug) VALUES (99, 'Other', 'other');"));
        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task UnitOfWork_Begin_WhenCancellationAlreadyRequested_DoesNotStartTransaction()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(factory).ApplyPendingMigrations();
        await InsertUser(databasePath, 99, "owner@example.com", "owner");
        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(factory).Get(UnitOfWorkContexts.Linkubator);
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => unitOfWork.Begin(cancellation.Token));
        await using SqliteConnection observer = factory.CreateOpenConnection();
        await observer.ExecuteAsync("PRAGMA busy_timeout = 0;");
        Assert.Equal(1, await observer.ExecuteAsync("INSERT INTO Collection (UserId, Name, Slug) VALUES (99, 'Other', 'other');"));
        DeleteDatabaseFiles(databasePath);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UnitOfWork_WhenFinishIsCancelled_LeavesPendingWritesUntilRollback(bool commit)
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory factory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(factory).ApplyPendingMigrations();
        await InsertUser(databasePath, 99, "owner@example.com", "owner");
        await using IUnitOfWork unitOfWork = new SqliteUnitOfWorkFactory(factory).Get(UnitOfWorkContexts.Linkubator);
        await unitOfWork.Begin();
        Collection collection = Collection.Create(99, "Pending", null);
        await unitOfWork.GetRepository<ICollectionRepository>().Insert(collection);
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => commit ? unitOfWork.Commit(cancellation.Token) : unitOfWork.Rollback(cancellation.Token));
        await using SqliteConnection observer = factory.CreateOpenConnection();
        await observer.ExecuteAsync("PRAGMA busy_timeout = 0;");
        Assert.Equal(0, await observer.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE Id = @Id;", new { Id = collection.Id }));
        SqliteException blocked = await Assert.ThrowsAsync<SqliteException>(() => observer.ExecuteAsync(new CommandDefinition("INSERT INTO Collection (UserId, Name, Slug) VALUES (99, 'Other', 'other');", commandTimeout: 1)));
        Assert.Equal(5, blocked.SqliteErrorCode);
        await unitOfWork.Rollback();
        Assert.Equal(0, await observer.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collection.Id }));
        DeleteDatabaseFiles(databasePath);
    }

    [Fact]
    public async Task ExecuteInTransaction_WhenActionIsCancelled_RollsBackEntityAndProjection()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        SqliteConnectionFactory connectionFactory = new($"Data Source={databasePath}");
        await new SqliteMigrationService(connectionFactory).ApplyPendingMigrations();
        await InsertUser(databasePath, 99, "owner@example.com", "owner");
        IUnitOfWorkFactory factory = new SqliteUnitOfWorkFactory(connectionFactory);
        using CancellationTokenSource cancellation = new();
        Collection collection = Collection.Create(99, "Cancelled", null);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => factory.ExecuteInTransaction(UnitOfWorkContexts.Linkubator,
            async (work, token) =>
            {
                await work.GetRepository<ICollectionRepository>().Insert(collection, token);
                await cancellation.CancelAsync();
                token.ThrowIfCancellationRequested();
            }, cancellation.Token));

        await using SqliteConnection observer = connectionFactory.CreateOpenConnection();
        Assert.Equal(0, await observer.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Collection WHERE Id = @Id;", new { Id = collection.Id }));
        Assert.Equal(0, await observer.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CollectionSearch WHERE rowid = @Id;", new { Id = collection.Id }));
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
