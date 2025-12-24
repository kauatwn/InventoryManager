using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(InventoryDbContext context) : IProductRepository
{
    public async Task AddAsync(Product product)
    {
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await context.Products.FindAsync(id);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        IQueryable<Product> query = context.Products.AsNoTracking();
        int totalCount = await query.CountAsync();

        List<Product> items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> ExistsAsync(string sku)
    {
        return await context.Products.AnyAsync(p => p.Sku == sku);
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid ignoreProductId)
    {
        return !await context.Products.AnyAsync(p => p.Sku == sku && p.Id != ignoreProductId);
    }
}