namespace InventoryManager.Application.DTOs.Requests;

public sealed record GetAllProductsRequest(int Page = 1, int PageSize = 10);