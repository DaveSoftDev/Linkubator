using DomainEnums = Linkubator.Domain.Enums;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainEntities = Linkubator.Domain.Entities;
using System.Reflection;

namespace Linkubator.Tests.Domain.Entities;

[Trait("Category", "Links")]
public class LinkTests
{
    [Fact]
    public void Create_WhenValid_StoresNormalizedUrlAndInitialState()
    {
        DomainEntities.Link link = CreateLink(
            1,
            2,
            " https://Example.com/path?utm_source=ads&x=2 ",
            "  Title  ",
            "  Desc  ",
            " //images.example.com/logo.png ");

        Assert.Equal(1, link.UserId);
        Assert.Equal(2, link.CollectionId);
        Assert.StartsWith("https://Example.com", link.UrlOriginal);
        Assert.Equal("examplecompathx2", link.UrlNormalized);
        Assert.Equal("Title", link.Title);
        Assert.Equal("Desc", link.Description);
        Assert.False(link.IsPublic);
        Assert.Equal(DomainEnums.ScrapingStatus.NotRequested, link.ScrapingStatus);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(-1, 2)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Create_WhenIdsAreNotPositive_Throws(long userId, long collectionId)
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            CreateLink(userId, collectionId, "https://example.com", null, null, null));
    }

    [Fact]
    public void Create_WhenCollectionBelongsToAnotherUser_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "Collection", null);
        EntityTestFactory.SetId(collection, 12);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null));
    }

    [Fact]
    public void MoveToCollection_WhenDestinationBelongsToAnotherUser_ThrowsAndKeepsCurrentCollection()
    {
        DomainEntities.Collection sourceCollection = DomainEntities.Collection.Create(1, "Source", null, true);
        EntityTestFactory.SetId(sourceCollection, 9);

        DomainEntities.Link link = DomainEntities.Link.Create(1, sourceCollection, "https://example.com", null, null, null, true);

        DomainEntities.Collection destinationCollection = DomainEntities.Collection.Create(2, "Destination", null);
        EntityTestFactory.SetId(destinationCollection, 21);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() => link.MoveToCollection(destinationCollection));
        Assert.Equal(9, link.CollectionId);
        Assert.True(link.IsPublic);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MoveToCollection_WhenDestinationIsTheSameCollection_LeavesStateUnchanged(bool linkIsPublic)
    {
        DomainEntities.Collection sourceCollection = DomainEntities.Collection.Create(1, "Source", null, linkIsPublic);
        EntityTestFactory.SetId(sourceCollection, 9);

        DomainEntities.Link link = DomainEntities.Link.Create(1, sourceCollection, "https://example.com/path", null, null, null, linkIsPublic);

        DomainEntities.Collection destinationCollection = DomainEntities.Collection.Create(1, "Destination", null);
        EntityTestFactory.SetId(destinationCollection, 9);

        link.MoveToCollection(destinationCollection);

        Assert.Equal(9, link.CollectionId);
        Assert.Equal(linkIsPublic, link.IsPublic);
    }

    [Fact]
    public void Create_WhenUrlIsInvalid_Throws()
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            CreateLink(1, 2, "ftp://example.com/path", null, null, null));
    }

    [Fact]
    public void Create_WhenImageContainsCredentials_Throws()
    {
        Assert.Throws<DomainExceptions.UrlCredentialsNotAllowedException>(() =>
            CreateLink(1, 2, "https://example.com/path", null, null, "https://usuario:clave@cdn.example.com/i.png"));
    }

    [Fact]
    public void Create_WhenImageHostHasASingleLetterTld_Throws()
    {
        Assert.Throws<DomainExceptions.UrlHostInvalidException>(() =>
            CreateLink(1, 2, "https://example.com/path", null, null, "https://cdn.example.c/i.png"));
    }

    [Fact]
    public void Create_WhenImagePortIsZero_Throws()
    {
        Assert.Throws<DomainExceptions.UrlPortInvalidException>(() =>
            CreateLink(1, 2, "https://example.com/path", null, null, "https://cdn.example.com:0/i.png"));
    }

    [Theory]
    [InlineData("https://0xC0A80101/x")]
    [InlineData("http://0xC0A80101:8080/x")]
    [InlineData("0xC0A80101:8080/x")]
    [InlineData("2130706433/x")]
    [InlineData("https://127.1")]
    [InlineData("https://192.168.1/x")]
    public void Create_WhenIpv4HostIsNotWrittenAsFourDecimalGroups_Throws(string urlOriginal)
    {
        Assert.Throws<DomainExceptions.UrlHostInvalidException>(() =>
            CreateLink(1, 2, urlOriginal, null, null, null));
    }

    [Theory]
    [InlineData("https://192.168.1.1/x")]
    [InlineData("http://192.168.1.1:8080/x")]
    [InlineData("https://192.168.001.001/x")]
    public void Create_WhenIpv4HostIsWrittenAsFourDecimalGroups_KeepsTheWrittenHost(string urlOriginal)
    {
        DomainEntities.Link link = CreateLink(1, 2, urlOriginal, null, null, null);

        Assert.Equal(urlOriginal, link.UrlOriginal);
    }

    [Fact]
    public void Create_WhenTheSameIpv4HostIsWrittenWithLeadingZeros_ProducesTheSameNormalizedKey()
    {
        DomainEntities.Link withLeadingZeros = CreateLink(1, 2, "https://192.168.001.001/x", null, null, null);
        DomainEntities.Link withoutLeadingZeros = CreateLink(1, 2, "https://192.168.1.1/x", null, null, null);

        Assert.NotEqual(withLeadingZeros.UrlOriginal, withoutLeadingZeros.UrlOriginal);
        Assert.Equal(withoutLeadingZeros.UrlNormalized, withLeadingZeros.UrlNormalized);
    }

    [Fact]
    public void Create_WhenOptionalFieldsAreEmpty_SetsThemToNull()
    {
        DomainEntities.Link link = CreateLink(
            1,
            2,
            "https://example.com/path",
            "   ",
            "  ",
            " ");

        Assert.Null(link.Title);
        Assert.Null(link.Description);
        Assert.Null(link.Image);
    }

    [Fact]
    public void Create_WhenFieldsAreAtMaximumLength_AcceptsValues()
    {
        string title = new('t', 300);
        string description = new('d', 1000);
        string imagePrefix = "https://images.example.com/";
        string image = imagePrefix + new string('i', 2048 - imagePrefix.Length);

        DomainEntities.Link link = CreateLink(
            1,
            2,
            "https://example.com/path",
            title,
            description,
            image);

        Assert.Equal(title, link.Title);
        Assert.Equal(description, link.Description);
        Assert.Equal(2048, link.Image!.Length);
    }

    [Fact]
    public void Create_WhenOptionalFieldExceedsMaximumLength_Throws()
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            CreateLink(1, 2, "https://example.com", new string('t', 301), null, null));
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            CreateLink(1, 2, "https://example.com", null, new string('d', 1001), null));
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            CreateLink(1, 2, "https://example.com", null, null, new string('i', 2049)));
    }

    [Fact]
    public void Edit_WhenInvalid_LeavesPreviousValuesUnchanged()
    {
        DomainEntities.Link link = CreateLink(1, 2, "https://example.com/path", "Original", "Desc", null);

        string longTitle = new('x', 301);
        Assert.ThrowsAny<DomainExceptions.DomainException>(() => link.Edit(longTitle, "New desc", null));

        Assert.Equal("Original", link.Title);
        Assert.Equal("Desc", link.Description);
    }

    private static DomainEntities.Link CreateLink(long userId, long collectionId, string urlOriginal, string? title, string? description, string? image)
    {
        long collectionOwnerId = userId > 0 ? userId : 1;
        DomainEntities.Collection collection = DomainEntities.Collection.Create(collectionOwnerId, "Collection", null);
        EntityTestFactory.SetId(collection, collectionId);

        return DomainEntities.Link.Create(userId, collection, urlOriginal, title, description, image);
    }

    [Fact]
    public void Edit_WhenValid_TrimsAndReplacesOptionalValues()
    {
        DomainEntities.Link link = CreateLink(1, 2, "https://example.com/path", "Original", "Old", null);

        link.Edit("  New title  ", "  New description  ", " //images.example.com/new.png ");

        Assert.Equal("New title", link.Title);
        Assert.Equal("New description", link.Description);
        Assert.Equal("https://images.example.com/new.png", link.Image);
    }

    [Fact]
    public void Edit_WhenOptionalFieldsAreEmpty_ClearsThem()
    {
        DomainEntities.Link link = CreateLink(
            1,
            2,
            "https://example.com/path",
            "Original",
            "Description",
            "https://images.example.com/old.png");

        link.Edit(null, "  ", "");

        Assert.Null(link.Title);
        Assert.Null(link.Description);
        Assert.Null(link.Image);
    }

    [Fact]
    public void Edit_WhenFieldsAreAtMaximumLength_AcceptsValues()
    {
        DomainEntities.Link link = CreateLink(1, 2, "https://example.com/path", null, null, null);
        string title = new('t', 300);
        string description = new('d', 1000);
        string imagePrefix = "https://images.example.com/";
        string image = imagePrefix + new string('i', 2048 - imagePrefix.Length);

        link.Edit(title, description, image);

        Assert.Equal(title, link.Title);
        Assert.Equal(description, link.Description);
        Assert.Equal(2048, link.Image!.Length);
    }

    [Fact]
    public void Edit_WhenDescriptionOrImageExceedsMaximumLength_LeavesValuesUnchanged()
    {
        DomainEntities.Link link = CreateLink(
            1,
            2,
            "https://example.com/path",
            "Original title",
            "Original description",
            "https://images.example.com/old.png");

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            link.Edit("New title", new string('d', 1001), null));
        Assert.Equal("Original title", link.Title);
        Assert.Equal("Original description", link.Description);
        Assert.Equal("https://images.example.com/old.png", link.Image);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            link.Edit("New title", "New description", new string('i', 2049)));
        Assert.Equal("Original title", link.Title);
        Assert.Equal("Original description", link.Description);
        Assert.Equal("https://images.example.com/old.png", link.Image);
    }

    [Fact]
    public void Edit_WhenImageIsInvalid_LeavesAllPreviousValuesUnchanged()
    {
        DomainEntities.Link link = CreateLink(
            1,
            2,
            "https://example.com/path",
            "Original title",
            "Original description",
            "https://images.example.com/old.png");

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            link.Edit("New title", "New description", "https://localhost/image.png"));

        Assert.Equal("Original title", link.Title);
        Assert.Equal("Original description", link.Description);
        Assert.Equal("https://images.example.com/old.png", link.Image);
    }

    [Fact]
    public void Create_WhenCollectionIsPublicAndPublicRequested_SetsPublicState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null, true);
        EntityTestFactory.SetId(collection, 22);

        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com/public", null, null, null, true);

        Assert.True(link.IsPublic);
        Assert.Equal(22, link.CollectionId);
    }

    [Fact]
    public void Create_WhenCollectionIsPrivateAndPublicRequested_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 22);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Link.Create(1, collection, "https://example.com/private", null, null, null, true));
    }

    [Fact]
    public void ChangePrivacy_WhenPublicRequestedOnPrivateCollection_ThrowsAndKeepsPreviousState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 22);

        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com/private", null, null, null);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() => link.ChangePrivacy(true, collection));
        Assert.False(link.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenPublicRequestedOnPublicCollection_SetsPublicState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null, true);
        EntityTestFactory.SetId(collection, 22);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com/private", null, null, null);

        link.ChangePrivacy(true, collection);

        Assert.True(link.IsPublic);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void MoveToCollection_WhenValid_LeavesLinkPrivate(bool sourceIsPublic, bool destinationIsPublic)
    {
        DomainEntities.Collection sourceCollection = DomainEntities.Collection.Create(1, "Source", null, sourceIsPublic);
        EntityTestFactory.SetId(sourceCollection, 9);

        DomainEntities.Collection destinationCollection = DomainEntities.Collection.Create(1, "Destination", null, destinationIsPublic);
        EntityTestFactory.SetId(destinationCollection, 21);

        DomainEntities.Link link = DomainEntities.Link.Create(1, sourceCollection, "https://example.com/path", null, null, null, sourceIsPublic);

        link.MoveToCollection(destinationCollection);

        Assert.Equal(21, link.CollectionId);
        Assert.False(link.IsPublic);
    }

    [Fact]
    public void Create_WhenCollectionIsNull_Throws()
    {
        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            DomainEntities.Link.Create(1, null!, "https://example.com", null, null, null));
    }

    [Fact]
    public void Create_WhenCollectionHasNoId_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null));
    }

    [Fact]
    public void ChangePrivacy_WhenCollectionIsNull_Throws()
    {
        DomainEntities.Link link = CreateLink(1, 2, "https://example.com", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => link.ChangePrivacy(true, null!));

        Assert.False(link.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenCollectionHasNoId_ThrowsAndKeepsPreviousState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 22);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        DomainEntities.Collection collectionWithoutId = DomainEntities.Collection.Create(1, "Collection Without Id", null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => link.ChangePrivacy(true, collectionWithoutId));

        Assert.False(link.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenCollectionBelongsToAnotherUser_ThrowsAndKeepsPreviousState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null, true);
        EntityTestFactory.SetId(collection, 22);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        DomainEntities.Collection otherUsersCollection = DomainEntities.Collection.Create(2, "Other Users Collection", null, true);
        EntityTestFactory.SetId(otherUsersCollection, 22);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => link.ChangePrivacy(true, otherUsersCollection));

        Assert.False(link.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenCollectionIsAnotherOfTheSameUser_ThrowsAndKeepsPreviousState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null, true);
        EntityTestFactory.SetId(collection, 22);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        DomainEntities.Collection otherCollection = DomainEntities.Collection.Create(1, "Other Collection", null, true);
        EntityTestFactory.SetId(otherCollection, 23);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => link.ChangePrivacy(true, otherCollection));

        Assert.False(link.IsPublic);
    }

    [Fact]
    public void MoveToCollection_WhenDestinationIsNull_ThrowsAndKeepsCurrentCollection()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 9);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => link.MoveToCollection(null!));

        Assert.Equal(9, link.CollectionId);
    }

    [Fact]
    public void RehydrateCollection_WhenCollectionMatchesLink_RebindsTheBidirectionalReference()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 9);

        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        EntityTestFactory.RehydrateCollection(link, collection);

        Assert.Equal(collection.Id, link.CollectionId);
    }

    [Fact]
    public void RehydrateCollection_WhenCollectionIsNull_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 9);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => EntityTestFactory.RehydrateCollection(link, null!));
    }

    [Fact]
    public void RehydrateCollection_WhenCollectionBelongsToAnotherUser_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 9);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        DomainEntities.Collection otherUserCollection = DomainEntities.Collection.Create(2, "Other User Collection", null);
        EntityTestFactory.SetId(otherUserCollection, 9);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => EntityTestFactory.RehydrateCollection(link, otherUserCollection));
    }

    [Fact]
    public void RehydrateCollection_WhenCollectionDoesNotMatchLink_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 9);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        DomainEntities.Collection otherCollection = DomainEntities.Collection.Create(1, "Other Collection", null);
        EntityTestFactory.SetId(otherCollection, 10);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => EntityTestFactory.RehydrateCollection(link, otherCollection));
    }

    [Fact]
    public void MoveToCollection_WhenDestinationIsTheSameInstanceButItsIdChanges_UsesReferenceBranch()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 9);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        PropertyInfo? property = typeof(DomainEntities.Bases.BaseEntity).GetProperty(nameof(DomainEntities.Bases.BaseEntity.Id), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        property!.SetValue(collection, 42L);

        link.MoveToCollection(collection);

        Assert.Equal(42L, link.CollectionId);
        Assert.False(link.IsPublic);
    }

    [Fact]
    public void MoveToCollection_WhenDestinationHasNoId_ThrowsAndKeepsCurrentCollection()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(1, "Collection", null);
        EntityTestFactory.SetId(collection, 9);
        DomainEntities.Link link = DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null);

        DomainEntities.Collection destinationWithoutId = DomainEntities.Collection.Create(1, "Destination Without Id", null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => link.MoveToCollection(destinationWithoutId));

        Assert.Equal(9, link.CollectionId);
        Assert.False(link.IsPublic);
    }
}
