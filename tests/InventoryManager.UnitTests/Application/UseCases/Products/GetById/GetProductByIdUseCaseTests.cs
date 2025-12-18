using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Application.UseCases.Products.GetById;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManager.UnitTests.Application.UseCases.Products.GetById;

[Trait("Category", "Unit")]
public class GetProductByIdUseCaseTests
{
    private readonly Mock<IProductRepository> _mockRepository = new();

    private readonly ILogger<GetProductByIdUseCase> _logger = Mock.Of<ILogger<GetProductByIdUseCase>>();

    private readonly GetProductByIdUseCase _sut;

    public GetProductByIdUseCaseTests()
    {
        _sut = new GetProductByIdUseCase(_mockRepository.Object, _logger);
    }

    [Fact(DisplayName = "Execute should return product response when product exists")]
    public void Execute_ShouldReturnProductResponse_WhenProductExists()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        Product product = new(
            name: "Gaming Monitor",
            description: "144Hz 1ms",
            price: 1500.00m,
            stockQuantity: 5,
            sku: "MON-144"
        );

        _mockRepository.Setup(r => r.GetById(productId))
            .Returns(product);

        // Act
        ProductResponse response = _sut.Execute(productId);

        // Assert
        Assert.NotNull(response);

        Assert.Equal(product.Id, response.Id);
        Assert.Equal(product.Name, response.Name);
        Assert.Equal(product.Description, response.Description);
        Assert.Equal(product.Price, response.Price);
        Assert.Equal(product.StockQuantity, response.StockQuantity);
        Assert.Equal(product.Sku, response.Sku);

        _mockRepository.Verify(r => r.GetById(productId), Times.Once);
    }

    [Fact(DisplayName = "Execute should throw NotFoundException when product does not exist")]
    public void Execute_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetById(productId))
            .Returns((Product?)null);

        string expectedMessage = string.Format(GetProductByIdUseCase.ProductNotFoundMessage, productId);

        // Act
        void Act() => _sut.Execute(productId);

        // Assert
        var exception = Assert.Throws<NotFoundException>(Act);
        Assert.Equal(expectedMessage, exception.Message);

        _mockRepository.Verify(r => r.GetById(productId), Times.Once);
    }
}