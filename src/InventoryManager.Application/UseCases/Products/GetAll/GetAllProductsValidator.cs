using FluentValidation;
using InventoryManager.Application.DTOs.Requests;

namespace InventoryManager.Application.UseCases.Products.GetAll;

public sealed class GetAllProductsValidator : AbstractValidator<GetAllProductsRequest>
{
    public const string PageMustBePositive = "Page must be at least 1.";
    public const string PageSizeMustBePositive = "Page size must be at least 1.";
    public const string PageSizeLimitExceeded = "Page size must not exceed 50.";

    public GetAllProductsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage(PageMustBePositive);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage(PageSizeMustBePositive)
            .LessThanOrEqualTo(50).WithMessage(PageSizeLimitExceeded);
    }
}