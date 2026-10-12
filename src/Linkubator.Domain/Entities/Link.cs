using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;
using DomainEnums = Linkubator.Domain.Enums;
using Bases = Linkubator.Domain.Entities.Bases;

namespace Linkubator.Domain.Entities;

public sealed class Link : Bases.BaseEntity
{
    private const int MaximumTitleLength = 300;
    private const int MaximumDescriptionLength = 1000;
    private const int MaximumImageLength = 2048;

    private Link()
    {
    }

    public long UserId { get; private set; }

    public long CollectionId { get; private set; }

    public string UrlOriginal { get; private set; } = string.Empty;

    public string UrlNormalized { get; private set; } = string.Empty;

    public string? Title { get; private set; }

    public string? Description { get; private set; }

    public string? Image { get; private set; }

    public bool IsPublic { get; private set; }

    public DomainEnums.ScrapingStatus ScrapingStatus { get; private set; }

    public static Link Create(long userId, long collectionId, string urlOriginal, string? title, string? description, string? image)
    {
        if (userId <= 0 || collectionId <= 0)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        string adjustedUrl = DomainPolicies.LinkUrlPolicy.Adjust(urlOriginal);
        DomainPolicies.UrlValidationPolicy.Validate(adjustedUrl);

        string normalizedUrl = DomainPolicies.UrlNormalizationPolicy.Normalize(adjustedUrl);

        string? nextTitle = NormalizeOptionalText(title, MaximumTitleLength);
        string? nextDescription = NormalizeOptionalText(description, MaximumDescriptionLength);
        string? nextImage = NormalizeOptionalImage(image, adjustedUrl, MaximumImageLength);

        return new Link
        {
            UserId = userId,
            CollectionId = collectionId,
            UrlOriginal = adjustedUrl,
            UrlNormalized = normalizedUrl,
            Title = nextTitle,
            Description = nextDescription,
            Image = nextImage,
            IsPublic = false,
            ScrapingStatus = DomainEnums.ScrapingStatus.NotRequested,
        };
    }

    public static Link Create(long userId, Collection collection, string urlOriginal, string? title, string? description, string? image)
    {
        if (collection is null)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        if (userId <= 0 || collection.Id <= 0 || collection.UserId != userId)
        {
            throw new DomainExceptions.NotValidInputException();
        }

        return Create(userId, collection.Id, urlOriginal, title, description, image);
    }

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

        CollectionId = destinationCollection.Id;
    }

    public void Edit(string? title, string? description, string? image)
    {
        string? nextTitle = NormalizeOptionalText(title, MaximumTitleLength);
        string? nextDescription = NormalizeOptionalText(description, MaximumDescriptionLength);
        string? nextImage = NormalizeOptionalImage(image, UrlOriginal, MaximumImageLength);

        Title = nextTitle;
        Description = nextDescription;
        Image = nextImage;
    }

    private static string? NormalizeOptionalText(string? input, int maximumLength)
    {
        string? value = DomainPolicies.UserTextPolicy.TrimToNull(input);
        if (value is null)
        {
            return null;
        }

        if (!DomainPolicies.UserTextPolicy.IsWithinMaximumLength(value, maximumLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        return value;
    }

    private static string? NormalizeOptionalImage(string? input, string baseUrl, int maximumLength)
    {
        string? value = DomainPolicies.UserTextPolicy.TrimToNull(input);
        if (value is null)
        {
            return null;
        }

        if (!DomainPolicies.UserTextPolicy.IsWithinMaximumLength(value, maximumLength))
        {
            throw new DomainExceptions.TextExceedsMaximumLengthException();
        }

        return DomainPolicies.ImageUrlPolicy.Adjust(value, baseUrl);
    }
}
