# Configuration

All settings can be configured via environment variables or `appsettings.json`. Environment variables override `appsettings.json`. See `.env.example` for all available variables.

## CORS

```json
"Cors": {
  "AllowedOrigins": ["https://yourdomain.com", "https://*.yourdomain.com"]
}
```

Or via environment variable:
```bash
CORS__ALLOWEDORIGINS=https://yourdomain.com,https://*.yourdomain.com
```

**Wildcard support:** `https://*.example.com` matches any subdomain.

**Behavior:**
- With origins configured: Strict CORS with credentials support
- In Development without origins: Allow any origin (no credentials)

## Caching

Optional response caching with Memory or Redis backends.

```json
"Caching": {
  "Enabled": false,
  "Provider": "Memory",
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "DefaultExpirationMinutes": 5
}
```

**Providers:**
- `Memory` — In-process memory cache (default, single instance only)
- `Redis` — Distributed cache (multiple instances)
- `None` / disabled — No-op (zero overhead)

**Usage in queries:**
```csharp
public sealed record GetProductByIdQuery(Guid Id)
    : IQuery<ProductResponse>, ICacheableQuery
{
    public string CacheKey => $"products:{Id}";
}
```

Implement `ICacheableQuery` and `CachingBehavior` handles the rest.

## Rate Limiting

```json
"RateLimiting": {
  "Global": { "PermitLimit": 100, "WindowMinutes": 1 },
  "Policies": {
    "Auth":    { "Limit": 5,  "Window": 60 },
    "Api":     { "Limit": 30, "Window": 60 },
    "Strict":  { "Limit": 10, "Window": 60 },
    "Relaxed": { "Limit": 60, "Window": 60 }
  }
}
```

- **Global limiter** — Applies to all requests per IP
- **Named policies** — Apply to endpoints with `[RateLimit("Api")]` or `[RateLimit("Auth", Per.User)]`
- **Partitioning** — Per IP (default), per User, or per Session

When rate limited, returns `429 Too Many Requests`.

## Background Jobs

```json
"BackgroundJobs": {
  "Provider": "Instant"
}
```

**Providers:**
- `Instant` — Fire-and-forget after response (default, good for development)
- `Memory` — In-memory queue with background worker
- `None` — Disabled, jobs are dropped

**Usage:**
```csharp
public class MyHandler(IBackgroundJobService jobs)
{
    public async Task Handle(...)
    {
        await jobs.EnqueueAsync(new SendWelcomeEmailCommand(userId));
        await jobs.ScheduleAsync(new CleanupCommand(), TimeSpan.FromHours(1));
    }
}
```

**For production**, replace with [Hangfire](https://www.hangfire.io/) (persistence, retries, dashboard) or message queues (RabbitMQ, Azure Service Bus) for distributed scenarios.

## Health Checks

| Endpoint        | Description                                     |
|-----------------|-------------------------------------------------|
| `/health/live`  | Liveness probe — is the app running?            |
| `/health/ready` | Readiness probe — is the app ready for traffic? |

The readiness check includes database connectivity.

```bash
curl http://localhost:5141/health/ready
```

## Observability

### Serilog Structured Logging

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Correlation ID

Every request gets a unique correlation ID for distributed tracing:

- **Header**: `X-Correlation-ID`
- Reads from request header or generates new GUID
- Included in all logs via Serilog LogContext
- Returned in response headers
- Included in error responses (ProblemDetails)

```bash
curl -H "X-Correlation-ID: my-trace-123" http://localhost:5141/Products
```

### Request Logging

Debug-level logging of HTTP requests/responses:
- Logs method, path, status code, and timing
- Excludes health check endpoints
- Redacts sensitive headers (Authorization, Cookie, API keys)

Enable by setting Serilog minimum level to `Debug`.

### OpenTelemetry

For production observability, add OpenTelemetry instrumentation:

```xml
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.11.2" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.11.1" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.10.0-beta.1" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.11.2" />
```

See [OpenTelemetry .NET documentation](https://opentelemetry.io/docs/languages/net/) for setup.

## API Versioning

Header-based versioning (`x-api-version`) with namespace conventions (`Controllers.V1` -> version 1).

| Endpoint                  | Description                         |
|---------------------------|-------------------------------------|
| `/openapi/{version}.json` | OpenAPI 3.0 specification           |
| `/scalar`                 | Scalar interactive documentation UI |

For complete documentation, see [Common.ApiVersioning/README.md](../Common.ApiVersioning/README.md).
