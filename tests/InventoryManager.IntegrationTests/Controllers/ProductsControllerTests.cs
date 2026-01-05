using InventoryManager.API.Infrastructure.Errors;
using InventoryManager.Application.DTOs.Common;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Application.UseCases.Products.Create;
using InventoryManager.IntegrationTests.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventoryManager.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class ProductsControllerTests(IntegrationTestWebAppFactory factory)
{
    private const string BaseUrl = "/api/products";
    private const int MaxSkuLength = 20;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client = factory.CreateClient();

    [Fact(DisplayName = "Should create a product when the request is valid")]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "IPhone 15 Pro",
            Description: "Apple Smartphone",
            Price: 1200m,
            StockQuantity: 10,
            Sku: $"SKU-{Guid.NewGuid():N}"[..MaxSkuLength]);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ProductResponse? productResponse = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(productResponse);
        Assert.Equal(request.Name, productResponse.Name);
        Assert.NotEqual(Guid.Empty, productResponse.Id);
    }

    [Fact(DisplayName = "Should return BadRequest when creating product with invalid data")]
    public async Task Create_ShouldReturnBadRequest_WhenDataIsInvalid()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: string.Empty,
            Description: "Description",
            Price: -10m,
            StockQuantity: 10,
            Sku: "SKU-INVALID");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(ValidationExceptionHandler.DefaultType, problemDetails.Type);
        Assert.Equal(ValidationExceptionHandler.DefaultDetail, problemDetails.Detail);
        Assert.True(problemDetails.Errors.ContainsKey(nameof(request.Name)));
        Assert.True(problemDetails.Errors.ContainsKey(nameof(request.Price)));
    }

    [Fact(DisplayName = "Should return Conflict when creating a product with an existing SKU")]
    public async Task Create_ShouldReturnConflict_WhenSkuAlreadyExists()
    {
        // Arrange
        CreateProductRequest product1 = new(
            Name: "Original Product",
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: $"SKU-DUPLICATE-{Guid.NewGuid():N}"[..MaxSkuLength]);

        string expectedSku = product1.Sku;

        HttpResponseMessage firstResponse = await _client.PostAsJsonAsync(BaseUrl, product1);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        CreateProductRequest product2WithSameSku = new(
            Name: "Copycat Product",
            Description: "Desc",
            Price: 200m,
            StockQuantity: 5,
            Sku: product1.Sku);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, product2WithSameSku);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(ConflictExceptionHandler.DefaultTitle, problemDetails.Title);
        Assert.Equal(ConflictExceptionHandler.DefaultType, problemDetails.Type);
        string expectedDetail = string.Format(CreateProductUseCase.SkuAlreadyExistsMessage, expectedSku);
        Assert.Equal(expectedDetail, problemDetails.Detail);
    }

    [Fact(DisplayName = "Should return all products with pagination header")]
    public async Task GetAll_ShouldReturnOk_WithProducts()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "IPhone 15 Pro",
            Description: "Apple Smartphone",
            Price: 1200m,
            StockQuantity: 10,
            Sku: $"SKU-{Guid.NewGuid():N}"[..MaxSkuLength]);

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(BaseUrl, request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{BaseUrl}?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ProductResponse>? products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();

        Assert.NotNull(products);
        Assert.NotEmpty(products);
        Assert.True(response.Headers.Contains("X-Pagination"), "Response should contain X-Pagination header");

        string jsonHeader = response.Headers.GetValues("X-Pagination").First();
        PagedMeta? meta = JsonSerializer.Deserialize<PagedMeta>(jsonHeader, JsonOptions);

        Assert.NotNull(meta);
        Assert.Equal(15, meta.TotalItems);
        Assert.Equal(2, meta.TotalPages);
        Assert.Equal(1, meta.CurrentPage);
        Assert.True(meta.HasNextPage);
    }

    [Fact(DisplayName = "Should return product when Id exists")]
    public async Task GetById_ShouldReturnOk_WhenIdExists()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "IPhone 15 Pro",
            Description: "Apple Smartphone",
            Price: 1200m,
            StockQuantity: 10,
            Sku: $"SKU-{Guid.NewGuid():N}"[..MaxSkuLength]);

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(BaseUrl, request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        ProductResponse? createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(createdProduct);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{BaseUrl}/{createdProduct.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ProductResponse? productResponse = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(productResponse);
        Assert.Equal(createdProduct.Id, productResponse.Id);
    }

    [Fact(DisplayName = "Should return NotFound when Id does not exist")]
    public async Task GetById_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);

        Assert.Equal(NotFoundExceptionHandler.DefaultTitle, problemDetails.Title);
        Assert.Equal(NotFoundExceptionHandler.DefaultType, problemDetails.Type);
    }

    [Fact(DisplayName = "Should return NoContent when product is updated successfully")]
    public async Task Update_ShouldReturnNoContent_WhenUpdateIsValid()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "IPhone 15 Pro",
            Description: "Apple Smartphone",
            Price: 1200m,
            StockQuantity: 10,
            Sku: $"SKU-{Guid.NewGuid():N}"[..MaxSkuLength]);

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(BaseUrl, request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        ProductResponse? createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(createdProduct);

        UpdateProductRequest updateRequest = new(
            Name: "Updated Name",
            Description: "Updated Description",
            Price: 1500m,
            StockQuantity: 50,
            Sku: createdProduct.Sku);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"{BaseUrl}/{createdProduct.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        ProductResponse? getResponse = await _client.GetFromJsonAsync<ProductResponse>($"{BaseUrl}/{createdProduct.Id}");
        Assert.Equal("Updated Name", getResponse?.Name);
    }

    [Fact(DisplayName = "Should return NotFound when updating non-existent product")]
    public async Task Update_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        // Arrange
        UpdateProductRequest updateRequest = new(
            Name: "Name",
            Description: "Desc",
            Price: 10m,
            StockQuantity: 1,
            Sku: "SKU-TEST");

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"{BaseUrl}/{Guid.NewGuid()}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Should return NoContent when product is deleted successfully")]
    public async Task Delete_ShouldReturnNoContent_WhenIdExists()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "IPhone 15 Pro",
            Description: "Apple Smartphone",
            Price: 1200m,
            StockQuantity: 10,
            Sku: $"SKU-{Guid.NewGuid():N}"[..MaxSkuLength]);

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(BaseUrl, request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        ProductResponse? createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(createdProduct);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"{BaseUrl}/{createdProduct.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        HttpResponseMessage getResponse = await _client.GetAsync($"{BaseUrl}/{createdProduct.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact(DisplayName = "Should return NotFound when deleting non-existent product")]
    public async Task Delete_ShouldReturnNotFound_WhenIdDoesNotExist()
    {
        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"{BaseUrl}/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}