using Linkubator.Application.Constants;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Results;
using Linkubator.Application.Validation.Collections;
using Linkubator.Application.Validation.Links;

namespace Linkubator.Tests.Application;

public class ValidationCoverageTests
{
    [Fact]
    public void LinkCreateValidator_CoversNullInvalidAndValidCases()
    {
        LinkCreateValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidCollection = validator.Validate(new LinkCreateRequest(0, "https://example.com"));
        Assert.False(invalidCollection.IsSuccess);
        Assert.Equal("LINK_COLLECTION_ID_INVALID", invalidCollection.Errors[0].Code);

        Result invalidUrl = validator.Validate(new LinkCreateRequest(1, "   "));
        Assert.False(invalidUrl.IsSuccess);
        Assert.Equal("URL_REQUIRED", invalidUrl.Errors[0].Code);

        Result valid = validator.Validate(new LinkCreateRequest(1, "https://example.com"));
        Assert.True(valid.IsSuccess);
    }

    [Fact]
    public void LinkEditValidator_CoversNullInvalidAndValidCases()
    {
        LinkEditValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidLinkId = validator.Validate(new LinkEditRequest(0, 1, "Title"));
        Assert.False(invalidLinkId.IsSuccess);
        Assert.Equal("LINK_ID_INVALID", invalidLinkId.Errors[0].Code);

        Result invalidCollectionId = validator.Validate(new LinkEditRequest(1, 0, "Title"));
        Assert.False(invalidCollectionId.IsSuccess);
        Assert.Equal("LINK_COLLECTION_ID_INVALID", invalidCollectionId.Errors[0].Code);

        Result valid = validator.Validate(new LinkEditRequest(1, 2, "Title"));
        Assert.True(valid.IsSuccess);
    }

    [Fact]
    public void LinkMoveValidator_CoversNullInvalidAndValidCases()
    {
        LinkMoveValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidLinkId = validator.Validate(new LinkMoveRequest(0, 2));
        Assert.False(invalidLinkId.IsSuccess);
        Assert.Equal("LINK_ID_INVALID", invalidLinkId.Errors[0].Code);

        Result invalidDestination = validator.Validate(new LinkMoveRequest(1, 0));
        Assert.False(invalidDestination.IsSuccess);
        Assert.Equal("LINK_DESTINATION_COLLECTION_ID_INVALID", invalidDestination.Errors[0].Code);

        Result valid = validator.Validate(new LinkMoveRequest(1, 2));
        Assert.True(valid.IsSuccess);
    }

    [Fact]
    public void LinkChangeVisibilityValidator_AndDeleteValidator_CoverNullInvalidAndValidCases()
    {
        LinkChangeVisibilityValidator changeVisibilityValidator = new();
        LinkDeleteValidator deleteValidator = new();

        Result changeVisibilityNull = changeVisibilityValidator.Validate(null!);
        Assert.False(changeVisibilityNull.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", changeVisibilityNull.Errors[0].Code);

        Result changeVisibilityInvalid = changeVisibilityValidator.Validate(new LinkChangeVisibilityRequest(0, true));
        Assert.False(changeVisibilityInvalid.IsSuccess);
        Assert.Equal("LINK_ID_INVALID", changeVisibilityInvalid.Errors[0].Code);

        Result changeVisibilityValid = changeVisibilityValidator.Validate(new LinkChangeVisibilityRequest(1, true));
        Assert.True(changeVisibilityValid.IsSuccess);

        Result deleteNull = deleteValidator.Validate(null!);
        Assert.False(deleteNull.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", deleteNull.Errors[0].Code);

        Result deleteInvalid = deleteValidator.Validate(new LinkDeleteRequest(0));
        Assert.False(deleteInvalid.IsSuccess);
        Assert.Equal("LINK_ID_INVALID", deleteInvalid.Errors[0].Code);

        Result deleteValid = deleteValidator.Validate(new LinkDeleteRequest(1));
        Assert.True(deleteValid.IsSuccess);
    }

    [Fact]
    public void LinkValidations_ValidateAllStaticBranches()
    {
        Assert.False(LinkValidations.ValidateLinkId(0).IsSuccess);
        Assert.True(LinkValidations.ValidateLinkId(1).IsSuccess);

        Assert.False(LinkValidations.ValidateCollectionId(0).IsSuccess);
        Assert.True(LinkValidations.ValidateCollectionId(1).IsSuccess);

        Assert.False(LinkValidations.ValidateUrl(null!).IsSuccess);
        Assert.Equal("URL_REQUIRED", LinkValidations.ValidateUrl(null!).Errors[0].Code);
        Assert.False(LinkValidations.ValidateUrl("  ").IsSuccess);
        Assert.Equal("URL_REQUIRED", LinkValidations.ValidateUrl("  ").Errors[0].Code);
        Assert.True(LinkValidations.ValidateUrl("https://example.com").IsSuccess);
    }

    [Fact]
    public void CollectionCreateValidator_CoversNullInvalidAndValidCases()
    {
        CollectionCreateValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidName = validator.Validate(new CollectionCreateRequest("   ", "desc", true));
        Assert.False(invalidName.IsSuccess);
        Assert.Equal("COLLECTION_NAME_REQUIRED", invalidName.Errors[0].Code);

        Result valid = validator.Validate(new CollectionCreateRequest("  My Collection  ", "desc", true));
        Assert.True(valid.IsSuccess);
    }

    [Fact]
    public void CollectionRenameValidator_CoversNullInvalidAndValidCases()
    {
        CollectionRenameValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidCollectionId = validator.Validate(new CollectionRenameRequest(0, "Name"));
        Assert.False(invalidCollectionId.IsSuccess);
        Assert.Equal("COLLECTION_ID_INVALID", invalidCollectionId.Errors[0].Code);

        Result invalidName = validator.Validate(new CollectionRenameRequest(1, "   "));
        Assert.False(invalidName.IsSuccess);
        Assert.Equal("COLLECTION_NAME_REQUIRED", invalidName.Errors[0].Code);

        Result valid = validator.Validate(new CollectionRenameRequest(1, "My Collection"));
        Assert.True(valid.IsSuccess);
    }

    [Fact]
    public void CollectionChangeVisibilityValidator_CoversNullInvalidAndValidCases()
    {
        CollectionChangeVisibilityValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidId = validator.Validate(new CollectionChangeVisibilityRequest(0, true));
        Assert.False(invalidId.IsSuccess);
        Assert.Equal("COLLECTION_ID_INVALID", invalidId.Errors[0].Code);

        Result valid = validator.Validate(new CollectionChangeVisibilityRequest(1, true));
        Assert.True(valid.IsSuccess);
    }

    [Fact]
    public void CollectionDeleteValidator_CoversNullInvalidAndValidCases()
    {
        CollectionDeleteValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidId = validator.Validate(new CollectionDeleteRequest(0));
        Assert.False(invalidId.IsSuccess);
        Assert.Equal("COLLECTION_ID_INVALID", invalidId.Errors[0].Code);

        Result valid = validator.Validate(new CollectionDeleteRequest(1));
        Assert.True(valid.IsSuccess);
    }

    [Fact]
    public void CollectionValidations_ValidateAllStaticBranches()
    {
        Assert.False(CollectionValidations.ValidateCollectionId(0).IsSuccess);
        Assert.True(CollectionValidations.ValidateCollectionId(1).IsSuccess);

        Assert.False(CollectionValidations.ValidateName(null).IsSuccess);
        Assert.False(CollectionValidations.ValidateName("   ").IsSuccess);
        Assert.True(CollectionValidations.ValidateName("Name").IsSuccess);
    }

    [Fact]
    public void CollectionServiceValidators_UseDefaultAndCustomInstances()
    {
        CollectionServiceValidators defaults = new();
        Assert.Null(defaults.CreateCollection);
        Assert.Null(defaults.RenameCollection);
        Assert.Null(defaults.ChangeCollectionVisibility);
        Assert.Null(defaults.DeleteCollection);
        Assert.NotNull(defaults.CollectionCreateValidator);
        Assert.NotNull(defaults.CollectionRenameValidator);
        Assert.NotNull(defaults.CollectionChangeVisibilityValidator);
        Assert.NotNull(defaults.CollectionDeleteValidator);

        Assert.True(defaults.CollectionCreateValidator.Validate(new CollectionCreateRequest("Name", "desc", true)).IsSuccess);
        Assert.True(defaults.CollectionRenameValidator.Validate(new CollectionRenameRequest(1, "Name")).IsSuccess);
        Assert.True(defaults.CollectionChangeVisibilityValidator.Validate(new CollectionChangeVisibilityRequest(1, true)).IsSuccess);
        Assert.True(defaults.CollectionDeleteValidator.Validate(new CollectionDeleteRequest(1)).IsSuccess);

        CollectionCreateValidator createValidator = new();
        CollectionRenameValidator renameValidator = new();
        CollectionChangeVisibilityValidator changeVisibilityValidator = new();
        CollectionDeleteValidator deleteValidator = new();

        CollectionServiceValidators custom = new(
            createValidator,
            renameValidator,
            changeVisibilityValidator,
            deleteValidator);

        Assert.Same(createValidator, custom.CollectionCreateValidator);
        Assert.Same(renameValidator, custom.CollectionRenameValidator);
        Assert.Same(changeVisibilityValidator, custom.CollectionChangeVisibilityValidator);
        Assert.Same(deleteValidator, custom.CollectionDeleteValidator);
    }

    [Fact]
    public void LinkServiceValidators_UseDefaultAndCustomInstances()
    {
        LinkServiceValidators defaults = new();
        Assert.NotNull(defaults.CreateLinkValidator);
        Assert.NotNull(defaults.EditLinkValidator);
        Assert.NotNull(defaults.MoveLinkValidator);
        Assert.NotNull(defaults.ChangeLinkVisibilityValidator);
        Assert.NotNull(defaults.DeleteLinkValidator);

        Assert.True(defaults.CreateLinkValidator.Validate(new LinkCreateRequest(1, "https://example.com")).IsSuccess);
        Assert.True(defaults.EditLinkValidator.Validate(new LinkEditRequest(1, 2, "Title")).IsSuccess);
        Assert.True(defaults.MoveLinkValidator.Validate(new LinkMoveRequest(1, 2)).IsSuccess);
        Assert.True(defaults.ChangeLinkVisibilityValidator.Validate(new LinkChangeVisibilityRequest(1, true)).IsSuccess);
        Assert.True(defaults.DeleteLinkValidator.Validate(new LinkDeleteRequest(1)).IsSuccess);

        LinkCreateValidator createValidator = new();
        LinkEditValidator editValidator = new();
        LinkMoveValidator moveValidator = new();
        LinkChangeVisibilityValidator changeVisibilityValidator = new();
        LinkDeleteValidator deleteValidator = new();

        LinkServiceValidators custom = new(
            createValidator,
            editValidator,
            moveValidator,
            changeVisibilityValidator,
            deleteValidator);

        Assert.Same(createValidator, custom.CreateLinkValidator);
        Assert.Same(editValidator, custom.EditLinkValidator);
        Assert.Same(moveValidator, custom.MoveLinkValidator);
        Assert.Same(changeVisibilityValidator, custom.ChangeLinkVisibilityValidator);
        Assert.Same(deleteValidator, custom.DeleteLinkValidator);
    }
}
