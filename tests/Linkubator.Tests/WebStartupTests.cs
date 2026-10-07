using Microsoft.AspNetCore.Mvc.Testing;

namespace Linkubator.Tests;

public class WebStartupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WebStartupTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HomePageReturnsSuccess()
    {
        using var response = await _client.GetAsync("/");

        Assert.True(response.IsSuccessStatusCode);
    }
}