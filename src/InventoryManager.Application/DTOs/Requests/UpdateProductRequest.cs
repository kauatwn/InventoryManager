namespace InventoryManager.Application.DTOs.Requests;

public sealed record UpdateProductRequest(string Name, string Description, decimal Price, int StockQuantity, string Sku);