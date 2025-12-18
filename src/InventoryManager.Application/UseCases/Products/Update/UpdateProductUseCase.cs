using FluentValidation;
using FluentValidation.Results;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ValidationException = InventoryManager.Domain.Exceptions.ValidationException;

namespace InventoryManager.Application.UseCases.Products.Update;

public sealed partial class UpdateProductUseCase(
    IProductRepository repository,
    IValidator<UpdateProductRequest> validator,
    ILogger<UpdateProductUseCase> logger) : IUpdateProductUseCase
{
    public const string ProductNotFoundMessage = "Product with Id '{0}' not found.";
    public const string SkuAlreadyExistsMessage = "Product with SKU {0} already exists.";

    public void Execute(Guid id, UpdateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        LogUpdatingProduct(id);

        ValidationResult result = validator.Validate(request);
        if (!result.IsValid)
        {
            Dictionary<string, string[]> errors = result.Errors
                .ToLookup(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray());

            throw new ValidationException(errors);
        }

        Product product = repository.GetById(id)
            ?? throw new NotFoundException(string.Format(ProductNotFoundMessage, id));

        if (!repository.IsSkuUnique(request.Sku, id))
        {
            throw new ConflictException(string.Format(SkuAlreadyExistsMessage, request.Sku));
        }

        product.Update(request.Name, request.Description, request.Price, request.StockQuantity, request.Sku);
        repository.Update(product);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating product {Id}")]
    private partial void LogUpdatingProduct(Guid id);
}