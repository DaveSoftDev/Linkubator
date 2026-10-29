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
    public void Rename_WhenNameProducesAnEmptySlug_LeavesPreviousNameAndSlugUnchanged()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc");

        Assert.ThrowsAny<DomainExceptions.DomainException>(() => collection.Rename("!!!"));

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

    [Fact]
    public void Create_WhenPublicRequested_SetsPublicCollectionState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc", true);

        Assert.True(collection.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenMakingPrivate_UpdatesLinksAndPreservesPublicState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc", true);
        EntityTestFactory.SetId(collection, 12);

        DomainEntities.Link first = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null, true);
        DomainEntities.Link second = DomainEntities.Link.Create(7, collection, "https://example.com/2", null, null, null, false);

        collection.ChangePrivacy(false, new[] { first, second });

        Assert.False(collection.IsPublic);
        Assert.False(first.IsPublic);
        Assert.False(second.IsPublic);

        collection.ChangePrivacy(true, new[] { first, second });

        Assert.True(collection.IsPublic);
        Assert.False(first.IsPublic);
        Assert.False(second.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenUsingLoadedLinks_MakesThemPrivate()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null, true);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link link = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null, true);

        collection.ChangePrivacy(false);

        Assert.False(collection.IsPublic);
        Assert.False(link.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenLinkDoesNotBelongToCollection_ThrowsWithoutPartialMutation()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc", true);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link associatedLink = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null, true);

        DomainEntities.Collection otherCollection = DomainEntities.Collection.Create(7, "Other Collection", null);
        EntityTestFactory.SetId(otherCollection, 13);
        DomainEntities.Link unrelatedLink = DomainEntities.Link.Create(7, otherCollection, "https://example.com/2", null, null, null);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            collection.ChangePrivacy(false, new[] { associatedLink, unrelatedLink }));

        Assert.True(collection.IsPublic);
        Assert.True(associatedLink.IsPublic);
        Assert.False(unrelatedLink.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenSubsetOfLinksIsProvided_ThrowsWithoutPartialMutation()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc", true);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link first = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null, true);
        DomainEntities.Link second = DomainEntities.Link.Create(7, collection, "https://example.com/2", null, null, null, true);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            collection.ChangePrivacy(false, new[] { first }));

        Assert.True(collection.IsPublic);
        Assert.True(first.IsPublic);
        Assert.True(second.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenDuplicateLinkReplacesMissingLink_ThrowsWithoutPartialMutation()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc", true);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link first = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null, true);
        DomainEntities.Link second = DomainEntities.Link.Create(7, collection, "https://example.com/2", null, null, null, true);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            collection.ChangePrivacy(false, new[] { first, first }));

        Assert.True(collection.IsPublic);
        Assert.True(first.IsPublic);
        Assert.True(second.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenLinksIsNull_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc", true);
        EntityTestFactory.SetId(collection, 12);

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            collection.ChangePrivacy(true, null!));

        Assert.True(collection.IsPublic);
    }

    [Fact]
    public void ChangePrivacy_WhenLinkHasMovedToAnotherCollection_ThrowsWithoutPartialMutation()
    {
        DomainEntities.Collection originCollection = DomainEntities.Collection.Create(1, "Origin", null, true);
        EntityTestFactory.SetId(originCollection, 9);
        DomainEntities.Link link = DomainEntities.Link.Create(1, originCollection, "https://example.com/path", null, null, null, true);

        DomainEntities.Collection holderCollection = DomainEntities.Collection.Create(1, "Holder", null, true);
        EntityTestFactory.SetId(holderCollection, 9);
        EntityTestFactory.AddLink(holderCollection, link);

        DomainEntities.Collection destinationCollection = DomainEntities.Collection.Create(1, "Destination", null);
        EntityTestFactory.SetId(destinationCollection, 21);
        link.MoveToCollection(destinationCollection);

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            holderCollection.ChangePrivacy(false, new[] { link }));

        Assert.True(holderCollection.IsPublic);
        Assert.Equal(21, link.CollectionId);
        Assert.False(link.IsPublic);
    }

    [Fact]
    public void AddLink_WhenLinkAlreadyExists_DoesNotDuplicate()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link link = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);

        EntityTestFactory.AddLink(collection, link);
        EntityTestFactory.AddLink(collection, link);

        Assert.Single(collection.Links);
    }

    [Fact]
    public void AddLink_WhenLinkBelongsToAnotherUser_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        DomainEntities.Collection otherUsersCollection = DomainEntities.Collection.Create(8, "Other Users Collection", null);
        EntityTestFactory.SetId(otherUsersCollection, 12);
        DomainEntities.Link otherUsersLink = DomainEntities.Link.Create(8, otherUsersCollection, "https://example.com/1", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            EntityTestFactory.AddLink(collection, otherUsersLink));
    }

    [Fact]
    public void AddLink_WhenLinkBelongsToAnotherCollection_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        DomainEntities.Collection otherCollection = DomainEntities.Collection.Create(7, "Other Collection", null);
        EntityTestFactory.SetId(otherCollection, 13);
        DomainEntities.Link link = DomainEntities.Link.Create(7, otherCollection, "https://example.com/1", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            EntityTestFactory.AddLink(collection, link));
    }

    [Fact]
    public void ChangePrivacy_WhenLinkBelongsToDifferentCollection_ThrowsWithoutPartialMutation()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc", true);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link associatedLink = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null, true);

        DomainEntities.Collection otherCollection = DomainEntities.Collection.Create(7, "Other Collection", null);
        EntityTestFactory.SetId(otherCollection, 13);
        DomainEntities.Link wrongCollectionLink = DomainEntities.Link.Create(7, otherCollection, "https://example.com/2", null, null, null);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            collection.ChangePrivacy(false, new[] { associatedLink, wrongCollectionLink }));

        Assert.True(collection.IsPublic);
        Assert.True(associatedLink.IsPublic);
        Assert.False(wrongCollectionLink.IsPublic);
    }

    [Fact]
    public void RemoveLink_WhenLinkExists_RemovesItFromCollection()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link link = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);
        EntityTestFactory.AddLink(collection, link);

        EntityTestFactory.RemoveLink(collection, link);

        Assert.Empty(collection.Links);
    }

    [Fact]
    public void RemoveLink_WhenLinkDoesNotExist_DoesNothing()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link link = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);

        EntityTestFactory.RemoveLink(collection, link);

        Assert.Empty(collection.Links); // Still empty
    }

    [Fact]
    public void RemoveLink_WhenLinkIsNull_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            EntityTestFactory.RemoveLink(collection, null!));
    }

    [Fact]
    public void RehydrateLinks_WhenLinksAreValid_RefreshesTheCollectionMembership()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        DomainEntities.Link first = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);
        DomainEntities.Link second = DomainEntities.Link.Create(7, collection, "https://example.com/2", null, null, null);

        EntityTestFactory.RehydrateLinks(collection, first, second, first);

        Assert.Equal(2, collection.Links.Count);
        Assert.Contains(first, collection.Links);
        Assert.Contains(second, collection.Links);
    }

    [Fact]
    public void RehydrateLinks_WhenSourceIsItsOwnLinks_PreservesMembership()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link first = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);
        DomainEntities.Link second = DomainEntities.Link.Create(7, collection, "https://example.com/2", null, null, null);

        collection.RehydrateLinks(collection.Links);

        Assert.Equal(new[] { first, second }, collection.Links);
        Assert.Same(collection, first.Collection);
        Assert.Same(collection, second.Collection);
    }

    [Fact]
    public void RehydrateLinks_WhenSourceIsDeferredOverItsOwnLinks_PreservesSelectedMembership()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link first = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);
        _ = DomainEntities.Link.Create(7, collection, "https://example.com/2", null, null, null);
        IEnumerable<DomainEntities.Link> selected = collection.Links.Where(link => ReferenceEquals(link, first));

        collection.RehydrateLinks(selected);

        Assert.Same(first, Assert.Single(collection.Links));
    }

    [Fact]
    public void RehydrateLinks_WhenLaterLinkIsInvalid_PreservesExistingMembershipAndOtherInstance()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link original = DomainEntities.Link.Create(7, collection, "https://example.com/original", null, null, null);
        DomainEntities.Collection otherInstance = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(otherInstance, 12);
        DomainEntities.Link candidate = DomainEntities.Link.Create(7, otherInstance, "https://example.com/candidate", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => collection.RehydrateLinks(new[] { candidate, null! }));

        Assert.Same(original, Assert.Single(collection.Links));
        Assert.Same(collection, original.Collection);
        Assert.Same(candidate, Assert.Single(otherInstance.Links));
        Assert.Same(otherInstance, candidate.Collection);
    }

    [Fact]
    public void RehydrateLinks_WhenEnumerationFails_PreservesExistingMembership()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link original = DomainEntities.Link.Create(7, collection, "https://example.com/original", null, null, null);
        IEnumerable<DomainEntities.Link> source = collection.Links.Select<DomainEntities.Link, DomainEntities.Link>(
            _ => throw new InvalidOperationException("Enumeration failed."));

        Assert.Throws<InvalidOperationException>(() => collection.RehydrateLinks(source));

        Assert.Same(original, Assert.Single(collection.Links));
        Assert.Same(collection, original.Collection);
    }

    [Fact]
    public void RehydrateLinks_WhenSourceIsEmpty_ClearsMembership()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        _ = DomainEntities.Link.Create(7, collection, "https://example.com/original", null, null, null);

        collection.RehydrateLinks(Array.Empty<DomainEntities.Link>());

        Assert.Empty(collection.Links);
    }

    [Fact]
    public void RehydrateLinks_WhenSourceBelongsToAnotherInstance_ReassignsNavigationWithoutDuplicates()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Collection otherInstance = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(otherInstance, 12);
        DomainEntities.Link candidate = DomainEntities.Link.Create(7, otherInstance, "https://example.com/candidate", null, null, null);

        collection.RehydrateLinks(new[] { candidate, candidate });

        Assert.Same(candidate, Assert.Single(collection.Links));
        Assert.Same(collection, candidate.Collection);
        Assert.Empty(otherInstance.Links);
    }

    [Fact]
    public void RehydrateLinks_WhenLinksIsNull_ThrowsWithoutMutatingState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        DomainEntities.Link original = DomainEntities.Link.Create(7, collection, "https://example.com/original", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => collection.RehydrateLinks(null!));

        Assert.Same(original, Assert.Single(collection.Links));
    }

    [Fact]
    public void RehydrateLinks_WhenLinkIsNull_ThrowsWithoutMutatingState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        DomainEntities.Link original = DomainEntities.Link.Create(7, collection, "https://example.com/original", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => collection.RehydrateLinks(new DomainEntities.Link[] { null! }));

        Assert.Same(original, Assert.Single(collection.Links));
    }

    [Fact]
    public void RehydrateLinks_WhenLinkBelongsToAnotherUser_ThrowsWithoutMutatingState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link validLink = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);

        DomainEntities.Collection otherCollection = DomainEntities.Collection.Create(8, "Other Collection", null);
        EntityTestFactory.SetId(otherCollection, 13);
        DomainEntities.Link foreignUserLink = DomainEntities.Link.Create(8, otherCollection, "https://example.com/2", null, null, null);

        DomainEntities.Link original = DomainEntities.Link.Create(7, collection, "https://example.com/original", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => collection.RehydrateLinks(new[] { validLink, foreignUserLink }));

        Assert.Equal(new[] { validLink, original }, collection.Links);
        Assert.Same(otherCollection, foreignUserLink.Collection);
    }

    [Fact]
    public void RehydrateLinks_WhenLinksDoNotMatchTheCollection_ThrowsWithoutMutatingState()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        DomainEntities.Link validLink = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);
        DomainEntities.Collection otherCollection = DomainEntities.Collection.Create(7, "Other Collection", null);
        EntityTestFactory.SetId(otherCollection, 13);
        DomainEntities.Link foreignLink = DomainEntities.Link.Create(7, otherCollection, "https://example.com/2", null, null, null);

        DomainEntities.Link original = DomainEntities.Link.Create(7, collection, "https://example.com/original", null, null, null);

        Assert.Throws<DomainExceptions.NotValidInputException>(() => collection.RehydrateLinks(new[] { validLink, foreignLink }));

        Assert.Equal(new[] { validLink, original }, collection.Links);
        Assert.Same(otherCollection, foreignLink.Collection);
    }

    [Fact]
    public void ChangePrivacy_WhenCollectionHasNoLinks_DoesNotThrowAndSetsPrivacy()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        collection.ChangePrivacy(true, Array.Empty<DomainEntities.Link>());

        Assert.True(collection.IsPublic);
    }

    [Fact]
    public void AddLink_WhenLinkIsNull_Throws()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            EntityTestFactory.AddLink(collection, null!));
    }

    [Fact]
    public void Rename_WhenNameHasLeadingTrailingSpaces_TrimsAndSetsNameAndSlug()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);

        collection.Rename("  Renamed Collection  ");

        Assert.Equal("Renamed Collection", collection.Name);
        Assert.Equal("renamed-collection", collection.Slug);
    }

    [Fact]
    public void AddLink_WhenLinkIsValidAndNotPresent_AddsToCollection()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", null);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link link = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null);

        EntityTestFactory.AddLink(collection, link);

        Assert.Single(collection.Links);
        Assert.Contains(link, collection.Links);
    }

    [Fact]
    public void ChangePrivacy_WhenLinkIsNullInArray_ThrowsWithoutPartialMutation()
    {
        DomainEntities.Collection collection = DomainEntities.Collection.Create(7, "My Collection", "desc", true);
        EntityTestFactory.SetId(collection, 12);
        DomainEntities.Link associatedLink = DomainEntities.Link.Create(7, collection, "https://example.com/1", null, null, null, true);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            collection.ChangePrivacy(false, new[] { associatedLink, null! }));

        Assert.True(collection.IsPublic);
        Assert.True(associatedLink.IsPublic);
    }
}