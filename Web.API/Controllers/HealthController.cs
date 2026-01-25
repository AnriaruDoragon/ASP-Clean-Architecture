using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.API.Controllers;

/// <summary>
/// Health check endpoints for monitoring and orchestration.
/// Not versioned - these are infrastructure endpoints.
/// </summary>
[ApiController]
[Route("health")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    /// <summary>
    /// Liveness probe - is the application running?
    /// </summary>
    [HttpGet("live")]
    public IActionResult Live() => Ok(new { status = "Healthy" });

    /// <summary>
    /// Readiness probe - is the application ready to serve traffic?
    /// Checks database connectivity and other dependencies.
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        HealthReport report = await healthCheckService.CheckHealthAsync(
            predicate: check => check.Tags.Contains("ready"),
            cancellationToken: cancellationToken);

        var response = new HealthResponse
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration.TotalMilliseconds,
            Entries = report.Entries.Select(e => new HealthEntryResponse
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration.TotalMilliseconds,
                Description = e.Value.Description,
                Error = e.Value.Exception?.Message
            }).ToList()
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}

/// <summary>
/// Health check response model.
/// </summary>
public sealed class HealthResponse
{
    public required string Status { get; init; }
    public double Duration { get; init; }
    public List<HealthEntryResponse> Entries { get; init; } = [];
}

/// <summary>
/// Individual health check entry response.
/// </summary>
public sealed class HealthEntryResponse
{
    public required string Name { get; init; }
    public required string Status { get; init; }
    public double Duration { get; init; }
    public string? Description { get; init; }
    public string? Error { get; init; }
}
