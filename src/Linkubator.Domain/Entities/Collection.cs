using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Entities;

/// <summary>
/// Representa una agrupación de enlaces (colección).
/// </summary>
public sealed class Collection : Bases.BaseEntity
{
    private readonly List<Link> _links = new();

    private Collection()
    {
    }

    /// <summary>
    /// Identificador del usuario propietario de la colección.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Nombre de la colección.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Slug único dentro del usuario generado a partir del nombre.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Descripción opcional de la colección.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indica si la colección es pública.
    /// </summary>
    public bool IsPublic { get; private set; }

    /// <summary>
    /// Colección de enlaces asociados a esta colección.
    /// </summary>
    public IReadOnlyCollection<Link> Links => _links.AsReadOnly();

    /// <summary>
    /// Crea una nueva instancia de <see cref="Collection"/>.
    /// </summary>
    /// <param name="userId">Identificador del usuario propietario.</param>
    /// <param name="name">Nombre de la colección.</param>
    /// <param name="description">Descripción opcional.</param>
    /// <param name="isPublic">Indica si la colección es pública inicialmente.</param>
    /// <returns>Nueva instancia de <see cref="Collection"/>.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si <paramref name="userId"/> no es válido.</exception>
    public static Collection Create(long userId, string name, string? description, bool isPublic = false)
    {
        if (userId <= 0)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        string trimmedName = DomainPolicies.UserTextPolicy.EnsureRequiredText(name, LengthLimitsConstants.CollectionName);

        string? trimmedDescription = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(description, LengthLimitsConstants.CollectionDescription);

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

    /// <summary>
    /// Establece el identificador generado por la persistencia.
    /// </summary>
    internal void SetId(long id)
    {
        if (id <= 0 || Id != 0)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        Id = id;
    }

    /// <summary>
    /// Rehidrata la colección con la lista de enlaces que pertenecen a ella.
    /// Materializa y valida toda la entrada antes de modificar las asociaciones existentes.
    /// </summary>
    /// <param name="links">Enlaces asociados a la colección.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si la colección o los enlaces no son válidos.</exception>
    public void RehydrateLinks(IEnumerable<Link> links)
    {
        if (links is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        List<Link> associatedLinks = links.ToList();
        foreach (Link link in associatedLinks)
        {
            if (link is null || link.UserId != UserId || link.CollectionId != Id)
            {
                throw new DomainExceptions.NotValidInputException();
            }
        }

        _links.Clear();
        foreach (Link link in associatedLinks)
        {
            link.RehydrateCollection(this);
        }
    }

    /// <summary>
    /// Cambia la privacidad de la colección y de sus enlaces cargados.
    /// Los enlaces no cargados deben actualizarse mediante la operación de persistencia correspondiente.
    /// </summary>
    public void ChangePrivacy(bool isPublic)
        => ChangePrivacy(isPublic, _links);

    /// <summary>
    /// Cambia la privacidad de la colección y, si pasa a privada, de los enlaces asociados cargados.
    /// </summary>
    /// <param name="isPublic">Nuevo valor de privacidad.</param>
    /// <param name="links">Colección de enlaces asociados que se validan contra el estado actual.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si los enlaces proporcionados no coinciden con los enlaces asociados o son inválidos.</exception>
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

    /// <summary>
    /// Añade un enlace a la colección.
    /// </summary>
    /// <param name="link">Enlace a añadir.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el enlace es nulo o no pertenece a esta colección.</exception>
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

    /// <summary>
    /// Elimina un enlace de la colección.
    /// </summary>
    /// <param name="link">Enlace a eliminar.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si el enlace es nulo.</exception>
    internal void RemoveLink(Link link)
    {
        if (link is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        _links.Remove(link);
    }

    /// <summary>
    /// Renombra la colección y regenera su slug.
    /// </summary>
    /// <param name="name">Nuevo nombre de la colección.</param>
    public void Rename(string name)
    {
        string trimmedName = DomainPolicies.UserTextPolicy.EnsureRequiredText(name, LengthLimitsConstants.CollectionName);

        string slug = DomainPolicies.SlugPolicy.Generate(trimmedName);

        Name = trimmedName;
        Slug = slug;
    }

    /// <summary>
    /// Cambia la descripción de la colección.
    /// </summary>
    /// <param name="description">Nueva descripción opcional.</param>
    public void ChangeDescription(string? description)
    {
        string? trimmedDescription = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(description, LengthLimitsConstants.CollectionDescription);

        Description = trimmedDescription;
    }
}
