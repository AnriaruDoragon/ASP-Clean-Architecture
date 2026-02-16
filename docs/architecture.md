# Architecture

## Layer Overview

```
Domain/           Entities, value objects, domain events (no dependencies)
Application/      CQRS handlers, validators, interfaces (depends on Domain)
Infrastructure/   EF Core, repositories, external services (depends on Application)
Web.API/          Controllers, middleware (depends on all layers)
Common.ApiVersioning/ Shared API versioning library (standalone)
```

**Dependency Flow:** Domain <- Application <- Infrastructure <- Web.API

## Domain Layer

The innermost layer. Zero external dependencies — not even NuGet packages beyond .NET itself.

**Base classes:**
- `BaseEntity` — Id + domain events support
- `AuditableEntity` — Adds CreatedAt/By, ModifiedAt/By (auto-filled by `AuditableEntityInterceptor`)
- `IAggregateRoot` — Marker for aggregate roots (only these should be directly queried)
- `ValueObject` — Base for value objects (equality by value, not reference)
- `IDomainEvent` — Domain event interface

**Why no dependencies?** Domain logic should be testable with nothing but `new` and `Assert`. If your domain needs a database connection or HTTP client, something is wrong.

## Application Layer

Business logic lives here, organized as CQRS handlers.

### CQRS Pattern

Every operation is either a **Command** (changes state) or a **Query** (reads state):

```csharp
// Command — changes state, returns Result or Result<T>
public sealed record CreateProductCommand(string Name, decimal Price) : ICommand;

// Query — reads state, always returns Result<T>
public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;
```

Each has exactly one handler:

```csharp
public sealed class CreateProductCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateProductCommand>
{
    public async Task<Result> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = Product.Create(request.Name, request.Price);
        context.Products.Add(product);
        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
```

**Why one handler per operation?** Dependencies are explicit. Testing is trivial — construct handler, call `Handle()`, assert result. No mocking 14 unused services to test one method.

### Pipeline Behaviors

MediatR behaviors run automatically before/after every handler:

| Behavior | Purpose |
|---|---|
| `ValidationBehavior` | Runs FluentValidation validators before the handler executes. If validation fails, the handler never runs. |
| `LoggingBehavior` | Logs request execution and timing. |
| `CachingBehavior` | Caches query results for `ICacheableQuery` implementations. |

**Why pipeline behaviors?** Cross-cutting concerns like validation and logging happen automatically. You can't forget to validate — it's in the pipeline.

### Result Pattern

All handlers return `Result` or `Result<T>` instead of throwing exceptions:

```csharp
// Failure is explicit in the return type
public async Task<Result<AuthTokensResponse>> Handle(LoginCommand request, ...)
{
    User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    if (user is null)
        return Result.Failure<AuthTokensResponse>(Error.From(ErrorCode.InvalidCredentials, "password"));

    // ...
    return new AuthTokensResponse(accessToken, refreshToken, expiresAt);
}
```

See [Error Handling](error-handling.md) for the full error model.

### Interfaces

Application defines interfaces, Infrastructure implements them:

- `IApplicationDbContext` — DbContext abstraction
- `IRepository<T>` — Generic repository for aggregate roots
- `IUnitOfWork` — Transaction coordination
- `ICurrentUserService` — Current authenticated user (throws if not authenticated)
- `IDateTimeProvider` — Testable time abstraction
- `IJwtService` — Token generation and validation
- `IPasswordHasher` — Password hashing
- `IEmailService` — Email sending
- `ICacheService` — Cache operations
- `IBackgroundJobService` — Background job scheduling

**Why interfaces?** Testability and swappability. Mock `IApplicationDbContext` in unit tests. Replace `ICacheService` implementation from Memory to Redis by changing one DI registration.

## Infrastructure Layer

Implements Application interfaces with concrete technology:

- `ApplicationDbContext` — EF Core DbContext implementing `IApplicationDbContext` and `IUnitOfWork`
- `Repository<T>` — Generic repository implementation
- `AuditableEntityInterceptor` — Auto-fills CreatedAt/ModifiedAt on save
- `BaseEntityConfiguration<T>` / `AuditableEntityConfiguration<T>` — EF Core configurations
- `JwtService`, `PasswordHasher`, `DateTimeProvider` — Service implementations

## Web.API Layer

The outermost layer. Thin controllers that construct commands/queries and return results:

```csharp
public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
{
    var command = new RegisterCommand(request.Email, request.Password, request.DeviceName, Request.Headers.UserAgent);
    Result<AuthTokensResponse> result = await sender.Send(command, ct);
    return result.ToActionResult();
}
```

Controllers have no business logic. They map HTTP requests to commands and commands to HTTP responses.

**Key infrastructure:**
- `GlobalExceptionHandlerMiddleware` — Converts unhandled exceptions to RFC 7807 ProblemDetails
- `ResultExtensions` — Converts `Result<T>` to appropriate HTTP responses with ProblemDetails
- `CurrentUserService` — Implements `ICurrentUserService` by extracting user ID from JWT claims

## Domain Events

Entities announce business-significant occurrences without knowing who listens:

```csharp
// 1. Define event (Domain layer)
public sealed record ProductCreatedEvent(Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// 2. Raise from entity
public static Product Create(string name, decimal price)
{
    var product = new Product { Name = name, Price = price };
    product.AddDomainEvent(new ProductCreatedEvent(product.Id));
    return product;
}

// 3. Handle (Application layer)
public sealed class ProductCreatedEventHandler
    : INotificationHandler<DomainEventNotification<ProductCreatedEvent>>
{
    public Task Handle(DomainEventNotification<ProductCreatedEvent> notification, CancellationToken ct)
    {
        // Send email, update cache, publish to message queue, etc.
        return Task.CompletedTask;
    }
}
```

Events are collected before `SaveChangesAsync()` and dispatched **after** the transaction commits. This guarantees data consistency — handlers only run if the write succeeded.

## Code Style

Enforced via `.editorconfig` and [CSharpier](https://csharpier.com/):

- File-scoped namespaces required
- Nullable reference types enabled
- Private fields: `_camelCase`, private static: `s_camelCase`
- `var` only when type is apparent
- Expression-bodied members for single-line methods/properties
- Line width: 120 characters
