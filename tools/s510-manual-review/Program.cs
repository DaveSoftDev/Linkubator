using System.Reflection;
using System.Text;
using Linkubator.Infrastructure.DevelopmentIdentity;
using Linkubator.Infrastructure.Sqlite;
using Microsoft.Data.Sqlite;

if (args.Length != 2)
{
    throw new ArgumentException("Usage: S510ManualReview <initialize|prepare|repeat> <database-path>");
}

string mode = args[0];
string databasePath = Path.GetFullPath(args[1]);
string connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
SqliteConnectionFactory factory = new(connectionString);

switch (mode)
{
    case "initialize":
        if (File.Exists(databasePath))
        {
            throw new IOException($"Refusing to overwrite existing database: {databasePath}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        await new SqliteMigrationService(factory).ApplyPendingMigrations();
        await Provision(factory);
        break;
    case "prepare":
        if (!File.Exists(databasePath))
        {
            throw new FileNotFoundException("Review database does not exist.", databasePath);
        }

        await PrepareMarkers(factory);
        break;
    case "repeat":
        if (!File.Exists(databasePath))
        {
            throw new FileNotFoundException("Review database does not exist.", databasePath);
        }

        await Provision(factory);
        break;
    default:
        throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown mode.");
}

await PrintState(factory, databasePath);

static async Task Provision(SqliteConnectionFactory factory)
{
    const string email = "s510-review@linkubator.test";
    const string name = "S510 Review User";
    const string alias = "s510-review-user";
    const string collectionName = "Bandeja de entrada";
    const string collectionSlug = "bandeja-de-entrada";
    string salt = Convert.ToBase64String(Enumerable.Range(1, 16).Select(value => (byte)value).ToArray()).TrimEnd('=');
    string digest = Convert.ToBase64String(Enumerable.Range(1, 32).Select(value => (byte)value).ToArray()).TrimEnd('=');
    string passwordHash = $"$argon2id$v=19$m=19456,t=2,p=1${salt}${digest}";
    string resourceName = "Linkubator.Infrastructure.DevelopmentIdentity.Sql.ProvisionDevelopmentIdentity.sql";
    Assembly assembly = typeof(DevelopmentIdentityProvisioner).Assembly;
    using Stream resource = assembly.GetManifestResourceStream(resourceName)
        ?? throw new InvalidOperationException($"Missing SQL resource: {resourceName}");
    using StreamReader reader = new(resource);
    string sql = await reader.ReadToEndAsync();

    await using SqliteConnection connection = factory.CreateOpenConnection();
    await Execute(connection, "BEGIN IMMEDIATE;");
    try
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("Email", email);
        command.Parameters.AddWithValue("Name", name);
        command.Parameters.AddWithValue("Alias", alias);
        command.Parameters.AddWithValue("PasswordHash", passwordHash);
        command.Parameters.AddWithValue("EmailConfirmedAt", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        command.Parameters.AddWithValue("SecurityStamp", Guid.NewGuid().ToString("N"));
        command.Parameters.AddWithValue("CollectionName", collectionName);
        command.Parameters.AddWithValue("CollectionSlug", collectionSlug);
        await command.ExecuteNonQueryAsync();
        await Execute(connection, "COMMIT;");
    }
    catch
    {
        await Execute(connection, "ROLLBACK;");
        throw;
    }
}

static async Task PrepareMarkers(SqliteConnectionFactory factory)
{
    await using SqliteConnection connection = factory.CreateOpenConnection();
    await using (SqliteCommand userCommand = connection.CreateCommand())
    {
        userCommand.CommandText = "UPDATE User SET Name = @Name, Alias = @Alias WHERE Email = @Email;";
        userCommand.Parameters.AddWithValue("Name", "S510 Marker Name");
        userCommand.Parameters.AddWithValue("Alias", "s510-marker-alias");
        userCommand.Parameters.AddWithValue("Email", "s510-review@linkubator.test");
        if (await userCommand.ExecuteNonQueryAsync() != 1)
        {
            throw new InvalidOperationException("Expected exactly one development user for marker setup.");
        }
    }

    await using (SqliteCommand collectionCommand = connection.CreateCommand())
    {
        collectionCommand.CommandText = "UPDATE Collection SET Name = @Name, Slug = @Slug, Description = @Description, IsPublic = 1 WHERE UserId = (SELECT Id FROM User WHERE Email = @Email);";
        collectionCommand.Parameters.AddWithValue("Name", "S510 Marker Collection");
        collectionCommand.Parameters.AddWithValue("Slug", "s510-marker-collection");
        collectionCommand.Parameters.AddWithValue("Description", "S510 marker description");
        collectionCommand.Parameters.AddWithValue("Email", "s510-review@linkubator.test");
        if (await collectionCommand.ExecuteNonQueryAsync() != 1)
        {
            throw new InvalidOperationException("Expected exactly one development collection for marker setup.");
        }
    }
}

static async Task PrintState(SqliteConnectionFactory factory, string databasePath)
{
    await using SqliteConnection connection = factory.CreateOpenConnection();
    await using (SqliteCommand versionCommand = connection.CreateCommand())
    {
        versionCommand.CommandText = "SELECT sqlite_version();";
        Console.WriteLine($"SQLite version: {await versionCommand.ExecuteScalarAsync()}");
    }

    await using (SqliteCommand userCommand = connection.CreateCommand())
    {
        userCommand.CommandText = "SELECT COUNT(*), MAX(Name), MAX(Alias) FROM User WHERE Email = @Email;";
        userCommand.Parameters.AddWithValue("Email", "s510-review@linkubator.test");
        await using SqliteDataReader userReader = await userCommand.ExecuteReaderAsync();
        await userReader.ReadAsync();
        Console.WriteLine($"User count: {userReader.GetInt64(0)}; name: {userReader.GetString(1)}; alias: {userReader.GetString(2)}");
    }

    await using (SqliteCommand collectionCommand = connection.CreateCommand())
    {
        collectionCommand.CommandText = "SELECT COUNT(*), MAX(Name), MAX(Slug), MAX(Description), MAX(IsPublic) FROM Collection WHERE UserId = (SELECT Id FROM User WHERE Email = @Email);";
        collectionCommand.Parameters.AddWithValue("Email", "s510-review@linkubator.test");
        await using SqliteDataReader collectionReader = await collectionCommand.ExecuteReaderAsync();
        await collectionReader.ReadAsync();
        string description = collectionReader.IsDBNull(3) ? "<null>" : collectionReader.GetString(3);
        Console.WriteLine($"Collection count: {collectionReader.GetInt64(0)}; name: {collectionReader.GetString(1)}; slug: {collectionReader.GetString(2)}; description: {description}; public: {collectionReader.GetInt64(4)}");
    }

    Console.WriteLine($"Database: {databasePath}");
}

static async Task Execute(SqliteConnection connection, string sql)
{
    await using SqliteCommand command = connection.CreateCommand();
    command.CommandText = sql;
    await command.ExecuteNonQueryAsync();
}
