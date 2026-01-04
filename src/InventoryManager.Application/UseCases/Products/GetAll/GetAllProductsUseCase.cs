using FluentValidation;
using FluentValidation.Results;
using InventoryManager.Application.DTOs.Common;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ValidationException = InventoryManager.Domain.Exceptions.ValidationException;

namespace InventoryManager.Application.UseCases.Products.GetAll;

public sealed partial class GetAllProductsUseCase(
    IProductRepository repository,
    IValidator<GetAllProductsRequest> validator,
    ILogger<GetAllProductsUseCase> logger) : IGetAllProductsUseCase
{
    public async Task<PagedResult<ProductResponse>> ExecuteAsync(GetAllProductsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        LogExecution(request.Page, request.PageSize);

        ValidationResult result = await validator.ValidateAsync(request);
        if (!result.IsValid)
        {
            Dictionary<string, string[]> errors = result.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray());

            throw new ValidationException(errors);
        }

        (IReadOnlyCollection<Product> products, int totalCount) = await repository.GetAllAsync(request.Page, request.PageSize);

        List<ProductResponse> items = [.. products
            .Select(p =>
                new ProductResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.Sku
                )
            )
        ];

        return new PagedResult<ProductResponse>(items, totalCount, request.Page, request.PageSize);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting all products. Page: {Page}, PageSize: {PageSize}")]
    private partial void LogExecution(int page, int pageSize);
}