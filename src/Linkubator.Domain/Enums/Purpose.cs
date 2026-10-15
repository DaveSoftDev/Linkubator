namespace Linkubator.Domain.Enums;

/// <summary>
/// Operación de cuenta para la que se emitió un token. Identifica el uso admitido del
/// token, de modo que uno emitido para una operación no sirve para otra
/// (ver domain-model.md → «UserToken» y specifications.md → «Tokens»).
/// </summary>
public enum Purpose
{
    /// <summary>Completar el registro de una cuenta que sigue sin completar.</summary>
    CompleteRegistration,

    /// <summary>Restablecer la contraseña de una cuenta completada.</summary>
    PasswordReset,

    /// <summary>Cambiar el email de una cuenta completada.</summary>
    EmailChange,

    /// <summary>Eliminar una cuenta completada.</summary>
    AccountDeletion
}
