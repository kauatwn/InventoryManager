using FluentValidation.TestHelper;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.UseCases.Products.GetAll;

namespace InventoryManager.UnitTests.Application.UseCases.Products.GetAll;

[Trait("Category", "Unit")]
public class GetAllProductsValidatorTests
{
    private readonly GetAllProductsValidator _sut = new();

    [Fact(DisplayName = "Validate should not have error when pagination parameters are valid")]
    public void Validate_ShouldNotHaveError_WhenPaginationParametersAreValid()
    {
        // Arrange
        GetAllProductsRequest request = new(Page: 1, PageSize: 20);

        // Act
        TestValidationResult<GetAllProductsRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = "Validate should return error when Page is invalid")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldReturnError_WhenPageIsInvalid(int invalidPage)
    {
        // Arrange
        GetAllProductsRequest request = new(Page: invalidPage, PageSize: 10);

        // Act
        TestValidationResult<GetAllProductsRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage(GetAllProductsValidator.PageMustBePositive);
    }

    [Theory(DisplayName = "Validate should return error when PageSize is invalid")]
    [InlineData(0, GetAllProductsValidator.PageSizeMustBePositive)]
    [InlineData(-5, GetAllProductsValidator.PageSizeMustBePositive)]
    [InlineData(51, GetAllProductsValidator.PageSizeLimitExceeded)]
    [InlineData(100, GetAllProductsValidator.PageSizeLimitExceeded)]
    public void Validate_ShouldReturnError_WhenPageSizeIsInvalid(int invalidPageSize, string expectedMessage)
    {
        // Arrange
        GetAllProductsRequest request = new(Page: 1, PageSize: invalidPageSize);

        // Act
        TestValidationResult<GetAllProductsRequest> result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(expectedMessage);
    }
}