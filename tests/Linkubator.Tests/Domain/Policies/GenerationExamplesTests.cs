using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class GenerationExamplesTests
{
    [Fact]
    public void CommonAsciiExamplesMatchTheSource()
    {
        Dictionary<string, string> examples = new Dictionary<string, string>
        {
            ["pingüino-2024"] = "pinguino-2024",
            ["ana--lopez"] = "ana-lopez",
            ["Anna_López!"] = "anna-lopez",
            ["Ana López García"] = "ana-lopez-garcia",
            ["Generación"] = "generacion",
            ["caça"] = "caca",
            ["España"] = "espana",
            ["  Ana   López  "] = "ana-lopez",
            ["---a---"] = "a",
            ["a - b"] = "a-b",
            ["Ana😀López"] = "analopez",
            ["Ana López (2024), ¿qué tal? ¡hola!"] = "ana-lopez-2024-que-tal-hola",
            ["Saltó la raña al charço!!! I luego, croo"] = "salto-la-rana-al-charco-i-luego-croo",
            ["Ana°López©"] = "analopez",
            ["!!!"] = "",
            [""] = "",
            ["   "] = "",
            ["Media ½ hora"] = "media-1-2-hora",
            ["Temp 20℃"] = "temp-20c",
            ["日本語-ana"] = "rechazado"
        };

        foreach ((string? input, string? expected) in examples)
        {
            if (expected == "rechazado")
            {
                Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AsciiTransformationPolicy.Transform(input));
                continue;
            }

            Assert.Equal(expected, DomainPolicies.AsciiTransformationPolicy.Transform(input));
        }
    }

    [Fact]
    public void AliasExamplesMatchTheSourceAndRejectReservedValues()
    {
        Assert.Equal("anna-lopez", DomainPolicies.AliasPolicy.Generate("Anna_López!"));

        foreach (string? candidate in new[] { "linkubator", "administrator", "administrador", "LINKUBATOR", " Linkubator ", "Linkúbator" })
        {
            Assert.Throws<DomainExceptions.AliasReservedException>(() => DomainPolicies.AliasPolicy.Generate(candidate));
        }

        Assert.Equal("anna-lopez", DomainPolicies.AliasPolicy.Generate("  Anna_López!  "));
        Assert.Throws<DomainExceptions.TextBelowMinimumLengthException>(() => DomainPolicies.AliasPolicy.Generate("short"));
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AliasPolicy.Generate("日本語-ana-lopez"));
    }

    [Fact]
    public void SlugExamplesMatchTheSourceAndEquivalentNamesProduceTheSameSlug()
    {
        Assert.Equal("bandeja-de-entrada", DomainPolicies.SlugPolicy.Generate("Bandeja de entrada"));
        Assert.Equal("q-and-a", DomainPolicies.SlugPolicy.Generate("Q & A"));
        Assert.Equal("c-sharp", DomainPolicies.SlugPolicy.Generate("C#"));
        Assert.Equal("tcp-ip", DomainPolicies.SlugPolicy.Generate("TCP/IP"));
        Assert.Equal("strasse", DomainPolicies.SlugPolicy.Generate("Straße"));
        Assert.Equal("papa", DomainPolicies.SlugPolicy.Generate("Papá"));
        Assert.Equal("salto-la-rana-al-charco-i-luego-croo", DomainPolicies.SlugPolicy.Generate("Saltó la raña al charço!!! I luego, croo"));

        Assert.Equal(DomainPolicies.SlugPolicy.Generate("Papá"), DomainPolicies.SlugPolicy.Generate("PAPA"));
        Assert.Equal(DomainPolicies.SlugPolicy.Generate("Node.js"), DomainPolicies.SlugPolicy.Generate("Node.js"));
        Assert.Throws<DomainExceptions.TextBelowMinimumLengthException>(() => DomainPolicies.SlugPolicy.Generate("!!!"));
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.SlugPolicy.Generate("日本語 Tokyo"));
    }
}
