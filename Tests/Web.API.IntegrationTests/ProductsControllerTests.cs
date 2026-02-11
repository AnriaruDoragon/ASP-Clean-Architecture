using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Web.API.IntegrationTests;

/// <summary>
/// Integration tests for the Products API endpoints.
/// </summary>
public class ProductsControllerTests(WebApiFactory factory) : IClassFixture<WebApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetProducts_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/Products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProduct_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var createRequest = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            StockQuantity = 10,
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/Products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
