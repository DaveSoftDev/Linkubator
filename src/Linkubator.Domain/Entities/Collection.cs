using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Bases = Linkubator.Domain.Entities.Bases;

namespace Linkubator.Domain.Entities;

public sealed class Collection : Bases.BaseEntity
{
    private const int MaximumNameLength = 50;
    private const int MaximumDescriptionLength = 250;

    private Collection()
    {
    }

    public long UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsPublic { get; private set; }

    public static Collection Create(long userId, string name, string? description)
    {
        if (userId <= 0)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        string trimmedName = name?.Trim() ?? string.Empty;
        DomainPolicies.UserTextPolicy.ThrowIfNullOrWhiteSpace(trimmedName);

        if (!DomainPolicies.UserTextPolicy.IsWithinMaximumLength(trimmedName, MaximumNameLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        string? trimmedDescription = DomainPolicies.UserTextPolicy.TrimToNull(description);
        if (trimmedDescription is not null
            && !DomainPolicies.UserTextPolicy.IsWithinMaximumLength(trimmedDescription, MaximumDescriptionLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        string slug = DomainPolicies.SlugPolicy.Generate(trimmedName);

        return new Collection
        {
            UserId = userId,
            Name = trimmedName,
            Slug = slug,
            Description = trimmedDescription,
            IsPublic = false,
        };
    }

    public void Rename(string name)
    {
        string trimmedName = name?.Trim() ?? string.Empty;
        DomainPolicies.UserTextPolicy.ThrowIfNullOrWhiteSpace(trimmedName);

        if (!DomainPolicies.UserTextPolicy.IsWithinMaximumLength(trimmedName, MaximumNameLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        Name = trimmedName;
        Slug = DomainPolicies.SlugPolicy.Generate(trimmedName);
    }

    public void ChangeDescription(string? description)
    {
        string? trimmedDescription = DomainPolicies.UserTextPolicy.TrimToNull(description);
        if (trimmedDescription is not null
            && !DomainPolicies.UserTextPolicy.IsWithinMaximumLength(trimmedDescription, MaximumDescriptionLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        Description = trimmedDescription;
    }
}
