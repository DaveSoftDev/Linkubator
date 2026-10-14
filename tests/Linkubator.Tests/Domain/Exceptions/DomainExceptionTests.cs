using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Tests.Domain.Exceptions;

[Trait("Category", "Exceptions")]
public class DomainExceptionTests
{
    [Fact]
    public void BaseTypeIsAbstractAndDerivesFromException()
    {
        Assert.True(typeof(DomainExceptions.DomainException).IsAbstract);
        Assert.True(typeof(Exception).IsAssignableFrom(typeof(DomainExceptions.DomainException)));
    }

    [Fact]
    public void BaseConstructorPreservesCodeForDerivedExceptions()
    {
        DomainExceptions.DomainException exception = new TestDomainException("TestCode");

        Assert.Equal("TestCode", exception.Code);
    }

    [Fact]
    public void IsDistinguishableByTypeWithoutInspectingMessage()
    {
        Exception exception = new DomainExceptions.TextExceedsMaximumLengthException();

        Assert.IsType<DomainExceptions.TextExceedsMaximumLengthException>(exception);
        Assert.IsAssignableFrom<DomainExceptions.DomainException>(exception);
    }

    [Fact]
    public void SpecificExceptionExposesItsStableCode()
    {
        DomainExceptions.TextExceedsMaximumLengthException exception = new();

        Assert.Equal("TextExceedsMaximumLength", exception.Code);
    }

    [Fact]
    public void MinimumLengthExceptionExposesItsStableCode()
    {
        DomainExceptions.TextBelowMinimumLengthException exception = new();

        Assert.Equal("TextBelowMinimumLength", exception.Code);
    }

    [Fact]
    public void ReservedAliasExceptionExposesItsStableCode()
    {
        DomainExceptions.AliasReservedException exception = new();

        Assert.Equal("AliasReserved", exception.Code);
    }

    [Fact]
    public void UnsupportedCharactersExceptionExposesItsStableCode()
    {
        DomainExceptions.TextContainsUnsupportedCharactersException exception = new();

        Assert.Equal("TextContainsUnsupportedCharacters", exception.Code);
        Assert.IsAssignableFrom<DomainExceptions.DomainException>(exception);
    }

    [Fact]
    public void NotValidInputExceptionExposesItsStableCode()
    {
        DomainExceptions.NotValidInputException exception = new();

        Assert.Equal("NotValidInput", exception.Code);
        Assert.IsAssignableFrom<DomainExceptions.DomainException>(exception);
    }

    [Fact]
    public void InvalidEmailExceptionExposesItsStableCode()
    {
        DomainExceptions.InvalidEmailException exception = new();

        Assert.Equal("InvalidEmail", exception.Code);
        Assert.IsAssignableFrom<DomainExceptions.DomainException>(exception);
    }

    [Fact]
    public void UnsupportedCharactersExceptionDoesNotAcceptOrExposeUserData()
    {
        System.Reflection.ConstructorInfo[] constructors = typeof(DomainExceptions.TextContainsUnsupportedCharactersException).GetConstructors();
        System.Reflection.PropertyInfo[] properties = typeof(DomainExceptions.TextContainsUnsupportedCharactersException).GetProperties(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.DeclaredOnly);

        Assert.True(typeof(DomainExceptions.TextContainsUnsupportedCharactersException).IsSealed);
        Assert.Collection(constructors, constructor => Assert.Empty(constructor.GetParameters()));
        Assert.Empty(properties);
    }

    [Fact]
    public void ContractDoesNotAcceptOrExposeUserData()
    {
        System.Reflection.ConstructorInfo[] baseConstructors = typeof(DomainExceptions.DomainException).GetConstructors(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        System.Reflection.ConstructorInfo[] specificConstructors = typeof(DomainExceptions.TextExceedsMaximumLengthException).GetConstructors();
        System.Reflection.PropertyInfo[] properties = typeof(DomainExceptions.DomainException).GetProperties(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.DeclaredOnly);

        Assert.Collection(
            baseConstructors,
            constructor => Assert.Equal(new[] { typeof(string) }, constructor.GetParameters().Select(parameter => parameter.ParameterType)));
        Assert.Collection(specificConstructors, constructor => Assert.Empty(constructor.GetParameters()));
        Assert.Collection(properties, property => Assert.Equal(nameof(DomainExceptions.DomainException.Code), property.Name));
    }

    private sealed class TestDomainException : DomainExceptions.DomainException
    {
        public TestDomainException(string code)
            : base(code)
        {
        }
    }

    [Fact]
    public void EveryDomainExceptionExposesACodeMatchingItsTypeNameWithoutTheExceptionSuffix()
    {
        System.Reflection.Assembly assembly = typeof(DomainExceptions.DomainException).Assembly;
        System.Type[] exceptionTypes = assembly.GetTypes()
            .Where(type => typeof(DomainExceptions.DomainException).IsAssignableFrom(type))
            .Where(type => type != typeof(DomainExceptions.DomainException))
            .ToArray();

        Assert.NotEmpty(exceptionTypes);

        foreach (System.Type type in exceptionTypes)
        {
            DomainExceptions.DomainException exception = (DomainExceptions.DomainException)System.Activator.CreateInstance(type)!;
            string expectedCode = type.Name.EndsWith("Exception", StringComparison.Ordinal)
                ? type.Name[..^"Exception".Length]
                : type.Name;

            Assert.Equal(expectedCode, exception.Code);
        }
    }
}