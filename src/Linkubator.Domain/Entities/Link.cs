using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using DomainEnums = Linkubator.Domain.Enums;
using Linkubator.Domain.Constants;

namespace Linkubator.Domain.Entities;

/// <summary>
/// Representa un enlace guardado por el usuario.
/// </summary>
public sealed class Link : Bases.BaseEntity
{
    private Collection? _collection;

    private Link()
    {
    }

    /// <summary>
    /// Identificador del usuario propietario del enlace.
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Identificador de la colección a la que pertenece el enlace.
    /// </summary>
    public long CollectionId { get; private set; }

    /// <summary>
    /// URL original ajustada y validada.
    /// </summary>
    public string UrlOriginal { get; private set; } = string.Empty;

    /// <summary>
    /// Versión normalizada de la URL utilizada para detectar duplicados dentro del usuario.
    /// </summary>
    public string UrlNormalized { get; private set; } = string.Empty;

    /// <summary>
    /// Título opcional del enlace.
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Descripción opcional del enlace.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// URL de imagen opcional asociada al enlace.
    /// </summary>
    public string? Image { get; private set; }

    /// <summary>
    /// Indica si el enlace es público.
    /// </summary>
    public bool IsPublic { get; private set; }

    /// <summary>
    /// Estado del scraping del enlace.
    /// </summary>
    public DomainEnums.ScrapingStatus ScrapingStatus { get; private set; }

    /// <summary>
    /// Crea una nueva instancia de <see cref="Link"/>.
    /// </summary>
    /// <param name="userId">Identificador del usuario propietario.</param>
    /// <param name="collection">Colección a la que pertenece el enlace.</param>
    /// <param name="urlOriginal">URL original a guardar.</param>
    /// <param name="title">Título opcional.</param>
    /// <param name="description">Descripción opcional.</param>
    /// <param name="image">URL de imagen opcional.</param>
    /// <param name="isPublic">Indica si el enlace es público inicialmente.</param>
    /// <returns>Nueva instancia de <see cref="Link"/>.</returns>
    /// <exception cref="DomainExceptions.NotValidInputException">Si la colección es nula, los datos no son válidos o no se puede marcar como público.</exception>
    public static Link Create(long userId, Collection collection, string urlOriginal, string? title, string? description, string? image, bool isPublic = false)
    {
        if (collection is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (userId <= 0 || collection.Id <= 0 || collection.UserId != userId)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (isPublic && !collection.IsPublic)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        Link link = CreateInternal(userId, collection.Id, urlOriginal, title, description, image, isPublic);
        collection.AddLink(link);
        link._collection = collection;
        return link;
    }

    /// <summary>
    /// Cambia la privacidad del enlace.
    /// </summary>
    /// <param name="isPublic">Nuevo valor de privacidad.</param>
    /// <param name="collection">Colección a la que pertenece el enlace.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si la colección es inválida o no se puede marcar como público.</exception>
    public void ChangePrivacy(bool isPublic, Collection collection)
    {
        if (collection is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (collection.Id <= 0 || collection.UserId != UserId || collection.Id != CollectionId)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (isPublic && !collection.IsPublic)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        IsPublic = isPublic;
    }

    /// <summary>
    /// Mueve el enlace a otra colección. Al moverlo, pasa a ser privado.
    /// </summary>
    /// <param name="destinationCollection">Colección de destino.</param>
    /// <exception cref="DomainExceptions.NotValidInputException">Si la colección de destino es inválida.</exception>
    public void MoveToCollection(Collection destinationCollection)
    {
        if (destinationCollection is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (destinationCollection.Id <= 0 || destinationCollection.UserId != UserId)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (destinationCollection.Id == CollectionId)
        {
            // Se intenta mover el enlace a la misma colección. No se realiza ninguna acción.
            return;
        }

        if (_collection is not null && _collection.Id == CollectionId)
        {
            _collection.RemoveLink(this);
        }

        CollectionId = destinationCollection.Id;

        if (!ReferenceEquals(_collection, destinationCollection))
        {
            destinationCollection.AddLink(this);
        }

        _collection = destinationCollection;
        IsPublic = false;
    }

    /// <summary>
    /// Edita las propiedades editables del enlace.
    /// </summary>
    /// <param name="title">Nuevo título opcional.</param>
    /// <param name="description">Nueva descripción opcional.</param>
    /// <param name="image">Nueva URL de imagen opcional.</param>
    public void Edit(string? title, string? description, string? image)
    {
        string? nextTitle = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(title, LengthLimitsConstants.LinkTitle);
        string? nextDescription = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(description, LengthLimitsConstants.LinkDescription);
        string? nextImage = NormalizeOptionalImage(image, UrlOriginal, LengthLimitsConstants.LinkImage);

        Title = nextTitle;
        Description = nextDescription;
        Image = nextImage;
    }

    /// <summary>
    /// Crea una nueva instancia de <see cref="Link"/> con los valores ya procesados.
    /// </summary>
    /// <param name="userId">Identificador del usuario propietario.</param>
    /// <param name="collectionId">Identificador de la colección.</param>
    /// <param name="urlOriginal">URL original ajustada.</param>
    /// <param name="title">Título opcional.</param>
    /// <param name="description">Descripción opcional.</param>
    /// <param name="image">URL de imagen opcional.</param>
    /// <param name="isPublic">Indica si el enlace es público.</param>
    /// <returns>Nueva instancia de <see cref="Link"/>.</returns>
    private static Link CreateInternal(long userId, long collectionId, string urlOriginal, string? title, string? description, string? image, bool isPublic)
    {
        string adjustedUrl = DomainPolicies.LinkUrlPolicy.Adjust(urlOriginal);
        DomainPolicies.UrlValidationPolicy.Validate(adjustedUrl);

        string normalizedUrl = DomainPolicies.UrlNormalizationPolicy.Normalize(adjustedUrl);

        string? nextTitle = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(title, LengthLimitsConstants.LinkTitle);
        string? nextDescription = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(description, LengthLimitsConstants.LinkDescription);
        string? nextImage = NormalizeOptionalImage(image, adjustedUrl, LengthLimitsConstants.LinkImage);

        return new Link
        {
            UserId = userId,
            CollectionId = collectionId,
            UrlOriginal = adjustedUrl,
            UrlNormalized = normalizedUrl,
            Title = nextTitle,
            Description = nextDescription,
            Image = nextImage,
            IsPublic = isPublic,
            ScrapingStatus = DomainEnums.ScrapingStatus.NotRequested,
        };
    }

    /// <summary>
    /// Normaliza una URL de imagen opcional aplicando el límite de longitud y el ajuste correspondiente.
    /// </summary>
    /// <param name="input">URL de imagen a normalizar.</param>
    /// <param name="baseUrl">URL base para resolver URLs relativas.</param>
    /// <param name="maximumLength">Longitud máxima permitida.</param>
    /// <returns>URL normalizada o <c>null</c>.</returns>
    private static string? NormalizeOptionalImage(string? input, string baseUrl, int maximumLength)
    {
        string? value = DomainPolicies.UserTextPolicy.EnsureOptionalMaximumLength(input, maximumLength);
        return value is null ? null : DomainPolicies.ImageUrlPolicy.Adjust(value, baseUrl);
    }
}
