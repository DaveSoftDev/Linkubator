namespace Linkubator.Application.Dtos.Models;

public sealed record CollectionDto(long Id, string Name, string Slug, string? Description, bool IsPublic, int LinkCount, DateTime CreatedAt)
{
    /// <summary>
    /// Inicializa CollectionDto con las dependencias necesarias.
    /// </summary>
    private CollectionDto()
        : this(default, string.Empty, string.Empty, null, default, default, default)
    {
    }
}
