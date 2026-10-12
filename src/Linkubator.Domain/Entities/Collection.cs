using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Bases = Linkubator.Domain.Entities.Bases;

namespace Linkubator.Domain.Entities;

public sealed class Collection : Bases.BaseEntity
{
    private const int MaximumNameLength = 50;
    private const int MaximumDescriptionLength = 250;

    private readonly List<Link> _links = new();

    private Collection()
    {
    }

    public long UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsPublic { get; private set; }

    public static Collection Create(long userId, string name, string? description, bool isPublic = false)
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
            IsPublic = isPublic,
        };
    }

    public void ChangePrivacy(bool isPublic, IEnumerable<Link> links)
    {
        if (links is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        List<Link> associatedLinks = links.ToList();
        HashSet<Link> suppliedLinks = new(associatedLinks);
        if (suppliedLinks.Count != associatedLinks.Count || !suppliedLinks.SetEquals(_links))
        {
            throw new DomainExceptions.NotValidInputException();
        }

        foreach (Link link in associatedLinks)
        {
            if (link is null || link.UserId != UserId || link.CollectionId != Id)
            {
                throw new DomainExceptions.NotValidInputException();
            }
        }

        if (!isPublic)
        {
            foreach (Link link in associatedLinks)
            {
                link.ChangePrivacy(false, this);
            }
        }

        IsPublic = isPublic;
    }

    internal void AddLink(Link link)
    {
        if (link is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (link.UserId != UserId || link.CollectionId != Id)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (!_links.Contains(link))
        {
            _links.Add(link);
        }
    }

    internal void RemoveLink(Link link)
    {
        if (link is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        _links.Remove(link);
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
