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
using Web.API.Models;

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
    [EndpointSummary("List products")]
    [ProducesResponseType<PagedList<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] GetProductsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        Result<PagedList<ProductResponse>> result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [EndpointSummary("Get product")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        Result<ProductResponse> result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create product")]
    [ProducesResponseType<CreateProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProduct(
        CreateProductRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.StockQuantity);
        Result<Guid> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(id =>
            CreatedAtAction(nameof(GetProduct), new { id }, new CreateProductResponse(id))
        );
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [EndpointSummary("Update product")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price);

        Result result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete product")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken = default)
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
/// Request model for creating a product.
/// </summary>
public sealed record CreateProductRequest(string Name, string? Description, decimal Price, int StockQuantity);

/// <summary>
/// Request model for updating a product.
/// </summary>
/// <remarks>
/// Id is provided via route parameter, not in the request body.
/// </remarks>
public sealed record UpdateProductRequest(string Name, string? Description, decimal Price);
