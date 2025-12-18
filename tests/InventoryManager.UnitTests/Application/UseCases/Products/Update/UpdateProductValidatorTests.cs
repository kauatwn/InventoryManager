using FluentValidation.TestHelper;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.UseCases.Products.Update;

namespace InventoryManager.UnitTests.Application.UseCases.Products.Update;

[Trait("Category", "Unit")]
public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _sut = new();

    [Fact(DisplayName = "Validate should not have error when request is valid")]
    public void Validate_ShouldNotHaveError_WhenRequestIsValid()
    {
        // Arrange
        UpdateProductRequest request = new(
            Name: "Gamer Mouse",
            Description: "High Precision",
            Price: 150.00m,
            StockQuantity: 10,
            Sku: "MSE-001"
        );

        // Act
        TestValidationResult<UpdateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = "Validate should return error when Name is invalid (Empty/Null)")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldReturnError_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        UpdateProductRequest request = new(
            Name: invalidName!,
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<UpdateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(UpdateProductValidator.NameRequired);
    }

    [Fact(DisplayName = "Validate should return error when Name exceeds max length")]
    public void Validate_ShouldReturnError_WhenNameExceedsMaxLength()
    {
        // Arrange
        string longName = new('A', 101);
        UpdateProductRequest request = new(
            Name: longName,
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<UpdateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(UpdateProductValidator.NameMaxLength);
    }

    [Theory(DisplayName = "Validate should return error when Price is invalid (Zero/Negative)")]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Validate_ShouldReturnError_WhenPriceIsInvalid(decimal invalidPrice)
    {
        // Arrange
        UpdateProductRequest request = new(
            Name: "Mouse",
            Description: "Desc",
            Price: invalidPrice,
            StockQuantity: 10,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<UpdateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage(UpdateProductValidator.PriceGreaterThanZero);
    }

    [Fact(DisplayName = "Validate should return error when StockQuantity is negative")]
    public void Validate_ShouldReturnError_WhenStockQuantityIsNegative()
    {
        // Arrange
        UpdateProductRequest request = new(
            Name: "Mouse",
            Description: "Desc",
            Price: 100m,
            StockQuantity: -1,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<UpdateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity)
            .WithErrorMessage(UpdateProductValidator.StockCannotBeNegative);
    }

    [Fact(DisplayName = "Validate should NOT have error when StockQuantity is zero")]
    public void Validate_ShouldNotHaveError_WhenStockQuantityIsZero()
    {
        UpdateProductRequest request = new(
            Name: "Mouse",
            Description: "Desc",
            Price: 100m,
            StockQuantity: 0,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<UpdateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StockQuantity);
    }

    [Theory(DisplayName = "Validate should return error when SKU is empty")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldReturnError_WhenSkuIsEmpty(string? invalidSku)
    {
        // Arrange
        UpdateProductRequest request = new(
            Name: "Mouse",
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: invalidSku!
        );

        // Act
        TestValidationResult<UpdateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage(UpdateProductValidator.SkuRequired);
    }

    [Theory(DisplayName = "Validate should return error when SKU length is invalid")]
    [InlineData("1234")]
    [InlineData("123456789012345678901")]
    public void Validate_ShouldReturnError_WhenSkuLengthIsInvalid(string invalidSku)
    {
        // Arrange
        UpdateProductRequest request = new(
            Name: "Mouse",
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: invalidSku
        );

        // Act
        TestValidationResult<UpdateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage(UpdateProductValidator.SkuLength);
    }
}