namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Se lanza cuando la entrada es nula, vacía o solo contiene espacios.
/// Precondición de los métodos públicos de Domain que reciben texto.
/// </summary>
public sealed class NotValidInputException : DomainException
{
    public NotValidInputException()
        : base("Entrada no válida: nula, vacía o en blanco.")
    {
    }
}
