using Application.Common.Models;
using Application.Features.Products;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Web.API.Extensions;

namespace Web.API.Controllers.V1;

/// <summary>
/// Example controller demonstrating CQRS pattern with MediatR.
/// </summary>
[ApiController]
[Route("[controller]")]
[EnableRateLimiting("endpoint")]
[RateLimit("Api", Per.User)]
public class ProductsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Gets a paginated list of products.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PagedList<ProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(pageNumber, pageSize, isActive, searchTerm);
        Result<PagedList<ProductDto>> result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        Result<ProductDto> result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CreateProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProduct(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        Result<Guid> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(id => CreatedAtAction(
            nameof(GetProduct),
            new { id },
            new CreateProductResponse(id)));
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price);

        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductCommand(id);
        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}

/// <summary>
/// Response returned when a product is created.
/// </summary>
public sealed record CreateProductResponse(Guid Id);

/// <summary>
/// Request model for updating a product.
/// </summary>
/// <remarks>
/// Id is provided via route parameter, not in the request body.
/// </remarks>
public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price);
