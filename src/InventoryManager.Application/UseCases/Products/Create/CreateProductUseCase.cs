using FluentValidation;
using FluentValidation.Results;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ValidationException = InventoryManager.Domain.Exceptions.ValidationException;

namespace InventoryManager.Application.UseCases.Products.Create;

public sealed partial class CreateProductUseCase(
    IProductRepository repository,
    IValidator<CreateProductRequest> validator,
    ILogger<CreateProductUseCase> logger) : ICreateProductUseCase
{
    public const string SkuAlreadyExistsMessage = "Product with SKU {0} already exists.";

    public async Task<ProductResponse> ExecuteAsync(CreateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        LogExecution(request.Name);

        ValidationResult result = await validator.ValidateAsync(request);
        if (!result.IsValid)
        {
            Dictionary<string, string[]> errors = result.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());

            throw new ValidationException(errors);
        }

        if (await repository.ExistsAsync(request.Sku))
        {
            throw new ConflictException(string.Format(SkuAlreadyExistsMessage, request.Sku));
        }

        Product product = new(request.Name, request.Description, request.Price, request.StockQuantity, request.Sku);
        await repository.AddAsync(product);

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.Sku);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Executing create product use case for: {Name}")]
    private partial void LogExecution(string name);
}