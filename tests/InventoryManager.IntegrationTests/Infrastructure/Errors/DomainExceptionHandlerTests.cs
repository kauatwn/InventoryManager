using System.Net;
using System.Net.Http.Json;
using InventoryManager.API.Infrastructure.Errors;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.UseCases.Products.Create;
using InventoryManager.Domain.Exceptions;
using InventoryManager.IntegrationTests.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace InventoryManager.IntegrationTests.Infrastructure.Errors;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class DomainExceptionHandlerTests(IntegrationTestWebAppFactory factory)
{
    [Fact(DisplayName = "Should return 422 Unprocessable Entity when a DomainException occurs")]
    public async Task Handle_ShouldReturnUnprocessableEntity_WhenDomainExceptionOccurs()
    {
        // Arrange
        Mock<ICreateProductUseCase> mockUseCase = new();
        const string domainErrorMessage = "Domain rule violation simulated for testing purposes.";
        
        mockUseCase .Setup(useCase => useCase.ExecuteAsync(It.IsAny<CreateProductRequest>()))
            .Throws(new DomainException(domainErrorMessage));
        
        HttpClient client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                ServiceDescriptor? descriptor = services.SingleOrDefault(d =>
                {
                    return d.ServiceType == typeof(ICreateProductUseCase);
                });
                
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }
                
                services.AddScoped(_ => mockUseCase.Object);
            });
        }).CreateClient();
        
        CreateProductRequest validRequest = new(Name: "Valid Name", Description: "Desc", Price: 10m, StockQuantity: 10, Sku: "VALID-SKU");

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/products", validRequest);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.Status);
        Assert.Equal(DomainExceptionHandler.DefaultTitle, problemDetails.Title);
        Assert.Equal(DomainExceptionHandler.DefaultType, problemDetails.Type);
        Assert.Equal(domainErrorMessage, problemDetails.Detail);
    }
}