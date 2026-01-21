namespace Application.Common.Models;

/// <summary>
/// Represents an error with a code and description.
/// Used with the Result pattern for explicit error handling.
/// </summary>
public sealed record Error(string Code, string Description)
{
    /// <summary>
    /// Represents no error (success state).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    public static Error NotFound(string entityName, object id) =>
        new("Error.NotFound", $"{entityName} with id '{id}' was not found.");

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    public static Error Validation(string description) =>
        new("Error.Validation", description);

    /// <summary>
    /// Creates a conflict error (e.g., duplicate entry).
    /// </summary>
    public static Error Conflict(string description) =>
        new("Error.Conflict", description);

    /// <summary>
    /// Creates a conflict error with a custom code.
    /// </summary>
    public static Error Conflict(string code, string description) =>
        new(code, description);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    public static Error Unauthorized(string description = "You are not authorized to perform this action.") =>
        new("Error.Unauthorized", description);

    /// <summary>
    /// Creates an unauthorized error with a custom code.
    /// </summary>
    public static Error Unauthorized(string code, string description) =>
        new(code, description);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    public static Error Forbidden(string description = "You do not have permission to access this resource.") =>
        new("Error.Forbidden", description);
}
