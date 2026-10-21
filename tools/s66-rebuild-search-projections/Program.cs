using Linkubator.Infrastructure.Contracts;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

if (args.Length != 1)
{
    throw new ArgumentException("Usage: S66RebuildSearchProjections <database-path>");
}

string databasePath = Path.GetFullPath(args[0]);
if (!File.Exists(databasePath))
{
    throw new FileNotFoundException("Database does not exist; refusing to create an empty database.", databasePath);
}

string connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
SqliteConnectionFactory connectionFactory = new(connectionString);
ISqliteSearchProjectionRebuilder projectionRebuilder = new SqliteSearchProjectionRebuilder(connectionFactory);
await projectionRebuilder.Rebuild();

Console.WriteLine("Search projections for links and collections were rebuilt successfully.");
