using FluentValidation.TestHelper;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.UseCases.Products.Create;

namespace InventoryManager.UnitTests.Application.UseCases.Products.Create;

[Trait("Category", "Unit")]
public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _sut = new();

    [Fact(DisplayName = "Validate should not have error when request is valid")]
    public void Validate_ShouldNotHaveError_WhenRequestIsValid()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "Gamer Mouse",
            Description: "High DPI",
            Price: 150.00m,
            StockQuantity: 10,
            Sku: "MSE-001"
        );

        // Act
        TestValidationResult<CreateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = "Validate should return error when Name is invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldReturnError_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        CreateProductRequest request = new(
            Name: invalidName!,
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<CreateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(CreateProductValidator.NameRequired);
    }

    [Fact(DisplayName = "Validate should return error when Name exceeds max length")]
    public void Validate_ShouldReturnError_WhenNameExceedsMaxLength()
    {
        // Arrange
        string longName = new('A', 101);
        CreateProductRequest request = new(
            Name: longName,
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<CreateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(CreateProductValidator.NameTooLong);
    }

    [Theory(DisplayName = "Validate should return error when Price is invalid")]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Validate_ShouldReturnError_WhenPriceIsInvalid(decimal invalidPrice)
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "Mouse",
            Description: "Desc",
            Price: invalidPrice,
            StockQuantity: 10,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<CreateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage(CreateProductValidator.PriceMustBePositive);
    }

    [Fact(DisplayName = "Validate should return error when StockQuantity is negative")]
    public void Validate_ShouldReturnError_WhenStockQuantityIsNegative()
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "Mouse",
            Description: "Desc",
            Price: 100m,
            StockQuantity: -1,
            Sku: "SKU-123"
        );

        // Act
        TestValidationResult<CreateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity)
            .WithErrorMessage(CreateProductValidator.StockCannotBeNegative);
    }

    [Theory(DisplayName = "Validate should return error when SKU is invalid")]
    [InlineData("", CreateProductValidator.SkuRequired)]
    [InlineData("ABC", CreateProductValidator.SkuLengthInvalid)]
    [InlineData("A-VERY-LONG-SKU-THAT-EXCEEDS-LIMIT", CreateProductValidator.SkuLengthInvalid)]
    public void Validate_ShouldReturnError_WhenSkuIsInvalid(string invalidSku, string expectedMessage)
    {
        // Arrange
        CreateProductRequest request = new(
            Name: "Mouse",
            Description: "Desc",
            Price: 100m,
            StockQuantity: 10,
            Sku: invalidSku
        );

        // Act
        TestValidationResult<CreateProductRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage(expectedMessage);
    }
}