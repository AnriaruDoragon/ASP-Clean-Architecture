namespace Application.Features.Products;

/// <summary>
/// Data transfer object for Product.
/// </summary>
public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt);
