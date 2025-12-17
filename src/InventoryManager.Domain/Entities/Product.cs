using InventoryManager.Domain.Common;
using InventoryManager.Domain.Exceptions;

namespace InventoryManager.Domain.Entities;

public class Product : IAggregateRoot
{
    public const string PriceMustBePositive = "Price must be greater than zero.";
    public const string QuantityMustBePositive = "Quantity must be positive.";
    public const string InsufficientStock = "Insufficient stock.";
    public const string StockCannotBeNegative = "Stock quantity cannot be negative.";
    public const string NameCannotBeEmpty = "Name cannot be empty.";
    public const string SkuCannotBeEmpty = "SKU cannot be empty.";

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string Sku { get; private set; }

    public Product(string name, string description, decimal price, int stockQuantity, string sku)
    {
        ValidateDomain(name, price, stockQuantity, sku);

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        Sku = sku;
    }

    public void Update(string name, string description, decimal price, int stockQuantity, string sku)
    {
        ValidateDomain(name, price, stockQuantity, sku);

        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        Sku = sku;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException(QuantityMustBePositive);
        }

        StockQuantity += quantity;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException(QuantityMustBePositive);
        }

        if (StockQuantity - quantity < 0)
        {
            throw new DomainException(InsufficientStock);
        }

        StockQuantity -= quantity;
    }

    private static void ValidateDomain(string name, decimal price, int stock, string sku)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(NameCannotBeEmpty);
        }

        if (price <= 0)
        {
            throw new DomainException(PriceMustBePositive);
        }

        if (stock < 0)
        {
            throw new DomainException(StockCannotBeNegative);
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException(SkuCannotBeEmpty);
        }
    }
}