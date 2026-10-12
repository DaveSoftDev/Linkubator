using DomainEnums = Linkubator.Domain.Enums;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainEntities = Linkubator.Domain.Entities;

namespace Linkubator.Tests.Domain.Entities;

[Trait("Category", "Links")]
public class LinkTests
{
    [Fact]
    public void Create_WhenValid_StoresNormalizedUrlAndInitialState()
    {
        DomainEntities.Link link = DomainEntities.Link.Create(
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
            DomainEntities.Link.Create(userId, collectionId, "https://example.com", null, null, null));
    }

    [Fact]
    public void Create_WhenCollectionBelongsToAnotherUser_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "Collection", null);
        SetEntityId(collection, 12);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Link.Create(1, collection, "https://example.com", null, null, null));
    }

    [Fact]
    public void MoveToCollection_WhenDestinationBelongsToAnotherUser_ThrowsAndKeepsCurrentCollection()
    {
        DomainEntities.Collection sourceCollection = DomainEntities.Collection.Create(1, "Source", null);
        SetEntityId(sourceCollection, 9);

        DomainEntities.Link link = DomainEntities.Link.Create(1, sourceCollection, "https://example.com", null, null, null);

        DomainEntities.Collection destinationCollection = DomainEntities.Collection.Create(2, "Destination", null);
        SetEntityId(destinationCollection, 21);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() => link.MoveToCollection(destinationCollection));
        Assert.Equal(9, link.CollectionId);
    }

    [Fact]
    public void Create_WhenUrlIsInvalid_Throws()
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Link.Create(1, 2, "ftp://example.com/path", null, null, null));
    }

    [Fact]
    public void Create_WhenOptionalFieldsAreEmpty_SetsThemToNull()
    {
        DomainEntities.Link link = DomainEntities.Link.Create(
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

        DomainEntities.Link link = DomainEntities.Link.Create(
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
            DomainEntities.Link.Create(1, 2, "https://example.com", new string('t', 301), null, null));
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Link.Create(1, 2, "https://example.com", null, new string('d', 1001), null));
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Link.Create(1, 2, "https://example.com", null, null, new string('i', 2049)));
    }

    [Fact]
    public void Edit_WhenInvalid_LeavesPreviousValuesUnchanged()
    {
        DomainEntities.Link link = DomainEntities.Link.Create(1, 2, "https://example.com/path", "Original", "Desc", null);

        string longTitle = new('x', 301);
        Assert.ThrowsAny<DomainExceptions.DomainException>(() => link.Edit(longTitle, "New desc", null));

        Assert.Equal("Original", link.Title);
        Assert.Equal("Desc", link.Description);
    }

    private static void SetEntityId(DomainEntities.Collection collection, long id)
    {
        System.Reflection.PropertyInfo? property = typeof(DomainEntities.Collection).BaseType?.GetProperty("Id");
        property?.SetValue(collection, id);
    }

    [Fact]
    public void Edit_WhenValid_TrimsAndReplacesOptionalValues()
    {
        DomainEntities.Link link = DomainEntities.Link.Create(1, 2, "https://example.com/path", "Original", "Old", null);

        link.Edit("  New title  ", "  New description  ", " //images.example.com/new.png ");

        Assert.Equal("New title", link.Title);
        Assert.Equal("New description", link.Description);
        Assert.Equal("https://images.example.com/new.png", link.Image);
    }

    [Fact]
    public void Edit_WhenOptionalFieldsAreEmpty_ClearsThem()
    {
        DomainEntities.Link link = DomainEntities.Link.Create(
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
        DomainEntities.Link link = DomainEntities.Link.Create(1, 2, "https://example.com/path", null, null, null);
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
        DomainEntities.Link link = DomainEntities.Link.Create(
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
        DomainEntities.Link link = DomainEntities.Link.Create(
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
}
