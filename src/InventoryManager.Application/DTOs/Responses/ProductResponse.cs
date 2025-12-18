namespace InventoryManager.Application.DTOs.Responses;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Sku
);