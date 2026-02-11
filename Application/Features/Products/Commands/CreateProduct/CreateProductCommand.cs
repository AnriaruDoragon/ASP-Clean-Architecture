using Application.Common.Messaging;

namespace Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Command to create a new product.
/// </summary>
public sealed record CreateProductCommand(string Name, string? Description, decimal Price, int StockQuantity)
    : ICommand<Guid>;
