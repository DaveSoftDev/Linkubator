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
    public void LinkUpdateValidator_CoversNullInvalidAndValidCases()
    {
        LinkUpdateValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidLinkId = validator.Validate(new LinkUpdateRequest(0, 1, "Title"));
        Assert.False(invalidLinkId.IsSuccess);
        Assert.Equal("LINK_ID_INVALID", invalidLinkId.Errors[0].Code);

        Result invalidCollectionId = validator.Validate(new LinkUpdateRequest(1, 0, "Title"));
        Assert.False(invalidCollectionId.IsSuccess);
        Assert.Equal("LINK_COLLECTION_ID_INVALID", invalidCollectionId.Errors[0].Code);

        Result valid = validator.Validate(new LinkUpdateRequest(1, 2, "Title"));
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
    public void LinkDeleteValidator_CoversNullInvalidAndValidCases()
    {
        LinkDeleteValidator deleteValidator = new();

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
    public void CollectionGetValidator_CoversNullInvalidAndValidCases()
    {
        CollectionGetValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidId = validator.Validate(new CollectionGetRequest(0));
        Assert.False(invalidId.IsSuccess);
        Assert.Equal("COLLECTION_ID_INVALID", invalidId.Errors[0].Code);

        Result valid = validator.Validate(new CollectionGetRequest(1));
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
        Assert.Null(defaults.GetCollection);
        Assert.NotNull(defaults.CollectionGetValidator);
        Assert.True(defaults.CollectionGetValidator.Validate(new CollectionGetRequest(1)).IsSuccess);
        Assert.Null(defaults.CreateCollection);
        Assert.Null(defaults.DeleteCollection);
        Assert.NotNull(defaults.CollectionCreateValidator);
        Assert.NotNull(defaults.CollectionDeleteValidator);

        Assert.True(defaults.CollectionCreateValidator.Validate(new CollectionCreateRequest("Name", "desc", true)).IsSuccess);
        Assert.True(defaults.CollectionDeleteValidator.Validate(new CollectionDeleteRequest(1)).IsSuccess);

        CollectionGetValidator getValidator = new();
        CollectionCreateValidator createValidator = new();
        CollectionUpdateValidator updateValidator = new();
        CollectionDeleteValidator deleteValidator = new();

        CollectionServiceValidators custom = new(
            getValidator,
            createValidator,
            updateValidator,
            deleteValidator);

        Assert.Same(getValidator, custom.CollectionGetValidator);
        Assert.Same(createValidator, custom.CollectionCreateValidator);
        Assert.Same(updateValidator, custom.CollectionUpdateValidator);
        Assert.Same(deleteValidator, custom.CollectionDeleteValidator);
    }

    [Fact]
    public void LinkGetValidator_CoversNullInvalidAndValidCases()
    {
        LinkGetValidator validator = new();

        Result nullResult = validator.Validate(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("REQUEST_REQUIRED", nullResult.Errors[0].Code);

        Result invalidId = validator.Validate(new LinkGetRequest(0));
        Assert.False(invalidId.IsSuccess);
        Assert.Equal("LINK_ID_INVALID", invalidId.Errors[0].Code);

        Result valid = validator.Validate(new LinkGetRequest(1));
        Assert.True(valid.IsSuccess);
    }

    [Fact]
    public void LinkServiceValidators_UseDefaultAndCustomInstances()
    {
        LinkServiceValidators defaults = new();
        Assert.Null(defaults.GetLink);
        Assert.NotNull(defaults.GetLinkValidator);
        Assert.True(defaults.GetLinkValidator.Validate(new LinkGetRequest(1)).IsSuccess);
        Assert.NotNull(defaults.CreateLinkValidator);
        Assert.NotNull(defaults.UpdateLinkValidator);
        Assert.NotNull(defaults.MoveLinkValidator);
        Assert.NotNull(defaults.DeleteLinkValidator);

        Assert.True(defaults.CreateLinkValidator.Validate(new LinkCreateRequest(1, "https://example.com")).IsSuccess);
        Assert.True(defaults.UpdateLinkValidator.Validate(new LinkUpdateRequest(1, 2, "Title")).IsSuccess);
        Assert.True(defaults.MoveLinkValidator.Validate(new LinkMoveRequest(1, 2)).IsSuccess);
        Assert.True(defaults.DeleteLinkValidator.Validate(new LinkDeleteRequest(1)).IsSuccess);

        LinkGetValidator getValidator = new();
        LinkCreateValidator createValidator = new();
        LinkUpdateValidator updateValidator = new();
        LinkMoveValidator moveValidator = new();
        LinkDeleteValidator deleteValidator = new();

        LinkServiceValidators custom = new(
            getValidator,
            createValidator,
            updateValidator,
            moveValidator,
            deleteValidator);

        Assert.Same(getValidator, custom.GetLinkValidator);
        Assert.Same(createValidator, custom.CreateLinkValidator);
        Assert.Same(updateValidator, custom.UpdateLinkValidator);
        Assert.Same(moveValidator, custom.MoveLinkValidator);
        Assert.Same(deleteValidator, custom.DeleteLinkValidator);
    }
}
