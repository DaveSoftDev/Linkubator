using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Contracts;

public interface ISqliteConnectionFactory
{
    SqliteConnection CreateOpenConnection();
}
