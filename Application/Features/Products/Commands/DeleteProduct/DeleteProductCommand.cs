using Application.Common.Messaging;

namespace Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product.
/// </summary>
public sealed record DeleteProductCommand(Guid Id) : ICommand;
