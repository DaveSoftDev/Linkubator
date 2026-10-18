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
    /// Se fija al construir el agregado para que el objeto tenga un estado coherente
    /// antes de persistir. La BBDD puede sobrescribirlo al insertar si así lo requiere la infraestructura.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
