using FluentValidation;
using InventoryManager.Application.DTOs.Requests;

namespace InventoryManager.Application.UseCases.Products.Create;

public sealed class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public const string NameRequired = "Name is required.";
    public const string NameTooLong = "Name must not exceed 100 characters.";
    public const string PriceMustBePositive = "Price must be greater than zero.";
    public const string StockCannotBeNegative = "Stock quantity cannot be negative.";
    public const string SkuRequired = "SKU is required.";
    public const string SkuLengthInvalid = "SKU must be between 5 and 20 characters.";

    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(NameRequired)
            .MaximumLength(100).WithMessage(NameTooLong);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage(PriceMustBePositive);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage(StockCannotBeNegative);

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage(SkuRequired)
            .Length(5, 20).WithMessage(SkuLengthInvalid);
    }
}