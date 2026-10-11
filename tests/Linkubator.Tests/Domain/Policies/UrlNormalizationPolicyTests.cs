using System.Globalization;
using DomainExceptions = Linkubator.Domain.Exceptions;
using Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

public class UrlNormalizationPolicyTests
{
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
    [InlineData("https://www.mediamarkt.es/es/product/_camara-instantanea-fujifilm-instax-mini-evo-iso-100-1600-pantalla-lcd-bluetooth-negro-1561872.html?utm_source=google&utm_medium=v2-sho&utm_campaign=es%7Cmm%7Crt%7Cv2-sho%7Cpmax%7Cnsp%7CTOP%7Cna%7Cd2c%7CNA%7CB0009TVC&utm_content=B0009TVC-TCID23908559954&gclsrc=aw.ds&gad_source=1&gad_campaignid=23912985034&gbraid=0AAAAADL_At-3b3_-pCuNKCsXxHKxHSfoY", "wwwmediamarktesesproductcamara-instantanea-fujifilm-instax-mini-evo-iso-100-1600-pantalla-lcd-bluetooth-negro-1561872htmlgclsrcawds")]
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

    [Trait("Category", "Validation")]
    [Fact]
    public void NormalizeDoesNotRevalidateAnAlreadyAdjustedUrl()
    {
        Assert.Throws<ArgumentException>(() => UrlNormalizationPolicy.Normalize("not a url"));
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previous;

        public CultureScope(string cultureName)
        {
            _previous = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previous;
            CultureInfo.CurrentUICulture = _previous;
        }
    }
}
