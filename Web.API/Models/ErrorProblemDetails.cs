using System.Text.Json.Serialization;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Models;

/// <summary>
/// Extended <see cref="ProblemDetails"/> with an error code and optional field-level validation errors.
/// Used as the standard error response shape for OpenAPI documentation.
/// </summary>
public sealed class ErrorProblemDetails : ProblemDetails
{
    [JsonPropertyName("errorCode")]
    public ErrorCode ErrorCode { get; set; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, FieldErrorDetail[]>? Errors { get; set; }
}

internal static class ProblemDetailsHelper
{
    internal static string GetTitle(int statusCode) =>
        statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            500 => "Internal Server Error",
            _ => "Error",
        };

    internal static string GetTypeUri(int statusCode) =>
        statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            500 => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            _ => "https://tools.ietf.org/html/rfc9110#section-15",
        };
}

/// <summary>
/// A single field-level validation error in the response.
/// </summary>
public sealed class FieldErrorDetail
{
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = null!;
}
