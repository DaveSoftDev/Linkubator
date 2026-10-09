using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class AliasPolicyTests
{
    [Theory]
    [InlineData("pingüino-2024", "pinguino-2024")]
    [InlineData("ana--lopez-123", "ana-lopez-123")]
    [InlineData("Ana_López!123", "ana-lopez123")]
    [InlineData("Ana López García 23", "ana-lopez-garcia-23")]
    [InlineData("  Ana   López   García  2026  ", "ana-lopez-garcia-2026")]
    [InlineData("Ana\u00A0López García", "ana-lopez-garcia")]
    [InlineData("\uFF21na L\u00F3pez \uFF11\uFF12\uFF13", "ana-lopez-123")]
    [InlineData("Ana López 😀 García", "ana-lopez-garcia")]
    public void GenerateTransformsTheInputToTheCanonicalAlias(string input, string expected)
    {
        Assert.Equal(expected, DomainPolicies.AliasPolicy.Generate(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    [InlineData("short")]
    [InlineData("aaaaaaaaa")]
    public void GenerateRejectsAliasesBelowMinimumLength(string candidate)
    {
        Assert.Throws<DomainExceptions.TextBelowMinimumLengthException>(() => DomainPolicies.AliasPolicy.Generate(candidate));
    }

    [Fact]
    public void GenerateAcceptsTheMinimumAndMaximumLengths()
    {
        Assert.Equal(new string('a', 10), DomainPolicies.AliasPolicy.Generate(new string('a', 10)));
        Assert.Equal(new string('a', 50), DomainPolicies.AliasPolicy.Generate(new string('a', 50)));
    }

    [Theory]
    [InlineData(51)]
    [InlineData(69)]
    public void GenerateRejectsAliasesAboveMaximumLengthWithoutTruncating(int length)
    {
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.AliasPolicy.Generate(new string('a', length)));
    }

    [Theory]
    [InlineData("linkubator")]
    [InlineData("administrator")]
    [InlineData("administrador")]
    [InlineData("LINKUBATOR")]
    [InlineData(" Linkubator ")]
    [InlineData("Linkúbator")]
    public void GenerateRejectsReservedAliases(string candidate)
    {
        Assert.Throws<DomainExceptions.AliasReservedException>(() => DomainPolicies.AliasPolicy.Generate(candidate));
    }

    [Theory]
    [InlineData("linkubator2")]
    [InlineData("mi-administrador")]
    public void GenerateAcceptsAliasesThatOnlyContainAReservedWord(string candidate)
    {
        Assert.NotNull(DomainPolicies.AliasPolicy.Generate(candidate));
    }

    [Theory]
    [InlineData("Ana López García\n")]
    [InlineData("\tAna López García")]
    [InlineData("Ana\u0007López García")]
    [InlineData("日本語-ana-lopez")]
    [InlineData("Ana López García ñ\u0141\u00B5")]
    public void GenerateRejectsUnsupportedCharactersWithoutRemovingThem(string candidate)
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AliasPolicy.Generate(candidate));
    }

    [Fact]
    public void GenerateDoesNotTrimTheCandidateBeforeTransforming()
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AliasPolicy.Generate("ana-lopez-garcia\r\n"));
        Assert.Equal("ana-lopez-garcia", DomainPolicies.AliasPolicy.Generate(" ana-lopez-garcia "));
    }

    [Fact]
    public void GenerateExposesASingleEntryPointThatOnlyReceivesTheText()
    {
        System.Reflection.MethodInfo[] methods = typeof(DomainPolicies.AliasPolicy).GetMethods(
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.DeclaredOnly);

        System.Reflection.MethodInfo method = Assert.Single(methods);
        Assert.Equal(nameof(DomainPolicies.AliasPolicy.Generate), method.Name);
        Assert.Equal(new[] { typeof(string) }, method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(typeof(string), method.ReturnType);
    }

    [Fact]
    public void GenerateDoesNotConsultTheAvailabilityOfTheAlias()
    {
        string first = DomainPolicies.AliasPolicy.Generate("Ana López García");
        string second = DomainPolicies.AliasPolicy.Generate("Ana López García");

        Assert.Equal("ana-lopez-garcia", first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void GenerateRejectsANullCandidate()
    {
        Assert.Throws<ArgumentNullException>(() => DomainPolicies.AliasPolicy.Generate(null!));
    }
}
