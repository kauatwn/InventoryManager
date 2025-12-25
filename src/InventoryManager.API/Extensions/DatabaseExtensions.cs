using InventoryManager.Infrastructure.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace InventoryManager.API.Extensions;

[ExcludeFromCodeCoverage(Justification = "Infrastructure wrapper for database initialization")]
public static class DatabaseExtensions
{
    public static async Task CreateDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        await context.Database.EnsureCreatedAsync();
    }
}