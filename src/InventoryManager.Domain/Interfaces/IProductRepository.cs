using InventoryManager.Domain.Entities;

namespace InventoryManager.Domain.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task<(List<Product> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<bool> ExistsAsync(string sku);
    Task<bool> IsSkuUniqueAsync(string sku, Guid ignoreProductId);
}