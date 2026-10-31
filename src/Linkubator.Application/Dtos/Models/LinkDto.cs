namespace Linkubator.Application.Dtos.Models;

public sealed record LinkDto(long Id, long CollectionId, string UrlOriginal, string UrlNormalized, string? Title, string? Description, string? Image, bool IsPublic, DateTime CreatedAt)
{
    public string? CollectionName { get; init; }

    /// <summary>
    /// Inicializa LinkDto con las dependencias necesarias.
    /// </summary>
    private LinkDto()
        : this(default, default, string.Empty, string.Empty, null, null, null, default, default)
    {
    }
}
