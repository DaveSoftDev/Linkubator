namespace Linkubator.Domain.Exceptions;

public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException()
        : base("Email inválido: la sintaxis no coincide con el subconjunto admitido de addr-spec.")
    {
    }
}
