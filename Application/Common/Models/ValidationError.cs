namespace Application.Common.Models;

/// <summary>
/// Represents a single field-level error with a code and human-readable message.
/// </summary>
public sealed record FieldError(string Code, string Message);

/// <summary>
/// Represents a validation error with field-level details.
/// </summary>
public sealed record ValidationError : Error
{
    public IReadOnlyDictionary<string, FieldError[]> Errors { get; }

    private ValidationError(IReadOnlyDictionary<string, FieldError[]> errors)
        : base("ValidationError", "One or more validation errors occurred.", 400)
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a validation error from a dictionary of field messages (from FluentValidation pipeline).
    /// All field error codes default to "ValidationError".
    /// </summary>
    public static ValidationError FromDictionary(IDictionary<string, string[]> errors) =>
        new(
            errors
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(msg => new FieldError("ValidationError", msg)).ToArray()
                )
                .AsReadOnly()
        );

    /// <summary>
    /// Creates a validation error for a single field with an <see cref="ErrorCode"/>.
    /// </summary>
    public static ValidationError ForField(string field, ErrorCode code, string? message = null) =>
        new(
            new Dictionary<string, FieldError[]>
            {
                [field] = [new FieldError(code.ToString(), message ?? code.GetDefaultMessage())],
            }.AsReadOnly()
        );

    /// <summary>
    /// Creates a validation error for multiple fields.
    /// </summary>
    public static ValidationError ForFields(params (string Field, ErrorCode Code, string? Message)[] fieldErrors) =>
        new(
            fieldErrors
                .GroupBy(e => e.Field)
                .ToDictionary(
                    g => g.Key,
                    g =>
                        g.Select(e => new FieldError(e.Code.ToString(), e.Message ?? e.Code.GetDefaultMessage()))
                            .ToArray()
                )
                .AsReadOnly()
        );
}
