namespace Application.Features.Products;

/// <summary>
/// Response model for Product.
/// </summary>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt
);
