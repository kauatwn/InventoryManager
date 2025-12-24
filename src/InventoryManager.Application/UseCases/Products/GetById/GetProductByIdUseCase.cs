using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManager.Application.UseCases.Products.GetById;

public sealed partial class GetProductByIdUseCase(
    IProductRepository repository,
    ILogger<GetProductByIdUseCase> logger) : IGetProductByIdUseCase
{
    public const string ProductNotFoundMessage = "Product with Id '{0}' not found.";

    public async Task<ProductResponse> ExecuteAsync(Guid id)
    {
        LogExecution(id);

        Product product = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException(string.Format(ProductNotFoundMessage, id));

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.Sku
        );
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching product with Id: {Id}")]
    private partial void LogExecution(Guid id);
}