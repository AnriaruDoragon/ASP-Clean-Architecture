using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Handler for GetProductByIdQuery.
/// </summary>
public sealed class GetProductByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        ProductResponse? product = await context
            .Products.AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.IsActive,
                p.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return product ?? Result.Failure<ProductResponse>(Error.NotFound("Product", request.Id));
    }
}
