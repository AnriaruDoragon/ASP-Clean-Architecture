# ASP.NET Clean Architecture Template

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](compose.yaml)

A production-ready ASP.NET Core 10.0 template implementing Clean Architecture with CQRS pattern.

## Build & Run Commands

```bash
# Build
dotnet build

# Run (development)
dotnet run --project Web.API/Web.API.csproj

# Production build
dotnet build --configuration Release

# EF Core migrations
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project Web.API --output-dir Data/Migrations
dotnet ef database update --project Infrastructure --startup-project Web.API
```

**Local Development Endpoints:**
- HTTP: http://localhost:5141
- HTTPS: https://localhost:7145
- Scalar API docs (dev): https://localhost:7145/scalar/v1

## Docker Development

Run the full stack (API + PostgreSQL):

```bash
docker compose up
```

**Docker Endpoints:**
- API: http://localhost:5050
- Scalar API docs: http://localhost:5050/scalar/v1
- PostgreSQL: localhost:5432

**Connection String (for Docker):**
```
Host=db;Database=aspcleanarchitecture;Username=postgres;Password=postgres
```

**Services included:**
- **PostgreSQL 18** - Database
- **API** - ASP.NET Core application

**First-time setup** (create database tables):
```bash
dotnet ef database update --project Infrastructure --startup-project Web.API
```

**Useful commands:**
```bash
# Start in detached mode
docker compose up -d

# View logs
docker compose logs -f api

# Rebuild after code changes
docker compose up --build

# Stop and remove containers
docker compose down

# Stop and remove containers + volumes (reset database)
docker compose down -v
```

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

## Example: Product Feature

A complete example demonstrating all patterns is included:

- **Entity**: `Domain/Entities/Product.cs` - rich domain model with business logic
- **Events**: `Domain/Events/ProductCreatedEvent.cs`, `ProductOutOfStockEvent.cs`
- **CQRS**: `Application/Features/Products/` - commands, queries, handlers, validators
- **EF Config**: `Infrastructure/Data/Configs/ProductConfiguration.cs`
- **Controller**: `Web.API/Controllers/V1/ProductsController.cs` - full CRUD API

Use this as a reference when adding new features.

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
JWT__ISSUER=ASPCleanArchitecture
JWT__AUDIENCE=ASPCleanArchitecture
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
| `/Auth/register`      | POST   | Register new user       |
| `/Auth/login`         | POST   | Login, returns tokens   |
| `/Auth/refresh`       | POST   | Refresh access token    |
| `/Auth/logout`        | POST   | Revoke refresh token(s) |
| `/Auth/sessions`      | GET    | List active sessions    |
| `/Auth/sessions/{id}` | DELETE | Revoke specific session |

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
- FluentValidation rules extracted to OpenAPI schemas

**Endpoints:**

| Endpoint                  | Description                         |
|---------------------------|-------------------------------------|
| `/openapi/{version}.json` | OpenAPI 3.0 specification           |
| `/scalar/v1`              | Scalar interactive documentation UI |

For complete documentation including configuration options, version statuses, lifecycle headers, and advanced usage, see **[Common.ApiVersioning/README.md](Common.ApiVersioning/README.md)**.

## Key Patterns

- **API Versioning**: Header-based (`x-api-version`) with namespace conventions (`Controllers.V1`)
- **Result Pattern**: Use `Result<T>` instead of exceptions for expected failures
- **Repository Pattern**: Only for aggregate roots via `IRepository<T>`
- **Unit of Work**: `IUnitOfWork.SaveChangesAsync()` commits all changes atomically
- **Secure by Default**: All endpoints require authentication unless marked `[Public]`

## Code Style

Enforced via `.editorconfig` (C# 14 / .NET 10):

- **File-scoped namespaces** required
- **Nullable reference types** enabled
- **Private fields**: `_camelCase`
- **Private static fields**: `s_camelCase`
- **Interfaces**: `I` prefix
- **No `this.` qualification**
- **`var`**: Only when type is apparent
- **Expression-bodied members**: For single-line methods/properties

## License

[MIT](LICENSE)

## Authors

[Anriaru Doragon](https://doragon.me/) (anriarudoragon@gmail.com)
