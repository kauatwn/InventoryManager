using FluentValidation;
using FluentValidation.Results;
using InventoryManager.Application.DTOs.Common;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Application.UseCases.Products.GetAll;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using ValidationException = InventoryManager.Domain.Exceptions.ValidationException;

namespace InventoryManager.UnitTests.Application.UseCases.Products.GetAll;

[Trait("Category", "Unit")]
public class GetAllProductsUseCaseTests
{
    private readonly Mock<IProductRepository> _mockRepository = new();
    private readonly Mock<IValidator<GetAllProductsRequest>> _mockValidator = new();

    private readonly ILogger<GetAllProductsUseCase> _logger = Mock.Of<ILogger<GetAllProductsUseCase>>();

    private readonly GetAllProductsUseCase _sut;

    public GetAllProductsUseCaseTests()
    {
        _sut = new GetAllProductsUseCase(_mockRepository.Object, _mockValidator.Object, _logger);
    }

    [Fact(DisplayName = "Execute should return paged products when request is valid")]
    public async Task Execute_ShouldReturnPagedProducts_WhenRequestIsValid()
    {
        // Arrange
        GetAllProductsRequest request = new(Page: 1, PageSize: 10);
        List<Product> products =
        [
            new Product(name: "Mouse", description: "Desc", price: 100m, stockQuantity: 5, sku: "SKU1"),
            new Product(name: "Keyboard", description: "Desc", price: 200m, stockQuantity: 10, sku: "SKU2")
        ];

        ProductResponse expected = new(
            products[0].Id,
            products[0].Name,
            products[0].Description,
            products[0].Price,
            products[0].StockQuantity,
            products[0].Sku
        );

        const int totalDatabaseCount = 50;

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.GetAllAsync(request.Page, request.PageSize))
            .ReturnsAsync((products, totalDatabaseCount));

        // Act
        PagedResult<ProductResponse> result = await _sut.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Page, result.Page);
        Assert.Equal(request.PageSize, result.PageSize);
        Assert.Equal(totalDatabaseCount, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(expected, result.Items[0]);

        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetAllAsync(request.Page, request.PageSize), Times.Once);
    }

    [Fact(DisplayName = "Execute should throw ArgumentNullException when request is null")]
    public async Task Execute_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Act
        async Task ActAsync() => await _sut.ExecuteAsync(null!);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(ActAsync);
        Assert.Equal("request", exception.ParamName);

        _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<GetAllProductsRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact(DisplayName = "Execute should return empty result when no products found")]
    public async Task Execute_ShouldReturnEmptyResult_WhenNoProductsFound()
    {
        // Arrange
        GetAllProductsRequest request = new(Page: 1, PageSize: 10);

        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(r => r.GetAllAsync(request.Page, request.PageSize))
            .ReturnsAsync(([], 0));

        // Act
        PagedResult<ProductResponse> result = await _sut.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);

        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact(DisplayName = "Execute should throw ValidationException when request is invalid")]
    public async Task Execute_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        // Arrange
        GetAllProductsRequest request = new(Page: 0, PageSize: 10);

        const string expectedKey = nameof(GetAllProductsRequest.Page);
        const string expectedMessage = GetAllProductsValidator.PageMustBePositive;

        ValidationFailure failure = new(expectedKey, expectedMessage);
        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([failure]));

        // Act
        async Task Act() => await _sut.ExecuteAsync(request);

        // Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(Act);
        Assert.Contains(expectedKey, exception.Errors.Keys);
        Assert.Contains(exception.Errors[expectedKey], error => error == expectedMessage);

        _mockValidator.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}