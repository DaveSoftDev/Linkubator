using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "Normalization")]
[Trait("Category", "Examples")]
public class UrlNormalizationExamplesTests
{
    [Fact]
    public void UrlNormalizationExamplesMatchTheSource()
    {
        Assert.Equal("examplecomgeneracion-de-datosid10", DomainPolicies.UrlNormalizationPolicy.Normalize("HTTPS://Example.COM/Generación de datos/?utm_source=google&id=10"));
        Assert.Equal("examplecom4587articulo-numero-2a1b2", DomainPolicies.UrlNormalizationPolicy.Normalize("http://example.com:4587/Artículo número 2?b=2&a=1"));
        Assert.Equal("examplecom8080x", DomainPolicies.UrlNormalizationPolicy.Normalize("https://example.com:8080/x"));
        Assert.Equal("examplecomx", DomainPolicies.UrlNormalizationPolicy.Normalize("https://example.com:443/x"));
        Assert.Equal("examplecomctutorialqc", DomainPolicies.UrlNormalizationPolicy.Normalize("https://example.com/c++/tutorial?q=c++"));
        Assert.Equal("examplecoma20b", DomainPolicies.UrlNormalizationPolicy.Normalize("https://example.com/a%2520b"));
        Assert.Equal("xn-mnchen-3yade日本語", DomainPolicies.UrlNormalizationPolicy.Normalize("https://xn--mnchen-3ya.de/日本語"));
        Assert.Equal("4587articulo-numero-2a1b2", DomainPolicies.UrlNormalizationPolicy.Normalize("example.com:4587/articulo-numero-2/?a=1&b=2"));
    }
}
