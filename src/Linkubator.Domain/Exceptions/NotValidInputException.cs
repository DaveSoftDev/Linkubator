namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Rechazo genérico del dominio ante una entrada o un estado no válidos: texto nulo, vacío
/// o con solo espacios, identificadores no positivos, recursos que no son del usuario o no
/// son los esperados, y operaciones que el estado actual no permite. El motivo concreto lo
/// conoce la operación que lanza; los mensajes que ve el usuario se definen en la capa de
/// aplicación (ver specifications.md → «Errores controlados»).
/// </summary>
public sealed class NotValidInputException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="NotValidInputException"/>.
    /// </summary>
    public NotValidInputException()
        : base("NotValidInput")
    {
    }
}
