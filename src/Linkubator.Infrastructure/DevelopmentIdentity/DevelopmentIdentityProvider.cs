using Dapper;
using Microsoft.Data.Sqlite;
using ApplicationContracts = Linkubator.Application.Contracts;
using InfrastructureContracts = Linkubator.Infrastructure.Contracts;
using InfrastructureOptions = Linkubator.Infrastructure.Options;

namespace Linkubator.Infrastructure.DevelopmentIdentity;

public sealed class DevelopmentIdentityProvider : ApplicationContracts.IDevelopmentIdentityProvider
{
    private const string GetUserIdSql = """
        SELECT Id
        FROM User
        WHERE Email = @Email
          AND Name IS NOT NULL
          AND Alias IS NOT NULL
          AND Password IS NOT NULL
          AND EmailConfirmedAt IS NOT NULL;
        """;

    private readonly InfrastructureContracts.ISqliteConnectionFactory _connectionFactory;
    private readonly InfrastructureOptions.DevelopmentIdentityOptions _options;

    public DevelopmentIdentityProvider(
        InfrastructureContracts.ISqliteConnectionFactory connectionFactory,
        InfrastructureOptions.DevelopmentIdentityOptions options)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(options);

        _connectionFactory = connectionFactory;
        _options = options;
    }

    public async Task<long> GetUserId(CancellationToken cancellationToken = default)
    {
        using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
        CommandDefinition command = new(
            GetUserIdSql,
            new { _options.Email },
            cancellationToken: cancellationToken);
        long? userId = await connection.QuerySingleOrDefaultAsync<long?>(command);

        return userId ?? throw new InvalidOperationException("The configured development identity is missing or incomplete.");
    }
}