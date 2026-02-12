# Common.ApiVersioning

A comprehensive ASP.NET Core library for managing API versioning with OpenAPI documentation and Scalar UI integration.
Provides automatic version lifecycle management, deprecation warnings, and sunset enforcement through HTTP standards
compliance.

## Features

- **Automatic Version Detection** - Controllers are versioned by namespace convention
- **Lifecycle Management** - Track versions from Internal/Alpha through Deprecation to Sunset
- **Standards Compliant** - Uses RFC 8594 Deprecation and Sunset HTTP headers
- **OpenAPI Integration** - Generates separate OpenAPI 3.0 documents per version
- **Scalar UI** - Interactive API documentation with version selector
- **Sunset Enforcement** - Automatically returns HTTP 410 Gone for end-of-life versions
- **Flexible Configuration** - JSON-based configuration with validation
- **FluentValidation Integration** - Automatically extracts validation rules into OpenAPI schemas
- **Enum String Serialization** - Converts enum schemas to camelCase strings matching `JsonStringEnumConverter` behavior
- **Multiple Document Groups** - Support for admin/public API separation

## Installation

Add the project reference to your Web.API project:

```xml
<ProjectReference Include="..\Common.ApiVersioning\Common.ApiVersioning.csproj" />
```

## Quick Start

### 1. Configure Program.cs

```csharp
using Common.ApiVersioning.Extensions;
using Common.ApiVersioning.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register services
builder.Services.AddApiVersioningServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Enable Scalar UI (can be used outside of this block if you want API docs to be public) 
    app.UseScalarApiReference();
}

app.UseHttpsRedirection();

// Register middleware right here if you want to use version lifecycle
app.UseMiddleware<ApiVersioningDeprecationMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. Configure appsettings.json

```json
{
  "ApiVersioning": {
    "Versions": [
      {
        "Name": "v1",
        "Version": "1.0.0",
        "Status": "Deprecated",
        "DeprecationDate": "2025-01-01T00:00:00Z",
        "SunsetDate": "2025-06-01T00:00:00Z",
        "Title": "API v1 (Legacy)",
        "Description": "Original API version, please migrate to v2"
      },
      {
        "Name": "v2",
        "Version": "2.0.0",
        "Status": "Active",
        "Title": "API v2",
        "Description": "Current production version with enhanced features"
      }
    ],
    "Scalar": {
      "Title": "My API",
      "Theme": "Default",
      "Layout": "Modern",
      "Servers": [
        {
          "Url": "https://prod.example.com/",
          "Description": "Productions server"
        },
        {
          "Url": "https://dev.example.com/",
          "Description": "Development server"
        }
      ]
    }
  }
}
```

> If no configuration is provided, a default v1.0.0 Active version is automatically created.

### 3. Organize Controllers by Namespace

Controllers are automatically versioned based on their namespace:

```csharp
namespace YourApi.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { version = "1.0" });
}
```

```csharp
namespace YourApi.Controllers.V2;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { version = "2.0", enhanced = true });
}
```

The namespace suffix (`V1`, `V2`) must match the version `Name` from configuration (`v1`, `v2`).

## Configuration Reference

### ApiVersioning Section

| Property   | Type   | Required | Description                                                             |
|------------|--------|----------|-------------------------------------------------------------------------|
| `Versions` | array  | Yes      | List of API version definitions (see [Version Object](#version-object)) |
| `Scalar`   | object | No       | Scalar UI Configuration (see [Scalar Properties](#scalar-properties))   |

### Version Object

| Property          | Type     | Required | Description                                                        |
|-------------------|----------|----------|--------------------------------------------------------------------|
| `Name`            | string   | Yes      | Unique identifier matching controller namespace (e.g., "v1", "v2") |
| `Version`         | string   | Yes      | Version string with 1-3 numeric parts (e.g., "1", "1.0", "1.0.0")  |
| `Status`          | string   | Yes      | Lifecycle status (see [Lifecycle Statuses](#lifecycle-statuses))   |
| `DeprecationDate` | datetime | No       | ISO 8601 date when version was/will be deprecated                  |
| `SunsetDate`      | datetime | No       | ISO 8601 date when version reaches end-of-life                     |
| `Title`           | string   | No       | Display title in Scalar UI                                         |
| `Description`     | string   | No       | Detailed description supporting markdown                           |

### Scalar Properties

| Property  | Type   | Required | Description                                                                                     |
|-----------|--------|----------|-------------------------------------------------------------------------------------------------|
| `Title`   | string | No       | Global title displayed in Scalar UI (default: "API")                                            |
| `Theme`   | string | No       | Color theme (see [Available Themes](#available-themes))                                         |
| `Layout`  | string | No       | Layout style: "Modern" (default) or "Classic"                                                   |
| `Servers` | array  | No       | List of server environments for API testing (see [Server Configuration](#server-configuration)) |

#### Available Themes

Choose from these Scalar themes:

- **None** - Scalar's default theme 
- **Default** - Standard Scalar theme 
- **Alternate** - Alternative color scheme 
- **Moon** - Dark theme with blue accents 
- **Purple** - Purple-based color scheme 
- **Solarized** - Solarized color palette 
- **BluePlanet** - Blue planetary theme 
- **Saturn** - Saturn-inspired theme (space aesthetic)
- **Kepler** - Kepler space theme 
- **Mars** - Mars-inspired red theme 
- **DeepSpace** - Dark space theme 
- **Laserwave** - Synthwave-inspired theme

#### Server Configuration

Servers appear in a dropdown in Scalar UI, allowing users to test API requests against different environments:

```json
{
  "Servers": [
    {
      "Url": "https://api.example.com",
      "Description": "Production API"
    },
    {
      "Url": "http://localhost:5000",
      "Description": "Local development"
    }
  ]
}
```

Each server object supports:

- **Url** (required) - The base URL of the API server
- **Description** (optional) - Human-readable description of the server

> If no servers are configured, Scalar uses the current request's base URL as the default.

## Lifecycle Statuses

Versions progress through defined lifecycle stages:

### Pre-Release Statuses

- **Internal** - Internal testing only, not for external use
- **Preview** - Early access preview, unstable
- **Alpha** - Experimental, significant changes expected
- **Beta** - Feature-complete but may have bugs

### Production Statuses

- **Active** - Recommended production version (only one allowed)
- **Current** - Synonym for Active (only one allowed)
- **Legacy** - Older stable version, still supported but not recommended

### End-of-Life Statuses

- **Deprecated** - Still functional but discouraged, emits deprecation warnings
- **Sunset** - Returns HTTP 410 Gone, requests blocked
- **Retired** - Synonym for Sunset
- **Obsolete** - Synonym for Sunset

## How It Works

### Version Detection

Clients specify the API version using the `x-api-version` header:

```http request
GET /api/products HTTP/1.1
Host: api.example.com
x-api-version: 2
```

> If no version is specified, the default Active/Current version is used.

### Deprecation Middleware

The `ApiVersioningDeprecationMiddleware` intercepts requests and:

1. **For Active/Beta/Preview versions:** Adds `X-API-Version-Status` header
2. **For Deprecated versions:** Adds deprecation headers:
   ```http
   X-API-Version-Status: deprecated
   Deprecation: true
   Sunset: Sat, 01 Jun 2025 00:00:00 GMT
   X-API-Info: This API version is deprecated. Please migrate to v2
   ```
3. **For Sunset versions:** Returns HTTP 410 Gone with RFC 7807 Problem Details:
   ```http
   HTTP/1.1 410 Gone
   Content-Type: application/problem+json
   X-API-Version-Status: sunset
   ```
   ```json
   {
     "type": "https://httpstatuses.io/410",
     "title": "Gone",
     "status": 410,
     "detail": "API version v1 has reached end-of-life and no longer accepts requests.",
     "instance": "/api/products",
     "migrateToVersion": "2.0.0"
   }
   ```

### OpenAPI Document Generation

Each version generates a separate OpenAPI document accessible at:

- `/openapi/v1.json`
- `/openapi/v2.json`

Scalar UI combines all documents with a version selector dropdown.

### FluentValidation Integration

When FluentValidation validators are registered, the library automatically extracts validation rules and adds them to OpenAPI schemas and parameters:

- **Schema Transformer** — Applies rules to request body properties (POST/PUT payloads)
- **Operation Transformer** — Applies rules to query and route parameters (`[FromQuery]`, `[FromRoute]`)

For query parameters to be picked up, bind them as a complex type:

```csharp
public async Task<IActionResult> GetProducts(
    [FromQuery] GetProductsQuery query,    // ✅ Rules extracted
    CancellationToken cancellationToken)
```

**Supported rules:**

| FluentValidation Rule      | OpenAPI Schema                         |
|----------------------------|----------------------------------------|
| `NotEmpty()` / `NotNull()` | `required`                             |
| `MaximumLength(100)`       | `maxLength: 100`                       |
| `MinimumLength(5)`         | `minLength: 5`                         |
| `Length(5, 100)`           | `minLength: 5, maxLength: 100`         |
| `GreaterThan(0)`           | `minimum: 0, exclusiveMinimum: true`   |
| `GreaterThanOrEqualTo(1)`  | `minimum: 1`                           |
| `LessThan(100)`            | `maximum: 100, exclusiveMaximum: true` |
| `LessThanOrEqualTo(99)`    | `maximum: 99`                          |
| `InclusiveBetween(1, 100)` | `minimum: 1, maximum: 100`             |
| `EmailAddress()`           | `format: "email"`                      |
| `Matches("^[a-z]+$")`      | `pattern: "^[a-z]+$"`                  |

### Enum Schema Transformer

When `JsonStringEnumConverter` is configured globally, enums serialize as camelCase strings at runtime — but the default OpenAPI schema still describes them as integers. The `EnumSchemaTransformer` fixes this by:

- Changing the schema `type` from `integer` to `string`
- Listing all enum values as camelCase strings (e.g., `["user", "admin"]`)
- Preserving nullable enum support

This runs automatically when registered via `AddApiVersioningServices()`.

### Numeric Type Fix

.NET 10's OpenAPI generator sets multi-type flags on numeric properties (e.g., `Integer | String`) due to JSON Schema 2020-12 semantics. Since OpenAPI 3.0 only supports a single `type` value, the serializer drops the field entirely — causing Scalar UI to display numeric fields as strings.

The `NumericTypeSchemaTransformer` strips the extraneous `String` flag from numeric types, and the Operation Transformer ensures query parameter types are correctly set.

## Advanced Usage

### Alternative Versioning Methods

By default, versioning uses the `x-api-version` header. You can enable additional methods:

```csharp
builder.Services.AddApiVersioningServices(builder.Configuration);

// Customize after registration
builder.Services.Configure<ApiVersioningOptions>(options =>
{
    options.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("x-api-version"),
        new QueryStringApiVersionReader("api-version"),
        new UrlSegmentApiVersionReader()
    );
});
```

For URL segment versioning (`/api/v2/products`), also enable:

```csharp
builder.Services.PostConfigure<ApiExplorerOptions>(options =>
{
    options.SubstituteApiVersionInUrl = true;
});
```

### Separate Document Groups

Create separate OpenAPI documents for public and admin APIs:

```csharp
// Public API
namespace YourApi.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase { }
```

```csharp
// Admin API
namespace YourApi.Controllers.V1;

[ApiController]
[Route("api/admin/[controller]")]
[ApiExplorerSettings(GroupName = "v1Admin")]
public class AdminController : ControllerBase { }
```

Configure in appsettings.json:

```json
{
  "ApiVersioning": {
    "Versions": [
      {
        "Name": "v1",
        "Version": "1.0.0",
        "Status": "Active",
        "Title": "Public API"
      },
      {
        "Name": "v1Admin",
        "Version": "1.0.0",
        "Status": "Internal",
        "Title": "Admin API"
      }
    ]
  }
}
```

This generates two separate documents in Scalar UI.

### Validation Rules

Configuration is validated on startup:

- At least one version with Active or Current status must exist
- Only one Active/Current version is allowed
- All versions must have non-empty Name and Version
- Version must contain 1-3 numeric parts (e.g., "1", "1.0", "1.0.0")

Validation failures throw `InvalidOperationException` with descriptive messages.

## Integration with Clean Architecture

Place `Common.ApiVersioning` as a shared library:

```
YourSolution.sln
├── src/
│   ├── Common.ApiVersioning/           # Shared library
│   │   ├── Configs/
│   │   ├── Enums/
│   │   ├── Extensions/
│   │   ├── Middlewares/
│   │   └── OpenApi/                    # Schema/operation transformers (enum, numeric, FluentValidation)
│   └── Web.API/                        # References Common.ApiVersioning
│       └── Program.cs
```

Reference from `Web.API.csproj`:

```xml
<ProjectReference Include="..\Common.ApiVersioning\Common.ApiVersioning.csproj" />
```

## Dependencies

- .NET 10.x
- Asp.Versioning.Mvc.ApiExplorer (8.x)
- Asp.Versioning.Mvc (8.x)
- FluentValidation (11.x)
- Microsoft.AspNetCore.OpenApi (10.x)
- Scalar.AspNetCore (2.x)

## Best Practices

1. **Always specify Active version** - Don't rely on defaults
2. **Set deprecation dates early** - Give clients time to migrate (6-12 months recommended)
3. **Document breaking changes** - Use the Description field extensively
4. **Test sunset behavior** - Verify HTTP 410 responses before sunset date
5. **Version by namespace** - Follow the convention for automatic detection
6. **Use semantic versioning** - Increment major version for breaking changes
7. **Monitor version usage** - Track which versions clients are using before sunset

## Troubleshooting

### Controllers not appearing in OpenAPI

- Verify controller namespace matches version Name
- Ensure `[ApiController]` attribute is present
- Check that controller is public

### Multiple Active versions error

Only one version can have Active or Current status. Use Legacy for older supported versions.

### Version not detected from header

- Verify header name is `x-api-version` (default)
- Ensure middleware is registered before `UseAuthorization()`
- Check that major version number is sent (e.g., "1" not "1.0")

### Scalar UI not showing

- Verify `UseScalarApiReference()` is called
- Check that `/openapi/{version}.json` endpoints are accessible
- Ensure `AddApiVersioningServices()` was called first

## License

[MIT](LICENSE)

## Authors

[Anriaru Doragon](https://doragon.me/) (anriarudoragon@gmail.com)
