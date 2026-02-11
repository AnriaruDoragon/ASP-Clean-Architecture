# ASP.NET Clean Architecture Template

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](compose.yaml)

A production-ready ASP.NET Core 10.0 template implementing Clean Architecture with CQRS pattern.

## Using as Template

### Installation

```bash
# Install from local clone
git clone https://github.com/AnriaruDoragon/ASP-Clean-Architecture
dotnet new install ./ASPCleanArchitecture
```

### Create a New Project

```bash
# Default: all features included (with auto-setup)
dotnet new aspclean -n MyApp --allow-scripts yes

# Without auto-setup (manual post-creation steps)
dotnet new aspclean -n MyApp

# Without Docker files
dotnet new aspclean -n MyApp --IncludeDocker false --allow-scripts yes

# Without example Product feature
dotnet new aspclean -n MyApp --IncludeExamples false --allow-scripts yes

# Minimal: no examples, no docker, no tests
dotnet new aspclean -n MyApp --IncludeExamples false --IncludeDocker false --IncludeTests false --allow-scripts yes
```

> **Note:** `--allow-scripts yes` automatically runs post-creation scripts (restore, copy .env, create migration).
> Without it, you'll be prompted to confirm each action or can run them manually.

### Template Parameters

| Parameter           | Default | Description                   |
|---------------------|---------|-------------------------------|
| `--IncludeExamples` | `true`  | Include Products CRUD example |
| `--IncludeDocker`   | `true`  | Include Docker/compose files  |
| `--IncludeTests`    | `true`  | Include test projects         |

### After Creating a Project

We recommend to install [Taskfile](https://taskfile.dev/docs/installation) to use predefined tasks.

With `--allow-scripts yes`, steps 1-3 run automatically.

```bash
cd MyApp

# 1. Setup environment (auto)
cp .env.example .env

# 2. Restore packages (auto)
task restore

# 3. Create initial migration (auto)
task migration:add -- Init

# 4. (Optional) Setup local HTTPS
task certs:setup:windows  # Windows
task certs:setup          # Linux/Mac

# 5. Start development
task docker:up
```

---

## Quick Start

```bash
# 1. Setup environment
cp .env.example .env

# 2. Start development environment
task docker:up            # Windows/Mac: DB+Traefik, Linux: full stack
task watch                # Run API locally (Windows/Mac)

# Access: http://localhost:5141
```

**Optional: Local HTTPS domain** (production-like setup)
```bash
# Generate HTTPS certificates and configure hosts (requires mkcert)
# Run as admin/sudo to auto-add hosts entry
task certs:setup:windows  # Windows (run as Administrator)
task certs:setup          # Linux/Mac (uses sudo)

# Access: https://api.app.localhost
```

**Optional: Redis caching**
```bash
# Enable Redis in .env
CACHING__PROVIDER=Redis

# Start with Redis profile
docker compose --profile cache up -d
```

## Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Task](https://taskfile.dev/) (task runner)
- [mkcert](https://github.com/FiloSottile/mkcert) *(optional, for local HTTPS)*

### First-Time Setup

1. **Copy environment file and restore tools:**
   ```bash
   cp .env.example .env
   dotnet tool restore
   ```

2. **(Optional) Local HTTPS domain setup:**
   ```bash
   # Generate certificates and configure hosts (requires mkcert)
   # Run as admin/sudo to auto-add hosts entry
   task certs:setup:windows  # Windows (run as Administrator)
   task certs:setup          # Linux/Mac (uses sudo)
   ```

### Development Workflow

**Windows/Mac** (recommended): Run DB + Redis in Docker, .NET locally via IDE:
```bash
task docker:up   # Start infrastructure (DB, Redis, Traefik)
task watch       # Run API with hot-reload
```

**Linux**: Run everything in Docker:
```bash
task docker:up   # Starts full stack including API container
```

**Development Endpoints:**

| Endpoint          | URL                                                 |
|-------------------|-----------------------------------------------------|
| API (direct)      | http://localhost:5141                               |
| API (via Traefik) | https://api.app.localhost *(requires hosts entry)*  |
| Scalar API docs   | http://localhost:5141/scalar/v1                     |
| Traefik Dashboard | http://localhost:8080                               |

### Redis Caching (Optional)

Redis is available but not started by default. To enable:

```bash
# Set in .env
CACHING__PROVIDER=Redis

# Start with cache profile
docker compose --profile cache up -d
```

### Docker Commands

| Command                | Description                                   |
|------------------------|-----------------------------------------------|
| `task docker:up`       | Smart start (Windows/Mac: infra, Linux: full) |
| `task docker:up:infra` | Start infrastructure only                     |
| `task docker:up:full`  | Start full stack with API container           |
| `task docker:down`     | Stop all containers                           |
| `task docker:logs`     | View container logs                           |
| `task docker:clean`    | Remove containers and volumes                 |

## Production Deployment

### Docker Compose

```bash
# Configure production values in .env
task prod:up     # Build and start production stack
task prod:logs   # View logs
task prod:down   # Stop
```

**Production services:**
- **PostgreSQL 18** - Database (internal only)
- **Redis 8** - Distributed caching (internal only, optional)
- **API** - ASP.NET Core application
- **Nginx** - Reverse proxy with HTTPS

### VM/EC2 Deployment

Build a self-contained package for deployment to VMs:

```bash
# Build for Linux x64 (default)
task deploy:build

# Build for different runtime
task deploy:build DEPLOY_RUNTIME=linux-arm64  # ARM64 (AWS Graviton)
task deploy:build DEPLOY_RUNTIME=win-x64      # Windows

# Create deployment package
task deploy:package          # Creates .tar.gz (Linux/Mac)
task deploy:package:windows  # Creates .zip (Windows)
```

**Systemd service example** (`/etc/systemd/system/myapp.service`):
```ini
[Unit]
Description=ASP.NET Clean Architecture API
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/opt/myapp
ExecStart=/opt/myapp/Web.API
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

## Build & Test Commands

```bash
# Build
task build              # Debug build
task build:release      # Release build

# Run
task run                # Run API
task watch              # Run with hot-reload

# Test
task test               # All tests
task test:unit          # Unit tests only
task test:integration   # Integration tests

# EF Core Migrations
task migration:add -- Name    # Create migration
task db:update                # Apply migrations
task db:shell                 # PostgreSQL shell
```

### Taskfile Commands

```bash
# Development
task docker:up          # Start dev environment (smart)
task docker:down        # Stop dev environment
task watch              # Run with hot reload

# Production
task prod:up            # Start production containers
task prod:down          # Stop production containers

# Deploy (VM/EC2)
task deploy:build       # Build self-contained package
task deploy:package     # Create deployment tarball/zip

# Testing
task test               # Run all tests
task test:unit          # Unit tests only
task test:coverage      # Tests with coverage

# Database
task migration:add -- Name    # Create migration
task db:update                # Apply migrations
task db:shell                 # Database shell

# Utilities
task format             # Format code (dotnet format + CSharpier)
task format:check       # Check formatting without changes
task info               # Show environment info
```

View all commands: `task --list-all`

## Architecture

ASP.NET Core 10.0 Clean Architecture with CQRS pattern:

```
Domain/           → Entities, value objects, domain events (no dependencies)
Application/      → CQRS handlers, validators, interfaces (depends on Domain)
Infrastructure/   → EF Core, repositories, external services (depends on Application)
Web.API/          → Controllers, middleware (depends on all layers)
Common.ApiVersioning/ → Shared API versioning library (standalone)
```

**Dependency Flow:** Domain ← Application ← Infrastructure ← Web.API

### Domain Layer

Base classes for entities:
- `BaseEntity` - Id + domain events support
- `AuditableEntity` - Adds CreatedAt/By, ModifiedAt/By
- `IAggregateRoot` - Marker for aggregate roots (only these get repositories)
- `ValueObject` - Base for value objects (equality by value)
- `IDomainEvent` - Domain event interface

### Application Layer

**CQRS with MediatR:**
- Commands: `ICommand`, `ICommand<TResponse>` → `ICommandHandler<T>`
- Queries: `IQuery<TResponse>` → `IQueryHandler<T, TResponse>`
- All handlers return `Result` or `Result<T>` (never throw for expected failures)

**Pipeline Behaviors:**
- `ValidationBehavior` - Auto-validates requests via FluentValidation
- `LoggingBehavior` - Logs request execution and timing
- `CachingBehavior` - Caches query results for `ICacheableQuery` implementations

**Interfaces defined here, implemented in Infrastructure:**
- `IRepository<T>` - Generic repository for aggregate roots
- `IUnitOfWork` - Transaction coordination
- `IApplicationDbContext` - DbContext abstraction (includes `Database` property for raw database access)
- `ICurrentUserService` - Current user access
- `IDateTimeProvider` - Testable time

### Infrastructure Layer

- `ApplicationDbContext` - EF Core DbContext
- `Repository<T>` - Generic repository implementation
- `AuditableEntityInterceptor` - Auto-fills audit fields on save
- `BaseEntityConfiguration<T>` / `AuditableEntityConfiguration<T>` - EF configs

### Web.API Layer

- `GlobalExceptionHandlerMiddleware` - Converts exceptions to RFC 7807 Problem Details
- `ResultExtensions` - Converts `Result<T>` to appropriate HTTP responses
- `CurrentUserService` - Implements `ICurrentUserService` from HTTP context

## Adding a New Feature

1. **Entity** (Domain): Create entity inheriting from `AuditableEntity`, implement `IAggregateRoot`
2. **DbSet** (Application + Infrastructure): Add to `IApplicationDbContext` and `ApplicationDbContext`
3. **Configuration** (Infrastructure): Create `EntityConfiguration : AuditableEntityConfiguration<Entity>`
4. **Repository** (optional): Create specific repository if needed beyond generic `IRepository<T>`
5. **Commands/Queries** (Application): Create in `Features/<FeatureName>/`
6. **Validators** (Application): Create FluentValidation validators
7. **Controller** (Web.API): Create in `Controllers/V1/`, use MediatR to send commands/queries

## Examples

### Product Feature (CRUD)

A complete example demonstrating all patterns is included:

- **Entity**: `Domain/Entities/Product.cs` - rich domain model with business logic
- **Events**: `Domain/Events/ProductCreatedEvent.cs`, `ProductOutOfStockEvent.cs`, `ProductDeletedEvent.cs`
- **CQRS**: `Application/Features/Products/` - commands, queries, handlers, validators
- **EF Config**: `Infrastructure/Data/Configs/ProductConfiguration.cs`
- **Controller**: `Web.API/Controllers/V1/ProductsController.cs` - full CRUD API

**Patterns demonstrated:**
- **Soft Delete**: Products use soft delete with `IsDeleted` and `DeletedAt` fields, filtered by global query filter
- **Optimistic Concurrency**: `RowVersion` column prevents concurrent update conflicts
- **Domain Events**: Events dispatched after successful database save
- **Pagination**: `PagedList<T>` wrapper with page/size/total metadata
- **Query Parameters**: `[FromQuery]` binding with FluentValidation rules visible in OpenAPI

### File Upload

Demonstrates file upload with validation:

- **Command**: `Application/Features/Files/Commands/UploadFile/` - command, handler, validator
- **Controller**: `Web.API/Controllers/V1/FilesController.cs` - upload endpoint
- **Validation**: Custom FluentValidation extensions for file size, content type, and extension checks

Use these as a reference when adding new features.

### Removing the Products Example

To start fresh without the example feature, run the removal script:

```shell
# Linux/Mac
./scripts/remove-products-example.sh
```

```powershell
# Windows (PowerShell)
.\scripts\remove-products-example.ps1
```

This removes all Products-related files, updates the DbContext, and self-deletes the scripts. After running:
1. Run `dotnet build` to verify no errors
2. Create a new migration or keep the existing Auth migration

## Authentication & Authorization

### JWT Authentication

JWT tokens are used for **authentication only** (proving WHO you are). Authorization is checked against the database on each request, ensuring role changes take effect immediately.

**Configuration via environment variables** (recommended for CI/CD):
```bash
CONNECTIONSTRINGS__DEFAULTCONNECTION=Host=localhost;Database=myapp;Username=postgres;Password=secret
JWT__SECRETKEY=your-secret-key-min-32-chars
JWT__ISSUER=MyApp
JWT__AUDIENCE=MyApp
```

See `.env.example` for all available variables. Environment variables override `appsettings.json`.

### Secure by Default

All endpoints require authentication by default. Use attributes to customize:

```csharp
[Public]                      // No authentication required
[RequireRole(Role.Admin)]     // Requires Admin role (checked against DB)
[EmailVerified]               // Requires verified email (checked against DB)
```

### Auth Endpoints

| Endpoint              | Method | Description             |
|-----------------------|--------|-------------------------|
| `/Auth/Register`      | POST   | Register new user       |
| `/Auth/Login`         | POST   | Login, returns tokens   |
| `/Auth/Refresh`       | POST   | Refresh access token    |
| `/Auth/Logout`        | POST   | Revoke refresh token(s) |
| `/Auth/Sessions`      | GET    | List active sessions    |
| `/Auth/Sessions/{id}` | DELETE | Revoke specific session |

### Multi-Device Support

- Each login creates a separate refresh token (session)
- Users can be logged in on multiple devices simultaneously
- Sessions can be viewed and revoked individually
- Device name and user agent stored for identification

### Creating Custom Guards

Use `[EmailVerified]` as a template. Create:
1. Attribute in `Web.API/Authorization/`
2. Requirement in `Web.API/Authorization/Requirements/`
3. Handler in `Web.API/Authorization/Handlers/`
4. Register policy in `AuthorizationPolicyProvider.cs`

## API Versioning & OpenAPI

The `Common.ApiVersioning` library provides API versioning, OpenAPI documentation, Scalar UI, and version lifecycle management. It's designed as a **standalone library** that can be used in any ASP.NET Core project.

**Quick Setup:**

```csharp
// Program.cs
builder.Services.AddApiVersioningServices(builder.Configuration);

var app = builder.Build();
app.UseScalarApiReference();  // Scalar UI (dev recommended)
app.UseMiddleware<ApiVersioningDeprecationMiddleware>();  // Version lifecycle
app.MapControllers();
```

**Key Features:**
- Header-based versioning (`x-api-version`)
- Namespace convention (`Controllers.V1` → version 1)
- Version lifecycle management (Active → Deprecated → Sunset)
- Automatic OpenAPI 3.0 document generation per version
- Scalar interactive documentation UI
- FluentValidation rules extracted to OpenAPI schemas (request bodies and query parameters)
- Numeric type fix for .NET 10's multi-type schema generation

**Endpoints:**

| Endpoint                  | Description                         |
|---------------------------|-------------------------------------|
| `/openapi/{version}.json` | OpenAPI 3.0 specification           |
| `/scalar`                 | Scalar interactive documentation UI |

For complete documentation including configuration options, version statuses, lifecycle headers, and advanced usage, see **[Common.ApiVersioning/README.md](Common.ApiVersioning/README.md)**.

## Health Checks

Health check endpoints for monitoring and orchestration:

| Endpoint        | Description                                     |
|-----------------|-------------------------------------------------|
| `/health/live`  | Liveness probe - is the app running?            |
| `/health/ready` | Readiness probe - is the app ready for traffic? |

The readiness check includes database connectivity. Responses use the standard health check UI format.

```bash
# Check if API is ready
curl http://localhost:5141/health/ready
```

## Observability

### Serilog Structured Logging

Serilog is configured for structured logging with console output. Configure log levels via `appsettings.json`:

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

Every request is assigned a unique correlation ID for distributed tracing:

- **Header**: `X-Correlation-ID`
- Reads from request header or generates new GUID
- Included in all logs via Serilog LogContext
- Returned in response headers
- Included in error responses (ProblemDetails)

```bash
# Pass a correlation ID
curl -H "X-Correlation-ID: my-trace-123" http://localhost:5141/Products
```

### Request Logging

Debug-level logging of HTTP requests and responses. Automatically:
- Logs request method, path, and sanitized headers
- Logs response status code and timing
- Excludes health check endpoints
- Redacts sensitive headers (Authorization, Cookie, API keys)

Enable by setting Serilog minimum level to `Debug` in development.

### OpenTelemetry (Recommended)

For production observability, add OpenTelemetry instrumentation:

```xml
<!-- Web.API.csproj -->
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.11.2" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.11.1" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.10.0-beta.1" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.11.2" />
```

See [OpenTelemetry .NET documentation](https://opentelemetry.io/docs/languages/net/) for setup.

## Rate Limiting

Built-in rate limiting protects against abuse:

**Configuration (`appsettings.json`):**
```json
"RateLimiting": {
  "Global": { "PermitLimit": 100, "WindowMinutes": 1 },
  "Api": { "PermitLimit": 30, "WindowMinutes": 1 }
}
```

- **Global limiter**: Applies to all requests per IP
- **Api limiter**: Apply to specific endpoints with `[EnableRateLimiting("Api")]`

When rate limited, returns `429 Too Many Requests`.

## CORS Configuration

Configure allowed origins in `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": ["https://yourdomain.com", "https://*.yourdomain.com"]
}
```

Or via environment variable:
```bash
CORS__ALLOWEDORIGINS=https://yourdomain.com,https://*.yourdomain.com
```

**Wildcard Support:**
- Use `*` for subdomain wildcards: `https://*.example.com`
- Matches any subdomain: `https://app.example.com`, `https://api.example.com`

**Behavior:**
- With origins configured: Strict CORS with credentials support
- In Development without origins: Allow any origin (no credentials)

## Caching

Optional response caching with Memory or Redis backends.

**Configuration (`appsettings.json`):**
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
- `Memory` - In-process memory cache (default)
- `Redis` - Distributed Redis cache
- `None` / disabled - No-op (zero overhead)

**Usage in queries:**
```csharp
public sealed record GetProductByIdQuery(Guid Id)
    : IQuery<ProductDto>, ICacheableQuery
{
    public string CacheKey => $"products:{Id}";
}
```

Implement `ICacheableQuery` to enable automatic caching via `CachingBehavior`.

Redis is included in the Docker development stack and production deployment.

## Background Jobs

Abstraction for background job processing with pluggable providers.

**Configuration (`appsettings.json`):**
```json
"BackgroundJobs": {
  "Provider": "Instant"
}
```

**Providers:**
- `Instant` - Fire-and-forget after response (default, good for dev)
- `Memory` - In-memory queue with background worker
- `None` - Disabled, jobs are dropped

**Usage:**
```csharp
public class MyHandler(IBackgroundJobService jobs)
{
    public async Task Handle(...)
    {
        // Job runs after response is sent
        await jobs.EnqueueAsync(new SendWelcomeEmailCommand(userId));
        await jobs.ScheduleAsync(new CleanupCommand(), TimeSpan.FromHours(1));
    }
}
```

**Production: Hangfire with Redis**

For production workloads, replace the built-in providers with [Hangfire](https://www.hangfire.io/):

```bash
dotnet add Web.API package Hangfire.AspNetCore
dotnet add Web.API package Hangfire.Redis.StackExchange
```

```csharp
// Program.cs
builder.Services.AddHangfire(config => config
    .UseRedisStorage(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddHangfireServer();

// Usage - same interface, production-ready
BackgroundJob.Enqueue(() => SendEmail(userId));
```

Benefits: persistence, retries, dashboard, distributed workers, scheduled jobs.

**Other options:**
- [Quartz.NET](https://www.quartz-scheduler.net/) - Enterprise cron-style scheduling
- Message queues (RabbitMQ, Azure Service Bus) - For distributed/microservice scenarios

## Domain Events

Domain events allow entities to announce business-significant occurrences without coupling to handlers.

**Flow:**
1. Entity raises event: `AddDomainEvent(new ProductCreatedEvent(...))`
2. Events stored in entity until `SaveChangesAsync()`
3. Events collected before save, dispatched via MediatR **after successful save**

This ensures data consistency - events are only dispatched after the database transaction commits.

**Creating a new event:**

1. Define event in `Domain/Events/`:
```csharp
public sealed record OrderShippedEvent(Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

1. Raise from entity:
```csharp
public void Ship()
{
    Status = OrderStatus.Shipped;
    AddDomainEvent(new OrderShippedEvent(Id));
}
```

1. Create handler in `Application/Features/<Feature>/Events/`:
```csharp
public sealed class OrderShippedEventHandler
    : INotificationHandler<DomainEventNotification<OrderShippedEvent>>
{
    public Task Handle(DomainEventNotification<OrderShippedEvent> notification,
        CancellationToken cancellationToken)
    {
        // Send email, update external systems, etc.
        return Task.CompletedTask;
    }
}
```

**Use cases:** Audit logging, email notifications, cache invalidation, search index updates, message queue publishing.

## Testing

Three test projects are included:

```
Tests/
├── Domain.UnitTests/           # Domain entity tests
├── Application.UnitTests/      # Handler and validator tests
└── Web.API.IntegrationTests/   # API integration tests with Testcontainers
```

**Run all tests:**
```bash
dotnet test
```

**Run specific project:**
```bash
dotnet test Tests/Domain.UnitTests
dotnet test Tests/Application.UnitTests
dotnet test Tests/Web.API.IntegrationTests
```

**Integration tests** use [Testcontainers](https://testcontainers.com/) to spin up a real PostgreSQL database. Docker must be running.

## Key Patterns

- **API Versioning**: Header-based (`x-api-version`) with namespace conventions (`Controllers.V1`)
- **Result Pattern**: Use `Result<T>` instead of exceptions for expected failures
- **Repository Pattern**: Only for aggregate roots via `IRepository<T>`
- **Unit of Work**: `IUnitOfWork.SaveChangesAsync()` commits all changes atomically
- **Secure by Default**: All endpoints require authentication unless marked `[Public]`

## Code Style

Enforced via `.editorconfig` and [CSharpier](https://csharpier.com/) (C# 14 / .NET 10):

- **File-scoped namespaces** required
- **Nullable reference types** enabled
- **Private fields**: `_camelCase`
- **Private static fields**: `s_camelCase`
- **Interfaces**: `I` prefix
- **No `this.` qualification**
- **`var`**: Only when type is apparent
- **Expression-bodied members**: For single-line methods/properties
- **Line width**: 120 characters (enforced by CSharpier)

## License

[MIT](LICENSE)

## Authors

[AnriaruDoragon](https://doragon.me/) (anriarudoragon@gmail.com)
