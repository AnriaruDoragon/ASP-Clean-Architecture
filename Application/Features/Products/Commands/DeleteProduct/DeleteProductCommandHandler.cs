using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handler for DeleteProductCommand.
/// </summary>
public sealed class DeleteProductCommandHandler(
    IApplicationDbContext context) : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
            return Result.Failure(Error.NotFound("Product", request.Id));

        // Soft delete - the product will be filtered out by the global query filter
        product.Delete();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
