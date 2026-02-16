# Error Handling

## Design Decisions

### Why Typed Error Codes?

When a frontend receives `{ "detail": "Email is already registered." }`, it has to match on an English string to show the right error message. Rename the string, and the frontend breaks. Translate the API to another language, and the frontend breaks.

This template uses a typed `ErrorCode` enum. Every error response includes an `errorCode` field:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Email is already registered.",
  "errorCode": "EmailTaken"
}
```

The frontend matches on `errorCode`, not `detail`. The `detail` field is for developers reading logs. The `errorCode` is for programmatic handling and i18n.

### Why the Result Pattern?

Exceptions are invisible control flow. A method that returns `User` might throw three different exceptions — you won't know until you read the implementation. The Result pattern makes failure explicit:

```csharp
// The return type tells you this can fail
Result<AuthTokensResponse> result = await sender.Send(loginCommand, ct);
```

See [Architecture](architecture.md) for more on the Result pattern.

## Creating Errors in Handlers

### Error.From() — The Primary Pattern

`Error.From(ErrorCode)` is the standard way to create errors in handlers:

```csharp
// Simple error — code determines status code and default message
return Result.Failure(Error.From(ErrorCode.EmailAlreadyVerified));

// With a field — produces a field-level error in the response
return Result.Failure(Error.From(ErrorCode.InvalidCredentials, "password"));

// With a message override — for dynamic messages
return Result.Failure(Error.From(ErrorCode.UsernameCooldown, message: $"Try again after {date}"));
```

### Error.NotFound() — Entity Lookups

For internal entity lookups where a coded error isn't useful to the frontend:

```csharp
return Result.Failure(Error.NotFound("Product", request.Id));
// Produces: { "status": 404, "detail": "Product with id '...' was not found." }
```

### Convenience Factories

Uncoded errors for generic situations:

```csharp
Error.Unauthorized()  // 401, default message
Error.Forbidden()     // 403, default message
```

## ErrorCode Enum

Every `ErrorCode` value is decorated with `[ErrorInfo(statusCode, defaultMessage)]`:

```csharp
public enum ErrorCode
{
    [ErrorInfo(400, "Invalid email or password.")]
    InvalidCredentials,

    [ErrorInfo(409, "Email is already registered.")]
    EmailTaken,

    [ErrorInfo(401, "Authentication is required.")]
    NotAuthenticated,

    // ...
}
```

The status code and default message are derived from the attribute automatically. To add a new error code, add a value to the enum with an `[ErrorInfo]` attribute — no other changes needed.

The enum is also exposed in the OpenAPI schema, so API consumers can see all possible error codes.

## Validation Errors

There are two kinds of errors that produce field-level details:

### FluentValidation Errors (Automatic)

FluentValidation runs automatically in the MediatR pipeline via `ValidationBehavior`. If validation fails, the handler never executes:

```json
{
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errorCode": "ValidationFailed",
  "errors": {
    "Email": [{ "detail": "Email is required." }],
    "Password": [
      { "detail": "Password must be at least 8 characters." },
      { "detail": "Password must contain at least one uppercase letter." }
    ]
  }
}
```

When `errorCode` is `ValidationFailed`, the `errors` dictionary is always present with at least one entry.

### Handler Field Errors (Error.From with field)

When a handler uses `Error.From(ErrorCode.X, "field")`, the response includes a single-field error with the specific code:

```json
{
  "status": 400,
  "detail": "Invalid email or password.",
  "errorCode": "InvalidCredentials",
  "errors": {
    "password": [{ "code": "InvalidCredentials", "detail": "Invalid email or password." }]
  }
}
```

The difference: FluentValidation errors have no per-field `code` (they all share `ValidationFailed`). Handler field errors have a specific `code` per field.

## ProblemDetails Response Format

All error responses follow RFC 7807:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Human-readable description of what went wrong.",
  "errorCode": "InvalidCredentials",
  "errors": {
    "fieldName": [
      { "code": "ErrorCodeName", "detail": "Field-level message." }
    ]
  }
}
```

| Field | Always present | Description |
|---|---|---|
| `type` | Yes | RFC 9110 reference URI for the status code |
| `title` | Yes | HTTP status text (e.g., "Bad Request") |
| `status` | Yes | HTTP status code |
| `detail` | Yes | Human-readable error description |
| `errorCode` | Yes | `ErrorCode` enum value (for programmatic matching) |
| `errors` | Only for validation/field errors | Dictionary of field name to error details |

## Frontend Consumption

```typescript
// Match on errorCode, not detail
switch (response.errorCode) {
  case "InvalidCredentials":
    showError(t("auth.invalidCredentials"));
    break;
  case "EmailTaken":
    showFieldError("email", t("auth.emailTaken"));
    break;
  case "ValidationFailed":
    // Show field-level errors from errors dictionary
    for (const [field, errors] of Object.entries(response.errors)) {
      showFieldError(field, errors.map(e => e.detail).join(", "));
    }
    break;
}
```
