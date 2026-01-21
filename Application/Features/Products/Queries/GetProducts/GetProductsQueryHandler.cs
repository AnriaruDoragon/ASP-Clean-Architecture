using Application.Common.Interfaces;
using Application.Common.Messaging;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProducts;

/// <summary>
/// Handler for GetProductsQuery.
/// </summary>
public sealed class GetProductsQueryHandler(
    IApplicationDbContext context) : IQueryHandler<GetProductsQuery, PagedList<ProductDto>>
{
    public async Task<Result<PagedList<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = context.Products.AsNoTracking();

        // Apply filters
        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(p => p.Name.Contains(request.SearchTerm));

        // Get total count
        int totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project to DTO
        List<ProductDto> items = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.IsActive,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedList<ProductDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
