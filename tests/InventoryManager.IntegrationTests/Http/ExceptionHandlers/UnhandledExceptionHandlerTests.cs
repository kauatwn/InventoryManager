using InventoryManager.API.ExceptionHandlers;
using InventoryManager.Domain.Interfaces;
using InventoryManager.IntegrationTests.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace InventoryManager.IntegrationTests.Http.ExceptionHandlers;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class UnhandledExceptionHandlerTests(IntegrationTestWebAppFactory factory)
{
    private const string BaseUrl = "/api/products";

    [Fact(DisplayName = "Should return 500 Internal Server Error when an unexpected exception occurs")]
    public async Task Handle_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        Mock<IProductRepository> mockRepository = new();

        mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .Throws(new Exception("Unexpected error simulated for testing purposes."));

        HttpClient client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                ServiceDescriptor? descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(IProductRepository));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddScoped(_ => mockRepository.Object);
            });
        }).CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal(UnhandledExceptionHandler.DefaultTitle, problemDetails.Title);
        Assert.Equal(UnhandledExceptionHandler.DefaultType, problemDetails.Type);
        Assert.Equal(UnhandledExceptionHandler.DefaultDetail, problemDetails.Detail);
    }
}