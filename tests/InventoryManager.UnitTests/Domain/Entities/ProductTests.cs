using InventoryManager.Domain.Entities;
using InventoryManager.Domain.Exceptions;

namespace InventoryManager.UnitTests.Domain.Entities;

[Trait("Category", "Unit")]
public class ProductTests
{
    [Fact(DisplayName = "Constructor should initialize correctly when data is valid")]
    public void Constructor_ShouldInitializeCorrectly_WhenDataIsValid()
    {
        // Arrange
        const string name = "Laptop Gamer";
        const string description = "High performance laptop";
        const decimal price = 5000.00m;
        const int stock = 10;
        const string sku = "NB-001";

        // Act
        Product product = new(name, description, price, stock, sku);

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(stock, product.StockQuantity);
        Assert.Equal(sku, product.Sku);
    }

    [Theory(DisplayName = "Constructor should throw exception when name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowDomainException_WhenNameIsEmpty(string invalidName)
    {
        // Act
        void Act() => _ = new Product(name: invalidName, description: "Desc", price: 100m, stockQuantity: 10, sku: "SKU-123");

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Product.NameCannotBeEmpty, exception.Message);
    }

    [Theory(DisplayName = "Constructor should throw exception when price is invalid")]
    [InlineData(0)]
    [InlineData(-10.50)]
    public void Constructor_ShouldThrowDomainException_WhenPriceIsInvalid(decimal invalidPrice)
    {
        // Act
        void Act() => _ = new Product(name: "Laptop", description: "Desc", price: invalidPrice, stockQuantity: 10, sku: "SKU-123");

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Product.PriceMustBePositive, exception.Message);
    }

    [Fact(DisplayName = "Constructor should throw exception when stock is negative")]
    public void Constructor_ShouldThrowDomainException_WhenStockIsNegative()
    {
        // Act
        static void Act() => _ = new Product(name: "Laptop", description: "Desc", price: 100m, stockQuantity: -1, sku: "SKU-123");

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Product.StockCannotBeNegative, exception.Message);
    }

    [Fact(DisplayName = "Constructor should throw exception when SKU is empty")]
    public void Constructor_ShouldThrowDomainException_WhenSkuIsEmpty()
    {
        // Act
        static void Act() => _ = new Product(name: "Laptop", description: "Desc", price: 100m, stockQuantity: 10, sku: string.Empty);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Product.SkuCannotBeEmpty, exception.Message);
    }

    [Fact(DisplayName = "Update should modify product values when data is valid")]
    public void Update_ShouldModifyProductValues_WhenDataIsValid()
    {
        // Arrange
        Product product = new(name: "Old Name", description: "Old Desc", price: 100m, stockQuantity: 5, sku: "OLD-SKU");

        // Act
        product.Update(name: "New Name", description: "New Desc", price: 200m, stockQuantity: 10, sku: "NEW-SKU");

        // Assert
        Assert.Equal("New Name", product.Name);
        Assert.Equal("New Desc", product.Description);
        Assert.Equal(200m, product.Price);
        Assert.Equal(10, product.StockQuantity);
        Assert.Equal("NEW-SKU", product.Sku);
    }

    [Fact(DisplayName = "AddStock should increase quantity when value is positive")]
    public void AddStock_ShouldIncreaseQuantity_WhenValueIsPositive()
    {
        // Arrange
        Product product = new(name: "Item", description: "Desc", price: 100m, stockQuantity: 10, sku: "SKU");

        // Act
        product.AddStock(5);

        // Assert
        Assert.Equal(15, product.StockQuantity);
    }

    [Theory(DisplayName = "AddStock should throw exception when quantity is invalid")]
    [InlineData(0)]
    [InlineData(-5)]
    public void AddStock_ShouldThrowDomainException_WhenQuantityIsInvalid(int invalidQuantity)
    {
        // Arrange
        Product product = new(name: "Item", description: "Desc", price: 100m, stockQuantity: 10, sku: "SKU");

        // Act
        void Act() => product.AddStock(invalidQuantity);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Product.QuantityMustBePositive, exception.Message);
    }

    [Fact(DisplayName = "RemoveStock should decrease quantity when stock is sufficient")]
    public void RemoveStock_ShouldDecreaseQuantity_WhenStockIsSufficient()
    {
        // Arrange
        Product product = new(name: "Item", description: "Desc", price: 100m, stockQuantity: 10, sku: "SKU");

        // Act
        product.RemoveStock(4);

        // Assert
        Assert.Equal(6, product.StockQuantity);
    }

    [Fact(DisplayName = "RemoveStock should throw exception when stock is insufficient")]
    public void RemoveStock_ShouldThrowDomainException_WhenStockIsInsufficient()
    {
        // Arrange
        Product product = new(name: "Item", description: "Desc", price: 100m, stockQuantity: 5, sku: "SKU");

        // Act
        void Act() => product.RemoveStock(6);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Product.InsufficientStock, exception.Message);
    }

    [Theory(DisplayName = "RemoveStock should throw exception when quantity is invalid")]
    [InlineData(0)]
    [InlineData(-5)]
    public void RemoveStock_ShouldThrowDomainException_WhenQuantityIsInvalid(int invalidQuantity)
    {
        // Arrange
        Product product = new(name: "Item", description: "Desc", price: 100m, stockQuantity: 10, sku: "SKU");

        // Act
        void Act() => product.RemoveStock(invalidQuantity);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Product.QuantityMustBePositive, exception.Message);
    }
}