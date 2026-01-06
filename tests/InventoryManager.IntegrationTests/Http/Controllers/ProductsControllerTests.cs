using InventoryManager.API.ExceptionHandlers;
using InventoryManager.Application.DTOs.Common;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Application.UseCases.Products.Create;
using InventoryManager.Application.UseCases.Products.GetAll;
using InventoryManager.Domain.Entities;
using InventoryManager.IntegrationTests.Abstractions;
using InventoryManager.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventoryManager.IntegrationTests.Http.Controllers;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class ProductsControllerTests(IntegrationTestWebAppFactory factory)
{
    private const string BaseUrl = "/api/products";
    private const int MaxSkuLength = 20;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client = factory.CreateClient();
    private readonly IntegrationTestSeeder _seeder = new(factory);

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
        Assert.True(problemDetails.Errors.ContainsKey(nameof(request.Name)));
        Assert.True(problemDetails.Errors.ContainsKey(nameof(request.Price)));
    }

    [Fact(DisplayName = "Should return Conflict when creating a product with an existing SKU")]
    public async Task Create_ShouldReturnConflict_WhenSkuAlreadyExists()
    {
        // Arrange
        string sku = $"SKU-DUP-{Guid.NewGuid():N}"[..MaxSkuLength];
        await _seeder.CreateProductAsync(sku: sku);

        CreateProductRequest productWithSameSku = new(
            Name: "Copycat Product",
            Description: "Desc",
            Price: 200m,
            StockQuantity: 5,
            Sku: sku);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, productWithSameSku);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(ConflictExceptionHandler.DefaultTitle, problemDetails.Title);
        string expectedDetail = string.Format(CreateProductUseCase.SkuAlreadyExistsMessage, sku);
        Assert.Equal(expectedDetail, problemDetails.Detail);
    }

    [Fact(DisplayName = "Should return all products with pagination header")]
    public async Task GetAll_ShouldReturnOk_WithProducts()
    {
        // Arrange
        Product product = await _seeder.CreateProductAsync();

        // Act
        HttpResponseMessage response = await _client.GetAsync(
            $"{BaseUrl}?page=1&pageSize={GetAllProductsValidator.MaxPageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ProductResponse>? products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();

        Assert.NotNull(products);
        Assert.NotEmpty(products);
        Assert.Contains(products, p => p.Id == product.Id);
        Assert.True(response.Headers.Contains("X-Pagination"));

        string jsonHeader = response.Headers.GetValues("X-Pagination").First();
        PagedMeta? meta = JsonSerializer.Deserialize<PagedMeta>(jsonHeader, JsonOptions);

        Assert.NotNull(meta);
        Assert.True(meta.TotalItems >= 15);
    }

    [Fact(DisplayName = "Should return product when Id exists")]
    public async Task GetById_ShouldReturnOk_WhenIdExists()
    {
        // Arrange
        Product product = await _seeder.CreateProductAsync(name: "Test GetById");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"{BaseUrl}/{product.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ProductResponse? productResponse = await response.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(productResponse);
        Assert.Equal(product.Id, productResponse.Id);
        Assert.Equal("Test GetById", productResponse.Name);
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
    }

    [Fact(DisplayName = "Should return NoContent when product is updated successfully")]
    public async Task Update_ShouldReturnNoContent_WhenUpdateIsValid()
    {
        // Arrange
        Product product = await _seeder.CreateProductAsync(name: "Original Name", price: 100m);

        UpdateProductRequest updateRequest = new(
            Name: "Updated Name",
            Description: "Updated Description",
            Price: 1500m,
            StockQuantity: 50,
            Sku: product.Sku);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"{BaseUrl}/{product.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        Product? updatedProduct = await _seeder.GetProductByIdAsync(product.Id);

        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Name", updatedProduct.Name);
        Assert.Equal(1500m, updatedProduct.Price);
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
        Product product = await _seeder.CreateProductAsync();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"{BaseUrl}/{product.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        Product? deletedProduct = await _seeder.GetProductByIdAsync(product.Id);
        Assert.Null(deletedProduct);
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