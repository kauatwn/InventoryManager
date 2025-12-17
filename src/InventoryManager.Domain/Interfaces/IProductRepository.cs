using InventoryManager.Domain.Entities;

namespace InventoryManager.Domain.Interfaces;

public interface IProductRepository
{
    void Add(Product product);
    Product? GetById(Guid id);
    (List<Product> Items, int TotalCount) GetAll(int page, int pageSize);
    void Update(Product product);
    void Delete(Product product);
    bool Exists(string sku);
    bool IsSkuUnique(string sku, Guid ignoreProductId);
}