using Dapper;
using Microsoft.Data.Sqlite;
using ApplicationContracts = Linkubator.Application.Contracts;
using ApplicationModels = Linkubator.Application.Models;
using InfrastructureConstants = Linkubator.Infrastructure.Constants;
using InfrastructureContracts = Linkubator.Infrastructure.Contracts;

namespace Linkubator.Infrastructure.DevelopmentIdentity;

public sealed class DevelopmentIdentityProvider : ApplicationContracts.IDevelopmentIdentityProvider
{
    private const string GetProfileSql = """
        SELECT User.Id AS UserId, User.Name, User.Alias, User.Email
        FROM User
        WHERE User.Email = @Email
          AND User.Name IS NOT NULL
          AND User.Alias IS NOT NULL
          AND User.Password IS NOT NULL
          AND User.EmailConfirmedAt IS NOT NULL;
        """;

    private readonly InfrastructureContracts.ISqliteConnectionFactory _connectionFactory;

    /// <summary>
    /// Crea un proveedor de la identidad de desarrollo usando la fábrica de conexiones indicada.
    /// </summary>
    public DevelopmentIdentityProvider(InfrastructureContracts.ISqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);

        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene el perfil de la identidad de desarrollo configurada.
    /// </summary>
    public async Task<ApplicationModels.DevelopmentIdentityProfile> GetProfile(CancellationToken cancellationToken = default)
    {
        using SqliteConnection connection = _connectionFactory.CreateOpenConnection();
        CommandDefinition command = new(
            GetProfileSql,
            new { Email = InfrastructureConstants.DevelopmentIdentityDefaults.Email },
            cancellationToken: cancellationToken);
        ApplicationModels.DevelopmentIdentityProfile? profile =
            await connection.QuerySingleOrDefaultAsync<ApplicationModels.DevelopmentIdentityProfile>(command);

        return profile is { IsValid: true }
            ? profile
            : throw new InvalidOperationException("The development identity is missing or incomplete.");
    }
}