using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;
using InventoryManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManager.Application.UseCases.Products.Delete;

public sealed partial class DeleteProductUseCase(
    IProductRepository repository,
    ILogger<DeleteProductUseCase> logger) : IDeleteProductUseCase
{
    public const string ProductNotFoundMessage = "Product with Id '{0}' not found.";

    public void Execute(Guid id)
    {
        LogDeletingProduct(id);

        Product product = repository.GetById(id) ?? throw new NotFoundException(string.Format(ProductNotFoundMessage, id));

        repository.Delete(product);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting product {Id}")]
    private partial void LogDeletingProduct(Guid id);
}