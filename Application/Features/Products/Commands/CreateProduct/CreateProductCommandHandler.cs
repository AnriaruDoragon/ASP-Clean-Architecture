using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Features.Products.Commands.CreateProduct;

/// <summary>
/// Handler for CreateProductCommand.
/// </summary>
public sealed class CreateProductCommandHandler(
    IApplicationDbContext context) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
