using InventoryManager.Application.UseCases.Products.Delete;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManager.UnitTests.Application.UseCases.Products.Delete;

[Trait("Category", "Unit")]
public class DeleteProductUseCaseTests
{
    private readonly Mock<IProductRepository> _mockRepository = new();

    private readonly ILogger<DeleteProductUseCase> _logger = Mock.Of<ILogger<DeleteProductUseCase>>();

    private readonly DeleteProductUseCase _sut;

    public DeleteProductUseCaseTests()
    {
        _sut = new DeleteProductUseCase(repository: _mockRepository.Object, logger: _logger);
    }

    [Fact(DisplayName = "Execute should delete product when product exists")]
    public async Task Execute_ShouldDeleteProduct_WhenProductExists()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        Product product = new(
            name: "Mouse to Delete",
            description: "Desc",
            price: 50m,
            stockQuantity: 1,
            sku: "DEL-001"
        );

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        await _sut.ExecuteAsync(productId);

        // Assert
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(product), Times.Once);
    }

    [Fact(DisplayName = "Execute should throw NotFoundException when product does not exist")]
    public async Task Execute_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        string expectedMessage = string.Format(DeleteProductUseCase.ProductNotFoundMessage, productId);

        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(productId);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(ActAsync);
        Assert.Equal(expectedMessage, exception.Message);

        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
    }
}