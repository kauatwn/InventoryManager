using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;

namespace InventoryManager.Application.UseCases.Products.Create;

public interface ICreateProductUseCase
{
    ProductResponse Execute(CreateProductRequest request);
}