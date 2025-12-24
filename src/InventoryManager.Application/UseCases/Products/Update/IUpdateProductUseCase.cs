using InventoryManager.Application.DTOs.Requests;

namespace InventoryManager.Application.UseCases.Products.Update;

public interface IUpdateProductUseCase
{
    Task ExecuteAsync(Guid id, UpdateProductRequest request);
}