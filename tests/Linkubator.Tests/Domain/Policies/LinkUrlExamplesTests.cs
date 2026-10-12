using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "Links")]
[Trait("Category", "Examples")]
public class LinkUrlExamplesTests
{
    [Fact]
    public void LinkUrlExamplesMatchTheSpecificationTable()
    {
        Assert.Equal("https://otro-ejemplo.com/otra-pagina", DomainPolicies.LinkUrlPolicy.Adjust("//otro-ejemplo.com/otra-pagina"));
        Assert.Equal("http://localhost:5000/pagina-interna", DomainPolicies.LinkUrlPolicy.Adjust("localhost:5000/pagina-interna"));
        Assert.Equal("https://example.com:8080/x", DomainPolicies.LinkUrlPolicy.Adjust("example.com:8080/x"));
        Assert.Equal("https://192.168.1.1/pagina", DomainPolicies.LinkUrlPolicy.Adjust("https://192.168.1.1/pagina"));
        Assert.Throws<DomainExceptions.UrlSchemeNotAllowedException>(() => DomainPolicies.LinkUrlPolicy.Adjust("javascript:alert(1)"));
        Assert.Throws<DomainExceptions.UrlSchemeNotAllowedException>(() => DomainPolicies.LinkUrlPolicy.Adjust("mailto:ana@example.com"));
        Assert.Throws<DomainExceptions.UrlAuthorityMissingException>(() => DomainPolicies.LinkUrlPolicy.Adjust("http:example.com"));
    }
}
