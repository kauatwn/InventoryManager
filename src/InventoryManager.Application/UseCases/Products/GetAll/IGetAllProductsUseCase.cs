using InventoryManager.Application.DTOs.Common;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;

namespace InventoryManager.Application.UseCases.Products.GetAll;

public interface IGetAllProductsUseCase
{
    PagedResult<ProductResponse> Execute(GetAllProductsRequest request);
}