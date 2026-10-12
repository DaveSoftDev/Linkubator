using System.Security.Cryptography;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Bases = Linkubator.Domain.Entities.Bases;

namespace Linkubator.Domain.Entities;

public sealed class User : Bases.BaseEntity
{
    private const int MaximumNameLength = 100;

    private User()
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Alias { get; private set; } = string.Empty;

    public string Password { get; private set; } = string.Empty;

    public DateTime EmailConfirmedAt { get; private set; }

    public string SecurityStamp { get; private set; } = string.Empty;

    public int FailedLoginAttempts { get; private set; }

    public DateTime? LockoutEnd { get; private set; }

    public DateTime? LastEmailSentAt { get; private set; }

    public DateTime? LastExistingAccountNoticeAt { get; private set; }

    public static User Create(string email, string name, string aliasCandidate, string passwordHash, DateTime emailConfirmedAt)
    {
        string normalizedEmail = DomainPolicies.EmailPolicy.Normalize(email);

        string trimmedName = name?.Trim() ?? string.Empty;
        DomainPolicies.UserTextPolicy.ThrowIfNullOrWhiteSpace(trimmedName);

        if (!DomainPolicies.UserTextPolicy.IsWithinMaximumLength(trimmedName, MaximumNameLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        string alias = DomainPolicies.AliasPolicy.Generate(aliasCandidate);

        DomainPolicies.UserTextPolicy.ThrowIfNullOrWhiteSpace(passwordHash);

        if (emailConfirmedAt == default)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        return new User
        {
            Email = normalizedEmail,
            Name = trimmedName,
            Alias = alias,
            Password = passwordHash,
            EmailConfirmedAt = emailConfirmedAt,
            SecurityStamp = GenerateSecurityStamp(),
            FailedLoginAttempts = 0,
            LockoutEnd = null,
            LastEmailSentAt = null,
            LastExistingAccountNoticeAt = null,
        };
    }

    private static string GenerateSecurityStamp()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
