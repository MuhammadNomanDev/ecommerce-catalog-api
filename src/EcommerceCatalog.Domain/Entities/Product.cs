using EcommerceCatalog.Domain.Common;

namespace EcommerceCatalog.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF Core constructor
    private Product() { }

    public static Product Create(string name, string description, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name is required.");
        if (price < 0) throw new ArgumentException("Price cannot be negative.");
        if (stockQuantity < 0) throw new ArgumentException("Stock quantity cannot be negative.");

        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity
        };
    }

    public void Update(string name, string description, decimal price, int stockQuantity)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        SetUpdated();
    }

    public void SetImageUrl(string imageUrl)
    {
        ImageUrl = imageUrl;
        SetUpdated();
    }

    public void Deactivate() => IsActive = false;
}
