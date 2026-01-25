using System.Net;
using FluentAssertions;
using Xunit;

namespace Web.API.IntegrationTests;

/// <summary>
/// Integration tests for health check endpoints.
/// </summary>
public class HealthCheckTests(WebApiFactory factory) : IClassFixture<WebApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LivenessCheck_ShouldReturnHealthy()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessCheck_ShouldReturnHealthy()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
