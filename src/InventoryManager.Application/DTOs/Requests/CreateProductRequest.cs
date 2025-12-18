namespace InventoryManager.Application.DTOs.Requests;

public sealed record CreateProductRequest(string Name, string Description, decimal Price, int StockQuantity, string Sku);