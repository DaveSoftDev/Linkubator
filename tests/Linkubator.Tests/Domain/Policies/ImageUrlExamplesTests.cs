using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainPolicies = Linkubator.Domain.Policies;

namespace Linkubator.Tests.Domain.Policies;

[Trait("Category", "Image")]
[Trait("Category", "Examples")]
public class ImageUrlExamplesTests
{
    [Theory]
    [InlineData("https://cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("http://cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("HTTP://cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("http://cdn.example.com:8080/img.jpg", "https://cdn.example.com:8080/img.jpg")]
    [InlineData("http://cdn.example.com:80/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("//cdn.example.com/img.jpg", "https://cdn.example.com/img.jpg")]
    [InlineData("cdn.example.com:8080/img.jpg", "https://cdn.example.com:8080/img.jpg")]
    [InlineData("//cdn.example.com/img/img-01.jpg", "https://cdn.example.com/img/img-01.jpg")]
    [InlineData("/img/a.jpg", "https://example.com/img/a.jpg", "http://example.com/post")]
    [InlineData("img.jpg", "https://example.com/a/b/img.jpg", "http://example.com/a/b/post")]
    [InlineData("cdn.example.com/img.jpg", "https://example.com/a/b/cdn.example.com/img.jpg", "https://example.com/a/b/post")]
    [InlineData("../assets/cover.webp?width=800&format=webp", "https://example.com/a/assets/cover.webp?width=800&format=webp", "https://example.com/a/b/post")]
    [InlineData("?width=1200", "https://example.com/a/b/post?width=1200", "https://example.com/a/b/post?old=1")]
    public void ImageUrlExamplesMatchTheSpecificationTable(
        string input,
        string expected,
        string? baseUrl = null)
    {
        if (baseUrl is null)
        {
            baseUrl = "https://example.com/a/b/post";
        }

        Assert.Equal(expected, DomainPolicies.ImageUrlPolicy.Adjust(input, baseUrl));
    }

    [Theory]
    [InlineData("localhost:5000/img.jpg")]
    [InlineData("http://localhost/img.jpg")]
    [InlineData("https://LOCALHOST/img.jpg")]
    [InlineData("//localhost/img.jpg")]
    [InlineData("http://127.0.0.1/img.jpg")]
    [InlineData("https://192.168.1.1/img.jpg")]
    [InlineData("https://[::1]/img.jpg")]
    [InlineData("/img/a.jpg", "https://192.168.1.1/post")]
    [InlineData("/img/a.jpg", "http://localhost/post")]
    public void ImageUrlExamplesRejectLocalHostsAndPrivateIpAddresses(
        string input,
        string? baseUrl = null)
    {
        if (baseUrl is null)
        {
            baseUrl = "https://example.com/a/b/post";
        }

        Assert.Throws<DomainExceptions.ImageHostNotAllowedException>(() => DomainPolicies.ImageUrlPolicy.Adjust(input, baseUrl));
    }
}
