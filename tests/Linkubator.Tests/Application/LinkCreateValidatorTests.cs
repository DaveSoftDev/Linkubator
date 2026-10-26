using Requests = Linkubator.Application.Dtos.Requests;
using Results = Linkubator.Application.Results;
using LinkCreateValidator = Linkubator.Application.Validation.Links.LinkCreateValidator;

namespace Linkubator.Tests.Application;

public sealed class LinkCreateValidatorTests
{
    [Theory]
    [InlineData("javascript:alert(1)", null, "Url", "UrlSchemeNotAllowed")]
    [InlineData("https://example.com", "file:///image.png", "Image", "UrlSchemeNotAllowed")]
    [InlineData("https://example.com", "https://127.0.0.1/image.png", "Image", "ImageHostNotAllowed")]
    public void Validate_WithInvalidUrlOrImage_IdentifiesField(string url, string? image, string field, string code)
    {
        LinkCreateValidator validator = new();

        Results.Result result = validator.Validate(new Requests.LinkCreateRequest(1, url, Image: image));

        Assert.False(result.IsSuccess);
        Assert.Equal(field, Assert.Single(result.Errors).Field);
        Assert.Equal(code, result.Errors[0].Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_WithOversizedText_IdentifiesField(bool title)
    {
        LinkCreateValidator validator = new();
        Requests.LinkCreateRequest request = new(1, "https://example.com",
            Title: title ? new string('x', 301) : null,
            Description: title ? null : new string('x', 1001));

        Results.Result result = validator.Validate(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(title ? "Title" : "Description", Assert.Single(result.Errors).Field);
        Assert.Equal("TextExceedsMaximumLength", result.Errors[0].Code);
    }
}
