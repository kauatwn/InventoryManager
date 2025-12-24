using InventoryManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasData(
            new Product("iPhone 15 Pro", "Apple Smartphone 256GB Titanium", 7500m, 10, "SKU-IPHONE-15P"),
            new Product("Samsung Galaxy S24", "Samsung Smartphone AI Features", 4500m, 15, "SKU-GALAXY-S24"),
            new Product("Xiaomi 13T", "Xiaomi Flagship Killer", 3200m, 20, "SKU-XIAOMI-13T"),
            new Product("Motorola Edge 40", "Smartphone intermediário premium", 2100m, 12, "SKU-MOTO-EDGE40"),

            new Product("MacBook Pro M3", "Notebook Apple Apple Silicon", 12000m, 5, "SKU-MAC-M3"),
            new Product("Dell XPS 13", "Ultrabook Windows Premium", 9000m, 7, "SKU-DELL-XPS"),

            new Product("Mouse Logitech MX Master", "Mouse ergonômico wireless", 450m, 50, "SKU-MOUSE-MX"),
            new Product("Teclado Mecânico Keychron", "Teclado mecânico switch brown", 600m, 25, "SKU-KEYCHRON-K2"),
            new Product("Monitor Dell 27 4K", "Monitor UHD USB-C Hub", 2800m, 10, "SKU-DELL-27-4K"),
            new Product("Webcam Logitech C920", "Webcam Full HD Pro", 350m, 40, "SKU-WEBCAM-C920"),

            new Product("Headset Gamer HyperX", "Fone surround 7.1", 300m, 60, "SKU-HEADSET-HX"),
            new Product("Cadeira Gamer DX", "Cadeira ergonômica reclinável", 1500m, 8, "SKU-CHAIR-DX"),

            new Product("Cabo HDMI 2.1", "Cabo 8K Ultra Speed", 80m, 200, "SKU-CABLE-HDMI"),
            new Product("Hub USB-C 7-in-1", "Adaptador para MacBook e Windows", 250m, 45, "SKU-HUB-USBC")
        );
    }
}