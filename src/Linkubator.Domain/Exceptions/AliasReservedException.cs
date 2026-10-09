namespace Linkubator.Domain.Exceptions;

public sealed class AliasReservedException : DomainException
{
    public AliasReservedException()
        : base("AliasReserved")
    {
    }
}
