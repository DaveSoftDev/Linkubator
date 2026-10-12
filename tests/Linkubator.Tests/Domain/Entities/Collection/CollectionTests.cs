using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainEntities = Linkubator.Domain.Entities;

namespace Linkubator.Tests.Domain.Entities;

[Trait("Category", "Collections")]
public class CollectionTests
{
    [Fact]
    public void Create_WhenValid_SetsPrivateCollectionState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "  My Collection  ", "  A nice one  ");

        Assert.Equal(7, collection.UserId);
        Assert.Equal("My Collection", collection.Name);
        Assert.Equal("my-collection", collection.Slug);
        Assert.Equal("A nice one", collection.Description);
        Assert.False(collection.IsPublic);
        Assert.Equal(0, collection.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenUserIdIsNotPositive_Throws(long userId)
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Collection.Create(userId, "Collection", null));
    }

    [Fact]
    public void Create_WhenNameAndDescriptionAreAtMaximumLength_AcceptsValues()
    {
        string name = new('n', 50);
        string description = new('d', 250);

        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, name, description);

        Assert.Equal(name, collection.Name);
        Assert.Equal(description, collection.Description);
    }

    [Fact]
    public void Create_WhenNameOrDescriptionExceedsMaximumLength_Throws()
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Collection.Create(7, new string('n', 51), null));
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Collection.Create(7, "Collection", new string('d', 251)));
    }

    [Fact]
    public void Create_WhenNameProducesAnEmptySlug_Throws()
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.Collection.Create(7, "!!!", null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeDescription_WhenEmpty_ClearsDescription(string? description)
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "Collection", "Original");

        collection.ChangeDescription(description);

        Assert.Null(collection.Description);
    }

    [Fact]
    public void ChangeDescription_WhenValid_TrimsAndAcceptsMaximumLength()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "Collection", null);
        string description = new('d', 250);

        collection.ChangeDescription($"  {description}  ");

        Assert.Equal(description, collection.Description);
    }

    [Fact]
    public void ChangeDescription_WhenTooLong_LeavesPreviousDescriptionUnchanged()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "Collection", "Original");

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            collection.ChangeDescription(new string('d', 251)));

        Assert.Equal("Original", collection.Description);
    }

    [Fact]
    public void Rename_WhenInvalid_LeavesPreviousNameAndSlugUnchanged()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc");

        Assert.ThrowsAny<DomainExceptions.DomainException>(() => collection.Rename("  "));

        Assert.Equal("My Collection", collection.Name);
        Assert.Equal("my-collection", collection.Slug);
    }

    [Fact]
    public void Rename_WhenNameExceedsMaximumLength_LeavesPreviousNameAndSlugUnchanged()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc");

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            collection.Rename(new string('n', 51)));

        Assert.Equal("My Collection", collection.Name);
        Assert.Equal("my-collection", collection.Slug);
    }

    [Fact]
    public void Rename_WhenNameIsAtMaximumLength_AcceptsNameAndRegeneratesSlug()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc");
        string name = new('n', 50);

        collection.Rename(name);

        Assert.Equal(name, collection.Name);
        Assert.Equal(name, collection.Slug);
    }

    [Fact]
    public void Rename_WhenValid_RecalculatesSlug()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc");

        collection.Rename("Renamed Collection");

        Assert.Equal("Renamed Collection", collection.Name);
        Assert.Equal("renamed-collection", collection.Slug);
    }
}
