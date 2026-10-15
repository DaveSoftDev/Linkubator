using Linkubator.Application;
using Linkubator.Domain.Exceptions;

namespace Linkubator.Tests.Application;

public class ResultTests
{
    [Fact]
    public void ResultSuccessAndFailureAreExplicitlyDistinguishable()
    {
        Result success = Result.Success();
        Result failure = Result.Failure(
            ApplicationError.Validation("EMPTY_NAME", "name", "El nombre no puede estar vacío."));

        Assert.True(success.IsSuccess);
        Assert.Empty(success.Errors);
        Assert.False(failure.IsSuccess);
        Assert.Single(failure.Errors);
        Assert.Equal(ApplicationErrorCategory.Validation, failure.Errors[0].Category);
        Assert.Equal("EMPTY_NAME", failure.Errors[0].Code);
        Assert.Equal("name", failure.Errors[0].Field);
    }

    [Fact]
    public void GenericResultCarriesValueAndDomainErrors()
    {
        Result<int> success = Result<int>.Success(42);
        Result<int> failure = Result<int>.Failure(
            ApplicationError.FromDomainException(new NotValidInputException(), "title"));

        Assert.True(success.IsSuccess);
        Assert.Equal(42, success.Value);
        Assert.False(failure.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, failure.Errors[0].Category);
        Assert.Equal("NotValidInput", failure.Errors[0].Code);
        Assert.Equal("title", failure.Errors[0].Field);
    }

    [Fact]
    public void ValidatorsRejectInvalidRequests()
    {
        var badCollection = new CreateCollectionRequest(" ", "123456...", false);
        var collectionValidator = new CollectionCreateValidator();

        Result collectionResult = collectionValidator.Validate(badCollection);

        Assert.False(collectionResult.IsSuccess);
        Assert.Contains(collectionResult.Errors, error => error.Field == "Name");

        var badLink = new CreateLinkRequest(0, " ", "titulo", "descripcion", "https://example.com/image.jpg", true);
        var linkValidator = new LinkCreateValidator();

        Result linkResult = linkValidator.Validate(badLink);

        Assert.False(linkResult.IsSuccess);
        Assert.Contains(linkResult.Errors, error => error.Field == "CollectionId" || error.Field == "Url");
    }
}
