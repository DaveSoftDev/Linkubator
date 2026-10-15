namespace Linkubator.Domain.Entities.Bases;

/// <summary>
/// Clase Base para todas las entidades del dominio.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único de la entidad.
    /// </summary>
    public long Id { get; protected set; }

    /// <summary>
    /// Fecha y hora en que la entidad fue creada.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }
}
