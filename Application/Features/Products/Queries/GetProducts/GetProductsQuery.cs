using Application.Common.Messaging;
using Application.Common.Models;

namespace Application.Features.Products.Queries.GetProducts;

/// <summary>
/// Query to get a paginated list of products.
/// </summary>
public sealed record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsActive = null,
    string? SearchTerm = null) : IQuery<PagedList<ProductDto>>;
