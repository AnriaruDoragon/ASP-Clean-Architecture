using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Handler for GetProductByIdQuery.
/// </summary>
public sealed class GetProductByIdQueryHandler(
    IApplicationDbContext context) : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        ProductDto? product = await context.Products
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.IsActive,
                p.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return product ?? Result.Failure<ProductDto>(Error.NotFound("Product", request.Id));
    }
}
