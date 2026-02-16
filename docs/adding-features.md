# Adding Features

## Step by Step

1. **Entity** (Domain): Create entity inheriting from `AuditableEntity`, implement `IAggregateRoot`
2. **DbSet** (Application + Infrastructure): Add to `IApplicationDbContext` and `ApplicationDbContext`
3. **Configuration** (Infrastructure): Create `EntityConfiguration : AuditableEntityConfiguration<Entity>`
4. **Commands/Queries** (Application): Create in `Features/<FeatureName>/`
5. **Validators** (Application): Create FluentValidation validators
6. **Controller** (Web.API): Create in `Controllers/V1/`, use MediatR to send commands/queries
7. **Migration**: Run `task migration:add -- AddMyEntity`

## Product Feature (CRUD Example)

A complete example demonstrating all patterns is included:

- **Entity**: `Domain/Entities/Product.cs` — rich domain model with business logic
- **Events**: `Domain/Events/ProductCreatedEvent.cs`, `ProductOutOfStockEvent.cs`, `ProductDeletedEvent.cs`
- **CQRS**: `Application/Features/Products/` — commands, queries, handlers, validators
- **EF Config**: `Infrastructure/Data/Configs/ProductConfiguration.cs`
- **Controller**: `Web.API/Controllers/V1/ProductsController.cs` — full CRUD API

**Patterns demonstrated:**
- **Soft Delete**: `IsDeleted` + `DeletedAt` fields, filtered by global query filter
- **Optimistic Concurrency**: `RowVersion` column prevents concurrent update conflicts
- **Domain Events**: Events dispatched after successful save
- **Pagination**: `PagedList<T>` wrapper with page/size/total metadata
- **Query Parameters**: `[FromQuery]` binding with FluentValidation rules visible in OpenAPI

## File Upload Example

Demonstrates file upload with validation:

- **Command**: `Application/Features/Files/Commands/UploadFile/` — command, handler, validator
- **Controller**: `Web.API/Controllers/V1/FilesController.cs` — upload endpoint
- **Validation**: Custom FluentValidation extensions for file size, content type, and extension checks

## Removing the Products Example

To start fresh without the example feature:

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

## Testing

Three test projects are included:

```
Tests/
├── Domain.UnitTests/           # Domain entity tests
├── Application.UnitTests/      # Handler and validator tests
└── Web.API.IntegrationTests/   # API integration tests with Testcontainers
```

```bash
dotnet test                           # All tests
dotnet test Tests/Domain.UnitTests    # Specific project
```

Integration tests use [Testcontainers](https://testcontainers.com/) to spin up a real PostgreSQL database. Docker must be running.
