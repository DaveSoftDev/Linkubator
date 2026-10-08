using DomainExceptions = Linkubator.Domain.Exceptions;

namespace Linkubator.Tests.Domain.Exceptions;

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
}