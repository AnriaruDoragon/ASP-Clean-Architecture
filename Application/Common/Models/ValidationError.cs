namespace Application.Common.Models;

/// <summary>
/// Represents a single field-level error with an optional code and human-readable message.
/// </summary>
public sealed record FieldError(string? Code, string Message);

/// <summary>
/// Represents a validation error with field-level details.
/// Used exclusively by the FluentValidation pipeline.
/// </summary>
public sealed record ValidationError : Error
{
    public IReadOnlyDictionary<string, FieldError[]> Errors { get; }

    private ValidationError(IReadOnlyDictionary<string, FieldError[]> errors)
        : base(nameof(ErrorCode.ValidationFailed), "One or more validation errors occurred.", 400)
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a validation error from a dictionary of field messages (from FluentValidation pipeline).
    /// </summary>
    public static ValidationError FromDictionary(IDictionary<string, string[]> errors) =>
        new(
            errors
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(msg => new FieldError(null, msg)).ToArray())
                .AsReadOnly()
        );
}
