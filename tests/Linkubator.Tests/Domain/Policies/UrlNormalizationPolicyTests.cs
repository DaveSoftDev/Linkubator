using System.Globalization;
using DomainExceptions = Linkubator.Domain.Exceptions;
using Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "Normalization")]
public class UrlNormalizationPolicyTests
{
    [Theory]
    [InlineData("utm_source") ]
    [InlineData("utm_medium") ]
    [InlineData("utm_campaign") ]
    [InlineData("utm_term") ]
    [InlineData("utm_content") ]
    [InlineData("gclsrc") ]
    [InlineData("gclid") ]
    [InlineData("wbraid") ]
    [InlineData("gbraid") ]
    [InlineData("fbclid") ]
    [InlineData("msclkid") ]
    [InlineData("ttclid") ]
    [InlineData("epik") ]
    [InlineData("gad_source") ]
    [InlineData("gad_campaignid") ]
    [InlineData("srsltid") ]
    [InlineData("gcs") ]
    [InlineData("gcd") ]
    [InlineData("at_medium") ]
    [InlineData("at_campaign") ]
    [InlineData("at_platform") ]
    [InlineData("at_creation") ]
    [InlineData("at_term") ]
    public void Normalize_WithMarketingParameter_RemovesOnlyExactName(string parameter)
    {
        string input = $"https://example.com/path?{parameter.ToUpperInvariant()}=tracking&{parameter}x=keep&zzz=1";
        string expected = "examplecompath" + parameter.Replace("_", "") + "xkeepzzz1";

        Assert.Equal(expected, UrlNormalizationPolicy.Normalize(input));
    }

    [Trait("Category", "Validation")]
    [Fact]
    public void NormalizeRejectsNullEmptyAndWhiteSpace()
    {
        Assert.Throws<DomainExceptions.NotValidInputException>(() => UrlNormalizationPolicy.Normalize(null!));
        Assert.Throws<DomainExceptions.NotValidInputException>(() => UrlNormalizationPolicy.Normalize(string.Empty));
        Assert.Throws<DomainExceptions.NotValidInputException>(() => UrlNormalizationPolicy.Normalize("   "));
    }

    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("HTTPS://Example.COM/Generación de datos/?utm_source=google&id=10", "examplecomgeneracion-de-datosid10")]
    [InlineData("http://example.com:4587/Artículo número 2?b=2&a=1", "examplecom4587articulo-numero-2a1b2")]
    [InlineData("http://example.com:8080/x", "examplecom8080x")]
    [InlineData("https://example.com:8080/x", "examplecom8080x")]
    [InlineData("http://example.com:80/x", "examplecomx")]
    [InlineData("https://example.com:443/x", "examplecomx")]
    [InlineData("https://example.com/x", "examplecomx")]
    [InlineData("https://example.com/c++/tutorial?q=c++", "examplecomctutorialqc")]
    [InlineData("https://example.com/a%2520b", "examplecoma20b")]
    [InlineData("https://münchen.de/日本語", "xn-mnchen-3yade日本語")]
    [InlineData("https://xn--mnchen-3ya.de/日本語", "xn-mnchen-3yade日本語")]
    [InlineData("https://www.mediamarkt.es/es/product/_camara-instantanea-fujifilm-instax-mini-evo-iso-100-1600-pantalla-lcd-bluetooth-negro-1561872.html?utm_source=google&utm_medium=v2-sho&utm_campaign=es%7Cmm%7Crt%7Cv2-sho%7Cpmax%7Cnsp%7CTOP%7Cna%7Cd2c%7CNA%7CB0009TVC&utm_content=B0009TVC-TCID23908559954&gclsrc=aw.ds&gad_source=1&gad_campaignid=23912985034&gbraid=0AAAAADL_At-3b3_-pCuNKCsXxHKxHSfoY", "wwwmediamarktesesproductcamara-instantanea-fujifilm-instax-mini-evo-iso-100-1600-pantalla-lcd-bluetooth-negro-1561872html")]
    public void NormalizeMatchesSpecificationExamples(string input, string expected)
    {
        string actual = UrlNormalizationPolicy.Normalize(input);

        Assert.Equal(expected, actual);
    }

    [Trait("Category", "Validation")]
    [Fact]
    public void NormalizeIgnoresSchemeAndFragmentAndUsesInvariantCulture()
    {
        string a = "https://example.com/path?b=2&a=1#fragment";
        string b = "http://example.com/path?a=1&b=2";

        Assert.Equal(UrlNormalizationPolicy.Normalize(a), UrlNormalizationPolicy.Normalize(b));

        string input = "https://EXAMPLE.com/Iİ?utm_source=google&b=2&a=1";

        string invariantResult = UrlNormalizationPolicy.Normalize(input);

        string turkishResult;
        using (new CultureScope("tr-TR"))
        {
            turkishResult = UrlNormalizationPolicy.Normalize(input);
        }

        Assert.Equal(invariantResult, turkishResult);
    }

    [Fact]
    public void NormalizePreservesAndOrdersRepeatedQueryParameters()
    {
        string first = UrlNormalizationPolicy.Normalize("https://example.com/path?q=Z&q=a&q=a");
        string reordered = UrlNormalizationPolicy.Normalize("https://example.com/path?q=a&q=a&q=Z");

        Assert.Equal("examplecompathqaqaqz", first);
        Assert.Equal(first, reordered);
    }

    [Fact]
    public void NormalizeHandlesAQueryParameterWithoutValue()
    {
        Assert.Equal("examplecompathflag", UrlNormalizationPolicy.Normalize("https://example.com/path?flag"));
    }

    [Fact]
    public void NormalizeHandlesUrlWithoutPath()
    {
        Assert.Equal("examplecom", UrlNormalizationPolicy.Normalize("https://example.com"));
    }

    [Fact]
    public void NormalizeRemovesMarketingParametersWithoutCaseSensitivity()
    {
        string actual = UrlNormalizationPolicy.Normalize("https://example.com/path?UTM_SOURCE=Google&id=10");

        Assert.Equal("examplecompathid10", actual);
    }

    [Fact]
    public void NormalizeTreatsPlusAsSpaceOnlyInQueryDecoding()
    {
        string plusAsSpace = UrlNormalizationPolicy.Normalize("https://example.com/path?q=hello+world");
        string encodedPlus = UrlNormalizationPolicy.Normalize("https://example.com/path?q=hello%2Bworld");

        Assert.Equal("examplecompathqhello-world", plusAsSpace);
        Assert.Equal("examplecompathqhelloworld", encodedPlus);
        Assert.NotEqual(plusAsSpace, encodedPlus);
    }

    [Trait("Category", "Validation")]
    [Fact]
    public void NormalizeTreatsUnicodeAndPercentEncodedPathFormsAsEquivalent()
    {
        string percentEncoded = "https://example.com/%E6%97%A5%E6%9C%AC%E8%AA%9E?b=2&x=1";
        string unicodePath = "https://example.com/日本語?b=2&x=1";

        Assert.Equal("examplecom日本語b2x1", UrlNormalizationPolicy.Normalize(percentEncoded));
        Assert.Equal(UrlNormalizationPolicy.Normalize(percentEncoded), UrlNormalizationPolicy.Normalize(unicodePath));

        string percentEncodedPath = "TOSHIBA-%E3%83%95%E3%83%A9%E3%83%83%E3%83%88%E3%83%86%E3%83%BC%E3%83%96%E3%83%AB-%E3%82%AA%E3%83%AC%E3%83%B3%E3%82%B8LED%E3%82%BF%E3%82%A4%E3%83%9E%E3%83%BC-ER-NS170A-%E3%83%98%E3%83%AB%E3%83%84%E3%83%95%E3%83%AA%E3%83%BC";
        string japanesePath = "TOSHIBA-フラットテーブル-オレンジLEDタイマー-ER-NS170A-ヘルツフリー";
        percentEncoded = "https://www.amazon.co.jp/" + percentEncodedPath + "/dp/B0DPZBT572/?_encoding=UTF8&pd_rd_w=XIQqc&content-id=amzn1.sym.3e5fb751-95ea-4415-9e9c-3ab031dfb18e&pf_rd_p=3e5fb751-95ea-4415-9e9c-3ab031dfb18e&pf_rd_r=MN96K93MC8AZA67Q7NBT&pd_rd_wg=vwLOk&pd_rd_r=38b4ba6e-1b67-42b0-a9ba-9dc8ac197f22&ref_=pd_hp_d_atf_unk&th=1";
        unicodePath = percentEncoded.Replace(percentEncodedPath, japanesePath, StringComparison.Ordinal);

        string expected = "wwwamazoncojptoshiba-フラットテーフル-オレンシledタイマー-er-ns170a-ヘルツフリーdpb0dpzbt572encodingutf8content-idamzn1sym3e5fb751-95ea-4415-9e9c-3ab031dfb18epdrdr38b4ba6e-1b67-42b0-a9ba-9dc8ac197f22pdrdwxiqqcpdrdwgvwlokpfrdp3e5fb751-95ea-4415-9e9c-3ab031dfb18epfrdrmn96k93mc8aza67q7nbtrefpdhpdatfunkth1";

        Assert.Equal(expected, UrlNormalizationPolicy.Normalize(percentEncoded));
        Assert.Equal(expected, UrlNormalizationPolicy.Normalize(unicodePath));
    }

    [Theory]
    [InlineData("²", "²")]
    [InlineData("Ⅷ", "ⅷ")]
    [InlineData("\U00020000", "\U00020000")]
    [InlineData("\U00010400", "\U00010428")]
    [InlineData("\U0001D7D8", "\U0001D7D8")]
    [InlineData("\U00010107", "\U00010107")]
    public void Normalize_WhenUnicodeIsLetterOrNumber_PreservesLiteralAndEncodedForms(string value, string normalizedValue)
    {
        string encoded = Uri.EscapeDataString(value);
        string expectedPath = $"examplecom{normalizedValue}";
        string expectedQuery = $"examplecomq{normalizedValue}";

        Assert.Equal(expectedPath, UrlNormalizationPolicy.Normalize($"https://example.com/{value}"));
        Assert.Equal(expectedPath, UrlNormalizationPolicy.Normalize($"https://example.com/{encoded}"));
        Assert.Equal(expectedQuery, UrlNormalizationPolicy.Normalize($"https://example.com/?q={value}"));
        Assert.Equal(expectedQuery, UrlNormalizationPolicy.Normalize($"https://example.com/?q={encoded}"));
    }

    [Theory]
    [InlineData("²")]
    [InlineData("\U00020000")]
    public void Normalize_WhenUrlsDifferByUnicodeLetterOrNumber_DoesNotCollapseTheirKeys(string value)
    {
        string withCharacter = UrlNormalizationPolicy.Normalize($"https://example.com/?q=x{value}");
        string withoutCharacter = UrlNormalizationPolicy.Normalize("https://example.com/?q=x");

        Assert.NotEqual(withoutCharacter, withCharacter);
    }

    [Theory]
    [InlineData("\U0001F600")]
    [InlineData("€")]
    public void Normalize_WhenUnicodeIsSymbol_RemovesLiteralAndEncodedForms(string symbol)
    {
        const string expected = "examplecomabqxy";
        string encoded = Uri.EscapeDataString(symbol);

        Assert.Equal(expected, UrlNormalizationPolicy.Normalize($"https://example.com/a{symbol}b?q=x{symbol}y"));
        Assert.Equal(expected, UrlNormalizationPolicy.Normalize($"https://example.com/a{encoded}b?q=x{encoded}y"));
    }

    [Trait("Category", "Validation")]
    [Fact]
    public void NormalizeRejectsTextThatCannotBeParsedAsAbsoluteUrl()
    {
        Assert.Throws<DomainExceptions.UrlNotAbsoluteException>(() => UrlNormalizationPolicy.Normalize("not a url"));
    }

    /// <summary>
    /// Entrada artificial fuera de la precondición de URL ajustada y validada.
    /// Comprueba la clave técnica resultante, no la aceptación de esta entrada por el producto.
    /// </summary>
    [Trait("Category", "Validation")]
    [Theory]
    [InlineData("https://example.com/path?q=\uD800", "examplecompathq")]
    [InlineData("https://example.com/\uD800", "examplecom")]
    public void NormalizeWithArtificialLoneSurrogate_ReturnsStableKey(string input, string expected)
    {
        Assert.Equal(expected, UrlNormalizationPolicy.Normalize(input));
    }

    [Fact]
    public void NormalizeHandlesAQueryContainingOnlyTheSeparator()
    {
        Assert.Equal("examplecom", UrlNormalizationPolicy.Normalize("https://example.com/?"));
    }

    [Fact]
    public void NormalizeIgnoresAParameterWithoutName()
    {
        Assert.Equal("examplecom", UrlNormalizationPolicy.Normalize("https://example.com/?=value"));
    }

    [Fact]
    public void NormalizeReturnsAnEmptyResultForAUrlWithoutHostPathAndQuery()
    {
        Assert.Equal(string.Empty, UrlNormalizationPolicy.Normalize("mailto:"));
    }

    [Fact]
    public void EscapePathUnicodeReturnsNullOrEmptyInputUnchanged()
    {
        Assert.Null(PrivatePolicyInvoker.InvokeStatic(typeof(UrlNormalizationPolicy), "EscapePathUnicode", new object?[] { null }));
        Assert.Equal(
            string.Empty,
            PrivatePolicyInvoker.InvokeStatic(typeof(UrlNormalizationPolicy), "EscapePathUnicode", new object?[] { string.Empty }) as string);
    }

    [Fact]
    public void CultureScope_WhenInitialCulturesDiffer_RestoresBoth()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        CultureInfo originalUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es-ES");
            using (new CultureScope("tr-TR"))
            {
                Assert.Equal("tr-TR", CultureInfo.CurrentCulture.Name);
                Assert.Equal("tr-TR", CultureInfo.CurrentUICulture.Name);
            }
            Assert.Equal("en-US", CultureInfo.CurrentCulture.Name);
            Assert.Equal("es-ES", CultureInfo.CurrentUICulture.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
            CultureInfo.CurrentUICulture = originalUi;
        }
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previous;
        private readonly CultureInfo _previousUi;

        public CultureScope(string cultureName)
        {
            _previous = CultureInfo.CurrentCulture;
            _previousUi = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previous;
            CultureInfo.CurrentUICulture = _previousUi;
        }
    }
}
