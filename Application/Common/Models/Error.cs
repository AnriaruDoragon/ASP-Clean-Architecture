namespace Application.Common.Models;

/// <summary>
/// Represents an error with a code, description, and HTTP status code.
/// Used with the Result pattern for explicit error handling.
/// </summary>
public record Error(string Code, string Description, int StatusCode = 500)
{
    /// <summary>
    /// Optional extension data to include in the ProblemDetails response.
    /// </summary>
    public IDictionary<string, object?>? Extensions { get; init; }

    /// <summary>
    /// Optional field name for field-level error reporting in ProblemDetails.
    /// </summary>
    public string? Field { get; init; }

    /// <summary>
    /// Represents no error (success state).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, 200);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("NullValue", "A null value was provided.", 400);

    /// <summary>
    /// Creates an error from an <see cref="ErrorCode"/> enum value, with an optional field and message override.
    /// </summary>
    public static Error From(ErrorCode code, string? field = null, string? message = null) =>
        new(code.ToString(), message ?? code.GetDefaultMessage(), code.GetStatusCode()) { Field = field };

    /// <summary>
    /// Creates a not found error for a specific entity by ID.
    /// </summary>
    public static Error NotFound(string entityName, object id) =>
        new("NotFound", $"{entityName} with id '{id}' was not found.", 404);

    /// <summary>
    /// Creates a not found error with a custom description.
    /// </summary>
    public static Error NotFound(string description) => new("NotFound", description, 404);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    public static Error Unauthorized(string description = "You are not authorized to perform this action.") =>
        new("Unauthorized", description, 401);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    public static Error Forbidden(string description = "You do not have permission to access this resource.") =>
        new("Forbidden", description, 403);
}
