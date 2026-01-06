using InventoryManager.IntegrationTests.Abstractions;
using System.Net;

namespace InventoryManager.IntegrationTests.Http.Specs;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class OpenApiTests(IntegrationTestWebAppFactory factory)
{
    const string BaseUrl = "/openapi/v1.json";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact(DisplayName = "Should return OpenAPI JSON documentation with correct metadata")]
    public async Task GetOpenApiJson_ShouldReturnOk_AndContainMetadata()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync(BaseUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));

        Assert.Contains("Inventory Manager API", content);
        Assert.Contains("v1", content);
    }
}