using InventoryManager.Domain.Entities;
using InventoryManager.Infrastructure.Persistence;
using InventoryManager.IntegrationTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManager.IntegrationTests.Helpers;

public class IntegrationTestSeeder(IntegrationTestWebAppFactory factory)
{
    private const int MaxSkuLength = 20;

    public async Task<Product> CreateProductAsync(string name = "Default Name", decimal price = 10m, int stock = 10, string? sku = null)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        string finalSku = sku ?? $"SKU-{Guid.NewGuid():N}"[..MaxSkuLength];

        Product product = new(name, "Default Desc", price, stock, finalSku);

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product;
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        return await context.Products.FindAsync(id);
    }
}