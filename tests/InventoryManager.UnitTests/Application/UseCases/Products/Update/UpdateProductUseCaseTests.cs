using FluentValidation;
using FluentValidation.Results;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.UseCases.Products.Update;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using ValidationException = InventoryManager.Domain.Exceptions.ValidationException;

namespace InventoryManager.UnitTests.Application.UseCases.Products.Update;

[Trait("Category", "Unit")]
public class UpdateProductUseCaseTests
{
    private readonly Mock<IProductRepository> _mockRepository = new();
    private readonly Mock<IValidator<UpdateProductRequest>> _mockValidator = new();

    private readonly ILogger<UpdateProductUseCase> _logger = Mock.Of<ILogger<UpdateProductUseCase>>();

    private readonly UpdateProductUseCase _sut;

    public UpdateProductUseCaseTests()
    {
        _sut = new UpdateProductUseCase(_mockRepository.Object, _mockValidator.Object, _logger);
    }

    [Fact(DisplayName = "Execute should update product successfully when request is valid")]
    public async Task Execute_ShouldUpdateProductSuccessfully_WhenRequestIsValid()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Product existingProduct = new(name: "Old Name", description: "Old Desc", price: 100m, stockQuantity: 5, sku: "OLD-SKU");

        UpdateProductRequest request = new(
            Name: "New Gamer Mouse",
            Description: "New Desc",
            Price: 150m,
            StockQuantity: 10,
            Sku: "NEW-SKU"
        );

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _mockRepository.Setup(r => r.IsSkuUniqueAsync(request.Sku, productId))
            .ReturnsAsync(true);

        // Act
        await _sut.ExecuteAsync(productId, request);

        // Assert
        Assert.Equal(request.Name, existingProduct.Name);
        Assert.Equal(request.Sku, existingProduct.Sku);

        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.IsSkuUniqueAsync(request.Sku, productId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
    }

    [Fact(DisplayName = "Execute should throw ArgumentNullException when request is null")]
    public async Task Execute_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        Guid productId = Guid.NewGuid();

        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(productId, null!);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(ActAsync);
        Assert.Equal("request", exception.ParamName);

        // Garante que nada foi chamado
        _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<UpdateProductRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact(DisplayName = "Execute should throw ValidationException when validator fails")]
    public async Task Execute_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        UpdateProductRequest request = new(Name: "Name", Description: "Desc", Price: -10m, StockQuantity: 10, Sku: "SKU");

        const string expectedKey = nameof(UpdateProductRequest.Price);
        const string expectedMessage = UpdateProductValidator.PriceGreaterThanZero;

        ValidationFailure failure = new(expectedKey, expectedMessage);

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([failure]));

        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(productId, request);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(ActAsync);
        Assert.Contains(expectedKey, exception.Errors.Keys);
        Assert.Contains(exception.Errors[expectedKey], error => error == expectedMessage);

        // Verify
        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact(DisplayName = "Execute should throw NotFoundException when product does not exist")]
    public async Task Execute_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        UpdateProductRequest request = new(Name: "Name", Description: "Desc", Price: 100m, StockQuantity: 10, Sku: "SKU");

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        string expectedMessage = string.Format(UpdateProductUseCase.ProductNotFoundMessage, productId);

        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(productId, request);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(ActAsync);
        Assert.Equal(expectedMessage, exception.Message);

        // Verify
        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact(DisplayName = "Execute should throw ConflictException when SKU is already in use by another product")]
    public async Task Execute_ShouldThrowConflictException_WhenSkuAlreadyInUse()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Product existingProduct = new(name: "Old Name", description: "Desc", price: 100m, stockQuantity: 5, sku: "OLD-SKU");
        UpdateProductRequest request = new(Name: "Name", Description: "Desc", Price: 100m, StockQuantity: 10, Sku: "DUPLICATE-SKU");

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        _mockRepository.Setup(r => r.IsSkuUniqueAsync(request.Sku, productId))
            .ReturnsAsync(false);

        string expectedMessage = string.Format(UpdateProductUseCase.SkuAlreadyExistsMessage, request.Sku);

        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(productId, request);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(ActAsync);

        // Verificamos a mensagem
        Assert.Equal(expectedMessage, exception.Message);

        // Verify
        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockRepository.Verify(r => r.IsSkuUniqueAsync(request.Sku, productId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
}