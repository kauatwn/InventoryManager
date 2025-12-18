using FluentValidation;
using InventoryManager.Application.DTOs.Requests;

namespace InventoryManager.Application.UseCases.Products.Update;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public const string NameRequired = "Name is required.";
    public const string NameMaxLength = "Name must not exceed 100 characters.";
    public const string PriceGreaterThanZero = "Price must be greater than zero.";
    public const string StockCannotBeNegative = "Stock quantity cannot be negative.";
    public const string SkuRequired = "SKU is required.";
    public const string SkuLength = "SKU must be between 5 and 20 characters.";

    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(NameRequired)
            .MaximumLength(100).WithMessage(NameMaxLength);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage(PriceGreaterThanZero);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage(StockCannotBeNegative);

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage(SkuRequired)
            .Length(5, 20).WithMessage(SkuLength);
    }
}