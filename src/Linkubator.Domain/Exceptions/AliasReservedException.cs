namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el alias propuesto está reservado y no está disponible.
/// </summary>
public sealed class AliasReservedException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AliasReservedException"/>.
    /// </summary>
    public AliasReservedException()
        : base("AliasReserved")
    {
    }
}
