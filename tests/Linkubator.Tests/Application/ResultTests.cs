using Linkubator.Application.Contracts;
using Linkubator.Application.Dtos.Requests;
using Linkubator.Application.Enums;
using Linkubator.Application.Results;
using Linkubator.Application.Validation;
using Linkubator.Domain.Exceptions;

namespace Linkubator.Tests.Application;

public class ResultTests
{
    [Fact]
    public void ResultSuccessAndFailureAreExplicitlyDistinguishable()
    {
        Result success = Result.Success();
        Result failure = Result.Failure(ApplicationError.Validation("EMPTY_NAME", "name", "El nombre no puede estar vacío."));

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
        Result<int> failure = Result<int>.Failure(ApplicationError.FromDomainException(new NotValidInputException(), "title"));

        Assert.True(success.IsSuccess);
        Assert.Equal(42, success.Value);
        Assert.False(failure.IsSuccess);
        Assert.Equal(ApplicationErrorCategory.DomainRule, failure.Errors[0].Category);
        Assert.Equal("NotValidInput", failure.Errors[0].Code);
        Assert.Equal("title", failure.Errors[0].Field);
    }

    [Fact]
    public void GenericResultTryGetValueAndFailureEnumerable_ReflectState()
    {
        Result<int> success = Result<int>.Success(7);
        Assert.True(success.TryGetValue(out int value));
        Assert.Equal(7, value);

        Result<int> failure = Result<int>.Failure(ApplicationError.Validation("INVALID", "value", "No válido."));

        Assert.False(failure.TryGetValue(out int failureValue));
        Assert.Equal(0, failureValue);
        Assert.False(failure.IsSuccess);
        Assert.Single(failure.Errors);
        Assert.Equal("INVALID", failure.Errors[0].Code);
    }

    [Fact]
    public void GenericResultFailureOverloads_FilterNullsAndRetainErrors()
    {
        Result<int> paramsFailure = Result<int>.Failure(
            ApplicationError.Validation("INVALID_1", "value", "Primero."),
            ApplicationError.Validation("INVALID_2", "value", "Segundo."));

        Assert.False(paramsFailure.IsSuccess);
        Assert.Equal(2, paramsFailure.Errors.Count);
        Assert.Equal(default(int), paramsFailure.Value);

        Result<int> enumerableFailure = Result<int>.Failure(ApplicationError.Validation("DOMAIN_ERROR", "title", "Error de dominio."));


        Assert.False(enumerableFailure.IsSuccess);
        Assert.Single(enumerableFailure.Errors);
        Assert.Equal("DOMAIN_ERROR", enumerableFailure.Errors[0].Code);

        Result<int> filteredFailure = Result<int>.Failure(ApplicationError.Validation("ONLY_VALID", "name", "Mensaje."));

        Assert.False(filteredFailure.IsSuccess);
        Assert.Single(filteredFailure.Errors);
        Assert.Equal("ONLY_VALID", filteredFailure.Errors[0].Code);
        Assert.Equal(default(int), filteredFailure.Value);

        Result<int> emptyFailure = Result<int>.Failure(Array.Empty<ApplicationError>());
        Assert.False(emptyFailure.IsSuccess);
        Assert.Empty(emptyFailure.Errors);
    }

    [Fact]
    public void ApplicationErrorFactoriesAndConstructor_ProduceExpectedMetadata()
    {
        ApplicationError validationError = ApplicationError.Validation("EMPTY_NAME", "name", "El nombre no puede estar vacío.");
        Assert.Equal("EMPTY_NAME", validationError.Code);
        Assert.Equal(ApplicationErrorCategory.Validation, validationError.Category);
        Assert.Equal("name", validationError.Field);
        Assert.Equal("El nombre no puede estar vacío.", validationError.Message);

        ApplicationError domainError = ApplicationError.Domain("DOMAIN_ERROR", "title", "Error de dominio.");
        Assert.Equal("DOMAIN_ERROR", domainError.Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, domainError.Category);
        Assert.Equal("title", domainError.Field);
        Assert.Equal("Error de dominio.", domainError.Message);

        ApplicationError inaccessibleError = ApplicationError.ResourceNotAccessible("NOT_ALLOWED", "CollectionId", "No accesible.");
        Assert.Equal("NOT_ALLOWED", inaccessibleError.Code);
        Assert.Equal(ApplicationErrorCategory.ResourceNotAccessible, inaccessibleError.Category);
        Assert.Equal("CollectionId", inaccessibleError.Field);
        Assert.Equal("No accesible.", inaccessibleError.Message);

        ApplicationError fromException = ApplicationError.FromDomainException(new NotValidInputException(), "slug");
        Assert.Equal("NotValidInput", fromException.Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, fromException.Category);
        Assert.Equal("slug", fromException.Field);
        Assert.Null(fromException.Message);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new ApplicationError(" ", ApplicationErrorCategory.Validation));
        Assert.Equal("code", exception.ParamName);

        Assert.Throws<ArgumentNullException>(() => ApplicationError.FromDomainException(null!));
    }

    [Fact]
    public void ValidatorsRejectInvalidRequests()
    {
        CreateCollectionRequest badCollection = new CreateCollectionRequest(" ", "123456...", false);
        CollectionCreateValidator collectionValidator = new CollectionCreateValidator();

        Result collectionResult = collectionValidator.Validate(badCollection);

        Assert.False(collectionResult.IsSuccess);
        Assert.Contains(collectionResult.Errors, error => error.Field == "Name");

        CreateLinkRequest badLink = new CreateLinkRequest(0, " ", "titulo", "descripcion", "https://example.com/image.jpg", true);
        LinkCreateValidator linkValidator = new LinkCreateValidator();

        Result linkResult = linkValidator.Validate(badLink);

        Assert.False(linkResult.IsSuccess);
        Assert.Contains(linkResult.Errors, error => error.Field == "CollectionId" || error.Field == "Url");
    }
}
