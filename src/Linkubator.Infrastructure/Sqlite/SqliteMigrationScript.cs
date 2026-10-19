namespace Linkubator.Infrastructure.Sqlite;

public sealed record SqliteMigrationScript(string Version, string Sql);
