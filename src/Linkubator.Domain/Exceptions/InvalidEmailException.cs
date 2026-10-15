namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción que indica que el correo electrónico proporcionado no es válido.
/// </summary>
public sealed class InvalidEmailException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="InvalidEmailException"/>.
    /// </summary>
    public InvalidEmailException()
        : base("InvalidEmail")
    {
    }
}
