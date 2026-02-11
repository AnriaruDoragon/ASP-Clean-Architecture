namespace Application.Common.Models;

/// <summary>
/// Represents a validation error with field-level details.
/// </summary>
public sealed record ValidationError : Error
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    private ValidationError(IReadOnlyDictionary<string, string[]> errors)
        : base("Error.Validation", "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public static ValidationError FromDictionary(IDictionary<string, string[]> errors) => new(errors.AsReadOnly());
}
