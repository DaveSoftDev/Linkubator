using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class SlugPolicyTests
{
    [Theory]
    [InlineData("Bandeja de entrada", "bandeja-de-entrada")]
    [InlineData("Q & A", "q-and-a")]
    [InlineData("C#", "c-sharp")]
    [InlineData("C++", "c-plus-plus")]
    [InlineData("Q&A", "q-and-a")]
    [InlineData("AT&T", "at-and-t")]
    [InlineData("Windows 1.0", "windows-1-0")]
    [InlineData("Node.js", "node-js")]
    [InlineData("TCP/IP", "tcp-ip")]
    [InlineData("Straße", "strasse")]
    [InlineData("Papá", "papa")]
    [InlineData("Saltó la raña al charço!!! I luego, croo", "salto-la-rana-al-charco-i-luego-croo")]
    [InlineData("a", "a")]
    public void GenerateProducesTheSlugOfTheSource(string name, string expected)
    {
        Assert.Equal(expected, DomainPolicies.SlugPolicy.Generate(name));
    }

    [Theory]
    [InlineData("Papá", "papa")]
    [InlineData("PAPA", "papa")]
    [InlineData("pápá", "papa")]
    [InlineData("Papa ", "papa")]
    public void GenerateProducesTheSameSlugForEquivalentNamesWithoutAffixes(string name, string expected)
    {
        Assert.Equal(expected, DomainPolicies.SlugPolicy.Generate(name));
    }

    [Fact]
    public void GenerateProducesTheSameSlugForTheSameNameUsedAsCollectionOrTag()
    {
        string asCollection = DomainPolicies.SlugPolicy.Generate("Node.js");
        string asTag = DomainPolicies.SlugPolicy.Generate("Node.js");

        Assert.Equal("node-js", asCollection);
        Assert.Equal(asCollection, asTag);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    [InlineData("😀")]
    [InlineData("--")]
    public void GenerateRejectsNamesWhoseSlugIsEmpty(string name)
    {
        Assert.Throws<DomainExceptions.TextBelowMinimumLengthException>(() => DomainPolicies.SlugPolicy.Generate(name));
    }

    [Fact]
    public void GenerateAcceptsTheMaximumLengthAndRejectsOneMoreWithoutTruncating()
    {
        Assert.Equal(new string('a', 150), DomainPolicies.SlugPolicy.Generate(new string('a', 150)));
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.SlugPolicy.Generate(new string('a', 151)));
    }

    [Fact]
    public void GenerateMeasuresTheLengthAfterTheSymbolsGrow()
    {
        string withinLimit = new('+', 30);
        string aboveLimit = new('+', 31);

        Assert.Equal(149, DomainPolicies.SlugPolicy.Generate(withinLimit).Length);
        Assert.Throws<DomainExceptions.TextExceedsMaximumLengthException>(() => DomainPolicies.SlugPolicy.Generate(aboveLimit));
    }

    [Theory]
    [InlineData("日本語")]
    [InlineData("日本語 Tokyo")]
    [InlineData("Tokyo 日本語")]
    [InlineData("Ana\n")]
    [InlineData("Ana\tLópez")]
    public void GenerateRejectsUnsupportedCharacters(string name)
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.SlugPolicy.Generate(name));
    }

    [Theory]
    [InlineData("ana", "ana")]
    [InlineData("linkubator", "linkubator")]
    public void GenerateDoesNotApplyTheRulesOfTheAlias(string name, string expected)
    {
        Assert.Equal(expected, DomainPolicies.SlugPolicy.Generate(name));
        Assert.ThrowsAny<DomainExceptions.DomainException>(() => DomainPolicies.AliasPolicy.Generate(name));
    }

    [Fact]
    public void GenerateExposesASingleEntryPointThatOnlyReceivesTheName()
    {
        System.Reflection.MethodInfo[] methods = typeof(DomainPolicies.SlugPolicy).GetMethods(
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.DeclaredOnly);

        System.Reflection.MethodInfo method = Assert.Single(methods);
        Assert.Equal(nameof(DomainPolicies.SlugPolicy.Generate), method.Name);
        Assert.Equal(new[] { typeof(string) }, method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(typeof(string), method.ReturnType);
    }

    [Fact]
    public void GenerateRejectsANullName()
    {
        Assert.Throws<ArgumentNullException>(() => DomainPolicies.SlugPolicy.Generate(null!));
    }
}
