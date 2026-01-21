using Application.Common.Messaging;

namespace Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Command to update an existing product.
/// </summary>
public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price) : ICommand;
