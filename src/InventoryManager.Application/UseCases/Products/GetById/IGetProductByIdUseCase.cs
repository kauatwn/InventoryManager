using InventoryManager.Application.DTOs.Responses;

namespace InventoryManager.Application.UseCases.Products.GetById;

public interface IGetProductByIdUseCase
{
    Task<ProductResponse> ExecuteAsync(Guid id);
}