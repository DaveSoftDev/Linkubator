namespace Linkubator.Domain.Exceptions;

/// <summary>
/// Excepción base para errores del dominio.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Código identificativo del error de dominio.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="DomainException"/>.
    /// </summary>
    /// <param name="code">Código identificativo del error.</param>
    protected DomainException(string code)
    {
        Code = code;
    }
}