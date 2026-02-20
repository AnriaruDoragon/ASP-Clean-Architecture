# ASP.NET Clean Architecture Template

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](compose.yaml)

A production-ready ASP.NET Core 10.0 template implementing Clean Architecture with CQRS pattern.

## Why This Template

`dotnet new webapi` gives you a single-file API with no structure. That's fine for a demo. For anything real, you'll spend weeks making the same decisions every team makes: how to organize code, where validation goes, how errors reach the client, how auth works, how to test without a running database. This template makes those decisions upfront so you can start building features on day one.

### Why Clean Architecture?

Most projects start as a single Web API project. Dependencies leak everywhere: controllers call EF Core directly, business logic lives in services that are impossible to test without a database, and swapping out infrastructure (say, moving from SQL Server to PostgreSQL) means touching half your codebase.

Clean Architecture enforces boundaries. Domain logic has zero dependencies. Application logic depends only on abstractions. Infrastructure is a pluggable detail. The result: you can test business rules without a database, swap caching providers without touching handlers, and read any handler in isolation because its dependencies are explicit.

### Why CQRS?

Service classes grow into god objects. `UserService` starts with `Register()` and `Login()`, then accumulates `UpdateProfile()`, `ChangePassword()`, `GetSessions()`, `RevokeSession()` until it has 20 methods and 15 constructor dependencies.

CQRS replaces services with focused handlers. Each handler does one thing, takes one request, returns one result. Dependencies are exactly what that operation needs. Testing is straightforward: construct the handler, call `Handle()`, assert the result. No mocking 14 unused dependencies to test one method.

### Why the Result pattern?

Exceptions are invisible control flow. A method signature says it returns `User`, but it might throw `NotFoundException`, `ValidationException`, `UnauthorizedException` — you won't know until you read the implementation or hit it at runtime. Callers forget to catch, and you get 500s in production.

`Result<T>` makes failure explicit. The return type tells you this operation can fail. The compiler forces you to handle both paths. Errors carry typed codes (`ErrorCode` enum) that frontends can match on for i18n, instead of parsing human-readable strings.

### Why secure by default?

Most frameworks require you to opt *in* to authentication with `[Authorize]`. Forget it on one endpoint and you have an unauthenticated route in production. This template flips it: all endpoints require authentication by default. You opt *out* with `[Public]` — a deliberate, visible decision.

## What's Included

| Feature                            | Why                                                                                                    |
|------------------------------------|--------------------------------------------------------------------------------------------------------|
| **JWT auth with refresh tokens**   | Stateless authentication with revocable sessions. Multi-device support out of the box.                 |
| **CQRS with MediatR**              | One handler per operation. Explicit dependencies. Easy to test.                                        |
| **FluentValidation pipeline**      | Validation runs before the handler, automatically. You can't forget it.                                |
| **Result + ProblemDetails errors** | Typed error codes (`ErrorCode` enum) in RFC 7807 responses. Frontends can match on codes, not strings. |
| **Rate limiting**                  | Per-IP, per-user, and per-session rate limiting. Configurable policies via `appsettings.json`.         |
| **Domain events**                  | Entities announce what happened. Handlers react. No coupling between the two.                          |
| **API versioning**                 | Header-based versioning with per-version OpenAPI docs and lifecycle management.                        |
| **Caching pipeline**               | Implement `ICacheableQuery` on a query and it's cached. Memory or Redis backends.                      |
| **Background jobs**                | Fire-and-forget or queued jobs via `IBackgroundJobService`. Swap in Hangfire for production.           |
| **Health checks**                  | Liveness and readiness probes for orchestrators. Database connectivity included.                       |
| **Structured logging**             | Serilog with correlation IDs for distributed tracing.                                                  |
| **Docker + Traefik**               | Development stack with HTTPS via Traefik. Production stack with Nginx.                                 |

## Architecture

```
Domain/           Entities, value objects, domain events. No dependencies.
Application/      CQRS handlers, validators, interfaces. Depends on Domain.
Infrastructure/   EF Core, JWT, email, caching. Depends on Application.
Web.API/          Controllers, middleware. Depends on all layers.
```

**Dependency flow:** `Domain` <- `Application` <- `Infrastructure` <- `Web.API`

Each layer only knows about the layer directly below it (through interfaces). Infrastructure details like databases, email providers, and JWT tokens are invisible to Application — it works with `IApplicationDbContext`, `IEmailService`, and `IJwtService`.

**Why this matters:** You can test any handler by providing a mock `IApplicationDbContext`. You can swap PostgreSQL for SQL Server by changing one project. You can replace JWT with API keys without touching a single handler.

## Quick Start

```bash
# Install template
git clone https://github.com/AnriaruDoragon/ASP-Clean-Architecture
dotnet new install ./ASPCleanArchitecture

# Create project (auto-setup: restore, .env, migration)
dotnet new aspclean -n MyApp --allow-scripts yes
cd MyApp

# Start development
task docker:up    # Start DB + Redis + Traefik
task watch        # Run API with hot-reload

# Access: http://localhost:5141
# API docs: http://localhost:5141/scalar/v1
```

See [Getting Started](docs/getting-started.md) for detailed setup, template parameters, and Docker workflows.

## Documentation

| Guide                                      | Description                                                                             |
|--------------------------------------------|-----------------------------------------------------------------------------------------|
| [Getting Started](docs/getting-started.md) | Installation, template parameters, development setup, Docker commands                   |
| [Architecture](docs/architecture.md)       | Layer responsibilities, CQRS pattern, pipeline behaviors, Result pattern, domain events |
| [Authentication](docs/authentication.md)   | JWT setup, secure-by-default, auth endpoints, custom guards                             |
| [Error Handling](docs/error-handling.md)   | `Error.From()` pattern, `ErrorCode` enum, validation errors, ProblemDetails format      |
| [Deployment](docs/deployment.md)           | Docker Compose production, VM/EC2 deployment, systemd service                           |
| [Configuration](docs/configuration.md)     | Environment variables, CORS, caching, rate limiting, background jobs, observability     |
| [Adding Features](docs/adding-features.md) | Step-by-step guide, Product CRUD example, file upload example                           |

## License

[MIT](LICENSE)

## Authors

[AnriaruDoragon](https://doragon.me/) (anriarudoragon@gmail.com)
