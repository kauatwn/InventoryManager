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
    public void Execute_ShouldDeleteProduct_WhenProductExists()
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

        _mockRepository.Setup(r => r.GetById(productId))
            .Returns(product);

        // Act
        _sut.Execute(productId);

        // Assert
        _mockRepository.Verify(r => r.GetById(productId), Times.Once);
        _mockRepository.Verify(r => r.Delete(product), Times.Once);
    }

    [Fact(DisplayName = "Execute should throw NotFoundException when product does not exist")]
    public void Execute_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetById(productId))
            .Returns((Product?)null);

        string expectedMessage = string.Format(DeleteProductUseCase.ProductNotFoundMessage, productId);

        // Act
        void Act() => _sut.Execute(productId);

        // Assert
        var exception = Assert.Throws<NotFoundException>(Act);
        Assert.Equal(expectedMessage, exception.Message);

        _mockRepository.Verify(r => r.Delete(It.IsAny<Product>()), Times.Never);
    }
}