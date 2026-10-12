namespace Linkubator.Domain.Entities.Bases;

public abstract class BaseEntity
{
    public long Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }
}
