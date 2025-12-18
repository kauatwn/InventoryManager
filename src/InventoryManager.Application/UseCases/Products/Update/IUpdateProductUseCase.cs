using InventoryManager.Application.DTOs.Requests;

namespace InventoryManager.Application.UseCases.Products.Update;

public interface IUpdateProductUseCase
{
    void Execute(Guid id, UpdateProductRequest request);
}