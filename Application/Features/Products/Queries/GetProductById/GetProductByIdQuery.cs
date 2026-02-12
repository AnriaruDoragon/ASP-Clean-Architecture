using Application.Common.Interfaces;
using Application.Common.Messaging;

namespace Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Query to get a product by its ID.
/// Implements ICacheableQuery for automatic response caching.
/// </summary>
public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>, ICacheableQuery
{
    public string CacheKey => $"products:{Id}";
}
