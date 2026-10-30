using Linkubator.Application.Enums;
using Linkubator.Application.Results;
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

        IEnumerable<ApplicationError> errors = new[]
        {
            ApplicationError.Validation("FIRST", "title", "Primero."),
            null!,
            ApplicationError.Validation("SECOND", "title", "Segundo."),
        };
        Result<int> enumerableFailure = Result<int>.Failure(errors);


        Assert.False(enumerableFailure.IsSuccess);
        Assert.Equal(new[] { "FIRST", "SECOND" }, enumerableFailure.Errors.Select(error => error.Code));

        Result<int> filteredFailure = Result<int>.Failure(null!, ApplicationError.Validation("ONLY_VALID", "name", "Mensaje."), null!);

        Assert.False(filteredFailure.IsSuccess);
        Assert.Single(filteredFailure.Errors);
        Assert.Equal("ONLY_VALID", filteredFailure.Errors[0].Code);
        Assert.Equal(default(int), filteredFailure.Value);
    }

    [Fact]
    public void Failure_WhenNoValidErrorsAreProvided_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure());
        Assert.Throws<ArgumentException>(() => Result.Failure(Array.Empty<ApplicationError>()));
        Assert.Throws<ArgumentException>(() => Result.Failure((IEnumerable<ApplicationError>)null!));
        Assert.Throws<ArgumentException>(() => Result.Failure(null!, null!));
        Assert.Throws<ArgumentException>(() => Result<int>.Failure());
        Assert.Throws<ArgumentException>(() => Result<int>.Failure(Array.Empty<ApplicationError>()));
        Assert.Throws<ArgumentException>(() => Result<int>.Failure((IEnumerable<ApplicationError>)null!));
        Assert.Throws<ArgumentException>(() => Result<int>.Failure(null!, null!));
    }

    [Fact]
    public void Success_WhenValueIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string?>.Success(null));
        Assert.Throws<ArgumentNullException>(() => Result<int?>.Success(null));
    }

    [Fact]
    public void Failure_WhenSourceArrayIsModified_ErrorsRemainUnchanged()
    {
        ApplicationError first = ApplicationError.Validation("FIRST");
        ApplicationError second = ApplicationError.Validation("SECOND");
        ApplicationError[] errors = [first];
        Result result = Result.Failure(errors);

        errors[0] = second;

        Assert.Same(first, Assert.Single(result.Errors));
        Assert.Throws<NotSupportedException>(() => ((IList<ApplicationError>)result.Errors)[0] = second);
    }

    [Fact]
    public void Constructor_WhenSuccessfulResultContainsErrors_Throws()
    {
        ApplicationError error = ApplicationError.Validation("INVALID");

        Assert.Throws<ArgumentException>(() => new DerivedResult(true, [error]));
    }

    private sealed class DerivedResult(bool isSuccess, IEnumerable<ApplicationError> errors)
        : Result(isSuccess, errors);

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
        Assert.Equal("NotValidInput", fromException.Message);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new ApplicationError(" ", ApplicationErrorCategory.Validation));
        Assert.Equal("code", exception.ParamName);

        Assert.Throws<ArgumentNullException>(() => ApplicationError.FromDomainException(null!));
    }

    [Fact]
    public void ApplicationError_FromDomainException_PreservesMessageWhenFieldIsNotProvided()
    {
        ApplicationError error = ApplicationError.FromDomainException(new NotValidInputException());

        Assert.Equal("NotValidInput", error.Code);
        Assert.Equal(ApplicationErrorCategory.DomainRule, error.Category);
        Assert.Null(error.Field);
        Assert.Equal("NotValidInput", error.Message);
    }

}
