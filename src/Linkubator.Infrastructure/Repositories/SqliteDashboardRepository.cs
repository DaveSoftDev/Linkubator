using Dapper;
using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Models;
using Microsoft.Data.Sqlite;

namespace Linkubator.Infrastructure.Repositories;

public sealed class SqliteDashboardRepository : IDashboardRepository
{
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Crea el repositorio del panel asociado a la conexión SQLite indicada.
    /// </summary>
    public SqliteDashboardRepository(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// Obtiene los datos recientes del panel del usuario.
    /// </summary>
    public async Task<DashboardDto> GetByUser(long userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LinkDto> recentLinks = (await _connection.QueryAsync<LinkDto>(new CommandDefinition(
            @"SELECT l.Id, l.CollectionId, l.UrlOriginal, l.UrlNormalized, l.Title, l.Description, l.Image, l.IsPublic, l.CreatedAt,
                     c.Name AS CollectionName
              FROM Link l
              LEFT JOIN Collection c ON c.Id = l.CollectionId
              WHERE l.UserId = @UserId
              ORDER BY l.CreatedAt DESC, l.Id DESC
              LIMIT 12;",
            new { UserId = userId },
            cancellationToken: cancellationToken)))
            .ToList();

        return new DashboardDto(recentLinks);
    }

}
