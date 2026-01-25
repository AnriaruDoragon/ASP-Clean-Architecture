using Domain.Common;
using Domain.Events;
using Domain.Exceptions;

namespace Domain.Entities;

/// <summary>
/// Example entity demonstrating Clean Architecture patterns.
/// </summary>
public class Product : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Optimistic concurrency
    public byte[] RowVersion { get; private set; } = [];

    // Private constructor for EF Core
    private Product() { }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    public static Product Create(string name, string? description, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty.");

        if (price < 0)
            throw new DomainException("Product price cannot be negative.");

        if (stockQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");

        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = true
        };

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name));

        return product;
    }

    /// <summary>
    /// Updates product details.
    /// </summary>
    public void Update(string name, string? description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty.");

        if (price < 0)
            throw new DomainException("Product price cannot be negative.");

        Name = name;
        Description = description;
        Price = price;
    }

    /// <summary>
    /// Adjusts stock quantity.
    /// </summary>
    public void AdjustStock(int quantity)
    {
        int newQuantity = StockQuantity + quantity;

        if (newQuantity < 0)
            throw new DomainException("Insufficient stock.");

        StockQuantity = newQuantity;

        if (StockQuantity == 0)
            AddDomainEvent(new ProductOutOfStockEvent(Id, Name));
    }

    /// <summary>
    /// Deactivates the product.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Activates the product.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Soft deletes the product.
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductDeletedEvent(Id, Name));
    }

    /// <summary>
    /// Restores a soft-deleted product.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
