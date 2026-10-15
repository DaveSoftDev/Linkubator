using System.Security.Cryptography;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Entities;

/// <summary>
/// Representa al propietario de los datos.
/// </summary>
public sealed class User : Bases.BaseEntity
{
    private User()
    {
    }

    /// <summary>
    /// Correo electrónico normalizado y único en el sistema.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre del usuario, o <c>null</c> mientras el registro está sin completar.
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// Alias público, mutable y único en el sistema, o <c>null</c> mientras el registro está sin completar.
    /// </summary>
    public string? Alias { get; private set; }

    /// <summary>
    /// Hash de la contraseña. Nunca se guarda la contraseña en claro.
    /// Es <c>null</c> mientras el registro está sin completar.
    /// </summary>
    public string? Password { get; private set; }

    /// <summary>
    /// Fecha y hora en que se confirmó el correo electrónico, o <c>null</c> si el registro está sin completar.
    /// </summary>
    public DateTime? EmailConfirmedAt { get; private set; }

    /// <summary>
    /// Sello de seguridad utilizado para invalidar sesiones emitidas anteriormente.
    /// </summary>
    public string SecurityStamp { get; private set; } = string.Empty;

    /// <summary>
    /// Número de intentos fallidos consecutivos de verificación de contraseña.
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// Fecha y hora hasta la que la cuenta permanece bloqueada, o <c>null</c> si no está bloqueada.
    /// </summary>
    public DateTime? LockoutEnd { get; private set; }

    /// <summary>
    /// Fecha y hora del último correo con token enviado a la cuenta.
    /// </summary>
    public DateTime? LastEmailSentAt { get; private set; }

    /// <summary>
    /// Fecha y hora del último aviso de cuenta existente enviado a la cuenta.
    /// </summary>
    public DateTime? LastExistingAccountNoticeAt { get; private set; }

    /// <summary>
    /// Crea una nueva instancia de <see cref="User"/> con el registro sin completar:
    /// solo se fijan el correo electrónico y el <see cref="SecurityStamp"/>.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <returns>Nueva instancia de <see cref="User"/> sin el registro completado.</returns>
    public static User CreateIncompleteRegistration(string email)
    {
        string normalizedEmail = DomainPolicies.EmailPolicy.Normalize(email);

        return new User
        {
            Email = normalizedEmail,
            Name = null,
            Alias = null,
            Password = null,
            EmailConfirmedAt = null,
            SecurityStamp = GenerateSecurityStamp(),
            FailedLoginAttempts = 0,
            LockoutEnd = null,
            LastEmailSentAt = null,
            LastExistingAccountNoticeAt = null,
        };
    }

    /// <summary>
    /// Crea una nueva instancia de <see cref="User"/> con el registro completado.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="name">Nombre del usuario.</param>
    /// <param name="aliasCandidate">Candidato para generar el alias.</param>
    /// <param name="passwordHash">Hash de la contraseña.</param>
    /// <param name="emailConfirmedAt">Fecha y hora de confirmación del correo electrónico.</param>
    /// <returns>Nueva instancia de <see cref="User"/>.</returns>
    public static User Create(string email, string name, string aliasCandidate, string passwordHash, DateTime emailConfirmedAt)
    {
        User user = CreateIncompleteRegistration(email);

        user.CompleteRegistration(name, aliasCandidate, passwordHash, emailConfirmedAt);

        return user;
    }

    /// <summary>
    /// Completa el registro fijando a la vez el nombre, el alias, el hash de la contraseña
    /// y la fecha de confirmación del correo electrónico. Solo se puede completar una vez.
    /// </summary>
    /// <param name="name">Nombre del usuario.</param>
    /// <param name="aliasCandidate">Candidato para generar el alias.</param>
    /// <param name="passwordHash">Hash de la contraseña.</param>
    /// <param name="emailConfirmedAt">Fecha y hora de confirmación del correo electrónico.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el registro ya está completado o faltan datos obligatorios.</exception>
    public void CompleteRegistration(string name, string aliasCandidate, string passwordHash, DateTime emailConfirmedAt)
    {
        if (EmailConfirmedAt is not null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        string trimmedName = DomainPolicies.UserTextPolicy.EnsureRequiredText(name, LengthLimitsConstants.UserName);

        string alias = DomainPolicies.AliasPolicy.Generate(aliasCandidate);

        DomainPolicies.UserTextPolicy.ThrowIfNullOrWhiteSpace(passwordHash);

        if (emailConfirmedAt == default)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        Name = trimmedName;
        Alias = alias;
        Password = passwordHash;
        EmailConfirmedAt = emailConfirmedAt;
    }

    /// <summary>
    /// Genera un nuevo sello de seguridad aleatorio para el usuario.
    /// </summary>
    /// <returns>Sello de seguridad codificado en formato URL-safe.</returns>
    private static string GenerateSecurityStamp()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
