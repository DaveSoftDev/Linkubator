using System.Globalization;
using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class AsciiTransformationPolicyTests
{
    [Theory]
    [InlineData("pingüino-2024", "pinguino-2024")]
    [InlineData("ana--lopez", "ana-lopez")]
    [InlineData("Ana_López!", "ana-lopez")]
    [InlineData("Ana López García", "ana-lopez-garcia")]
    [InlineData("Generación", "generacion")]
    [InlineData("caça", "caca")]
    [InlineData("España", "espana")]
    [InlineData("  Ana   López  ", "ana-lopez")]
    [InlineData("---a---", "a")]
    [InlineData("a - b", "a-b")]
    [InlineData("Ana\uD83D\uDE00López", "analopez")]
    [InlineData("Ana López (2024), ¿qué tal? ¡hola!", "ana-lopez-2024-que-tal-hola")]
    [InlineData("Saltó la raña al charço!!! I luego, croo", "salto-la-rana-al-charco-i-luego-croo")]
    [InlineData("Ana°López©", "analopez")]
    [InlineData("!!!", "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void TransformAppliesTheCommonStepsOfTheSource(string text, string expected)
    {
        Assert.Equal(expected, DomainPolicies.AsciiTransformationPolicy.Transform(text));
    }

    [Theory]
    [InlineData("\uFF21na\uFF11", "ana1")]
    [InlineData("\uFF21\uFF22\uFF23 \uFF11\uFF12\uFF13", "abc-123")]
    [InlineData("Área x²", "area-x2")]
    [InlineData("H\u2082O", "h2o")]
    [InlineData("Media \u00BD hora", "media-1-2-hora")]
    [InlineData("\u00BD", "1-2")]
    [InlineData("1\u00BD", "11-2")]
    [InlineData("Temp 20\u2103", "temp-20c")]
    [InlineData("Temp 70\u2109", "temp-70f")]
    [InlineData("\u212Aelvin", "kelvin")]
    [InlineData("A\u2122", "atm")]
    [InlineData("\u2460\u2461", "12")]
    [InlineData("\u2167", "viii")]
    [InlineData("a\u00A0b", "a-b")]
    [InlineData("a\u3000b", "a-b")]
    [InlineData("\uFF06", "and")]
    [InlineData("\uFB01n", "fin")]
    public void TransformReducesCompatibilityCharactersToAscii(string text, string expected)
    {
        Assert.Equal(expected, DomainPolicies.AsciiTransformationPolicy.Transform(text));
    }

    [Theory]
    [InlineData("C#", "c-sharp")]
    [InlineData("C++", "c-plus-plus")]
    [InlineData("Q&A", "q-and-a")]
    [InlineData("Q & A", "q-and-a")]
    [InlineData("AT&T", "at-and-t")]
    [InlineData("Windows 1.0", "windows-1-0")]
    [InlineData("Node.js", "node-js")]
    [InlineData("TCP/IP", "tcp-ip")]
    [InlineData("a_b", "a-b")]
    [InlineData("a@b", "a-at-b")]
    [InlineData("50%", "50-percent")]
    [InlineData("$5", "dollar-5")]
    [InlineData("€5", "euro-5")]
    [InlineData("£5", "pound-5")]
    [InlineData("Straße", "strasse")]
    [InlineData("Papá", "papa")]
    public void TransformAppliesTheSymbolTable(string text, string expected)
    {
        Assert.Equal(expected, DomainPolicies.AsciiTransformationPolicy.Transform(text));
    }

    [Theory]
    [InlineData("ß", "ss")]
    [InlineData("\u1E9E", "ss")]
    [InlineData("æ", "ae")]
    [InlineData("Æ", "ae")]
    [InlineData("œ", "oe")]
    [InlineData("Œ", "oe")]
    [InlineData("ø", "o")]
    [InlineData("Ø", "o")]
    [InlineData("đ", "d")]
    [InlineData("Đ", "d")]
    [InlineData("ł", "l")]
    [InlineData("Ł", "l")]
    [InlineData("ð", "d")]
    [InlineData("Ð", "d")]
    [InlineData("þ", "th")]
    [InlineData("Þór", "thor")]
    [InlineData("ħ", "h")]
    [InlineData("Ħ", "h")]
    [InlineData("ı", "i")]
    [InlineData("\u0130", "i")]
    [InlineData("ĸ", "k")]
    [InlineData("ŋ", "ng")]
    [InlineData("Ŋ", "ng")]
    [InlineData("ŧ", "t")]
    [InlineData("Ŧ", "t")]
    [InlineData("\u00AA", "a")]
    [InlineData("\u00BA", "o")]
    [InlineData("\u0133", "ij")]
    [InlineData("\u0140", "l")]
    [InlineData("\u017F", "s")]
    [InlineData("\u00FF", "y")]
    public void TransformConvertsTheLatinLettersOfLatin1AndLatinExtendedA(string text, string expected)
    {
        Assert.Equal(expected, DomainPolicies.AsciiTransformationPolicy.Transform(text));
    }

    [Theory]
    [InlineData("Ana\n")]
    [InlineData("\nAna")]
    [InlineData("Ana\r\nLópez")]
    [InlineData("Ana\tLópez")]
    [InlineData("\tAna")]
    [InlineData("Ana\u0007López")]
    [InlineData("Ana\u0085López")]
    [InlineData("Ana\u2028López")]
    [InlineData("Ana\u2029López")]
    public void TransformRejectsControlsAndSeparatorsOtherThanTheNormalSpace(string text)
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AsciiTransformationPolicy.Transform(text));
    }

    [Theory]
    [InlineData("日本語")]
    [InlineData("日本語 Tokyo")]
    [InlineData("Tokyo 日本語")]
    [InlineData("日本語-ana")]
    [InlineData("Привет")]
    [InlineData("\u03B1")]
    [InlineData("\u0641")]
    [InlineData("\u0663")]
    [InlineData("\u0BE7")]
    [InlineData("\uD840\uDC00")]
    [InlineData("\u00B5")]
    [InlineData("\u0149")]
    [InlineData("\u00D7\u00C0\u0414")]
    public void TransformRejectsLettersAndNumbersThatAreNotAscii(string text)
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AsciiTransformationPolicy.Transform(text));
    }

    [Fact]
    public void TransformRejectsTextThatIsNotValidUnicode()
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AsciiTransformationPolicy.Transform("ana\uD800lopez"));
    }

    [Fact]
    public void TransformDoesNotLeakTheInputInTheRejection()
    {
        DomainExceptions.TextContainsUnsupportedCharactersException exception =
            Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AsciiTransformationPolicy.Transform("日本語 secreto"));

        Assert.DoesNotContain("secreto", exception.Message);
        Assert.DoesNotContain("日本語", exception.Message);
    }

    [Fact]
    public void TransformDoesNotTrimBeforeApplyingTheRules()
    {
        Assert.Throws<DomainExceptions.TextContainsUnsupportedCharactersException>(() => DomainPolicies.AsciiTransformationPolicy.Transform("ana\n"));
        Assert.Equal("ana", DomainPolicies.AsciiTransformationPolicy.Transform(" ana "));
    }

    [Fact]
    public void TransformIsDeterministic()
    {
        string first = DomainPolicies.AsciiTransformationPolicy.Transform("Ana López & Çaça ½");
        string second = DomainPolicies.AsciiTransformationPolicy.Transform("Ana López & Çaça ½");

        Assert.Equal("ana-lopez-and-caca-1-2", first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void TransformDoesNotDependOnTheCurrentCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            Assert.Equal("iii", DomainPolicies.AsciiTransformationPolicy.Transform("IİI"));
            Assert.Equal("ana-lopez", DomainPolicies.AsciiTransformationPolicy.Transform("ANA LOPEZ"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void TransformExposesASingleEntryPointWithoutAnOriginParameter()
    {
        System.Reflection.MethodInfo[] methods = typeof(DomainPolicies.AsciiTransformationPolicy).GetMethods(
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.DeclaredOnly);

        System.Reflection.MethodInfo method = Assert.Single(methods);
        Assert.Equal(nameof(DomainPolicies.AsciiTransformationPolicy.Transform), method.Name);
        Assert.Equal(new[] { typeof(string) }, method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(typeof(string), method.ReturnType);
    }

    [Fact]
    public void TransformRejectsANullText()
    {
        Assert.Throws<ArgumentNullException>(() => DomainPolicies.AsciiTransformationPolicy.Transform(null!));
    }
}
