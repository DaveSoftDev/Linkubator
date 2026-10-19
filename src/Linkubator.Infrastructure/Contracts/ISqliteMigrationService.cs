namespace Linkubator.Infrastructure.Contracts;

public interface ISqliteMigrationService
{
    Task ApplyPendingMigrations();
}
