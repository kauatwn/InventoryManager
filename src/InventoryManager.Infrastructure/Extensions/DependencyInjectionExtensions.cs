using InventoryManager.Domain.Interfaces;
using InventoryManager.Infrastructure.Persistence;
using InventoryManager.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace InventoryManager.Infrastructure.Extensions;

[ExcludeFromCodeCoverage(Justification = "Pure dependency injection configuration")]
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        AddPersistence(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddDbContext<InventoryDbContext>(options => options.UseInMemoryDatabase("InventoryManagerDb"));

        services.AddScoped<IProductRepository, ProductRepository>();
    }
}