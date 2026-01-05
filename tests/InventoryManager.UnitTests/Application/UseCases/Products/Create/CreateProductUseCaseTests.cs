using FluentValidation;
using FluentValidation.Results;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Application.UseCases.Products.Create;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using ValidationException = InventoryManager.Domain.Exceptions.ValidationException;

namespace InventoryManager.UnitTests.Application.UseCases.Products.Create;

[Trait("Category", "Unit")]
public class CreateProductUseCaseTests
{
    private readonly Mock<IProductRepository> _mockRepository = new();
    private readonly Mock<IValidator<CreateProductRequest>> _mockValidator = new();

    private readonly ILogger<CreateProductUseCase> _logger = Mock.Of<ILogger<CreateProductUseCase>>();

    private readonly CreateProductUseCase _sut;

    public CreateProductUseCaseTests()
    {
        _sut = new CreateProductUseCase(_mockRepository.Object, _mockValidator.Object, _logger);
    }

    [Fact(DisplayName = "Execute should create product successfully when request is valid")]
    public async Task Execute_ShouldCreateProductSuccessfully_WhenRequestIsValid()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "Gamer Keyboard",
            Description: "Mechanical RGB",
            Price: 200.00m,
            StockQuantity: 50,
            Sku: "KB-RGB-001");

        _mockValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.ExistsAsync(request.Sku))
            .ReturnsAsync(false);

        // Act
        ProductResponse response = await _sut.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.Description, response.Description);
        Assert.Equal(request.Price, response.Price);
        Assert.Equal(request.StockQuantity, response.StockQuantity);
        Assert.Equal(request.Sku, response.Sku);


        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.ExistsAsync(request.Sku), Times.Once);

        _mockRepository.Verify(r => r.AddAsync(It.Is<Product>(p =>
            p.Name == request.Name &&
            p.Sku == request.Sku &&
            p.Price == request.Price
        )), Times.Once);
    }

    [Fact(DisplayName = "Execute should throw ArgumentNullException when request is null")]
    public async Task Execute_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(null!);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(ActAsync);
        Assert.Equal("request", exception.ParamName);

        _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<CreateProductRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact(DisplayName = "Execute should throw ValidationException when validator fails")]
    public async Task Execute_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: string.Empty,
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: "SKU-001");

        const string expectedKey = nameof(CreateProductRequest.Name);
        const string expectedMessage = CreateProductValidator.NameRequired;

        ValidationFailure failure = new(propertyName: expectedKey, errorMessage: expectedMessage);

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([failure]));

        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(request);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(ActAsync);

        Assert.Contains(expectedKey, exception.Errors.Keys);
        Assert.Contains(exception.Errors[expectedKey], error => error == expectedMessage);

        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact(DisplayName = "Execute should throw ConflictException when SKU already exists")]
    public async Task Execute_ShouldThrowConflictException_WhenSkuAlreadyExists()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "Existing Item",
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: "EXISTING-SKU");

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.ExistsAsync(request.Sku))
            .ReturnsAsync(true);

        string expectedMessage = string.Format(CreateProductUseCase.SkuAlreadyExistsMessage, request.Sku);

        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(request);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(ActAsync);
        Assert.Equal(expectedMessage, exception.Message);

        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRepository.Verify(r => r.ExistsAsync(request.Sku), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }
}