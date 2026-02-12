using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Models;

/// <summary>
/// Extended <see cref="ProblemDetails"/> with an error code and optional field-level validation errors.
/// Used as the standard error response shape for OpenAPI documentation.
/// </summary>
public sealed class ErrorProblemDetails : ProblemDetails
{
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = null!;

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, FieldErrorDetail[]>? Errors { get; set; }
}

/// <summary>
/// A single field-level validation error in the response.
/// </summary>
public sealed class FieldErrorDetail
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = null!;
}
