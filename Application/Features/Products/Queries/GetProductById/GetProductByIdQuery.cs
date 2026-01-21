using Application.Common.Messaging;

namespace Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Query to get a product by its ID.
/// </summary>
public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
