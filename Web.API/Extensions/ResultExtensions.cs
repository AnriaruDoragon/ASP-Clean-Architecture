using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Web.API.Models;

namespace Web.API.Extensions;

/// <summary>
/// Extension methods to convert Result objects to IActionResult responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an appropriate IActionResult.
    /// </summary>
    public static IActionResult ToActionResult(this Result result) =>
        result.IsSuccess ? new OkResult() : ToProblemDetails(result.Error);

    extension<T>(Result<T> result)
    {
        /// <summary>
        /// Converts a Result&lt;T&gt; to an appropriate IActionResult.
        /// </summary>
        public IActionResult ToActionResult() =>
            result.IsSuccess ? new OkObjectResult(result.Value) : ToProblemDetails(result.Error);

        /// <summary>
        /// Converts a Result&lt;T&gt; to an appropriate IActionResult with a custom success response.
        /// </summary>
        public IActionResult ToActionResult(Func<T, IActionResult> onSuccess) =>
            result.IsSuccess ? onSuccess(result.Value) : ToProblemDetails(result.Error);
    }

    private static ObjectResult ToProblemDetails(Error error)
    {
        _ = Enum.TryParse(error.Code, out ErrorCode errorCode);

        var problemDetails = new ErrorProblemDetails
        {
            Type = GetTypeUri(error.StatusCode),
            Title = GetTitle(error.StatusCode),
            Status = error.StatusCode,
            Detail = error.Description,
            ErrorCode = errorCode,
        };

        // ValidationError from FluentValidation — multiple fields, no per-field codes
        if (error is ValidationError validationError)
        {
            problemDetails.Errors = validationError.Errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(e => new FieldErrorDetail { Code = e.Code, Detail = e.Message }).ToArray()
            );
        }
        // Error.From() with a field — single field with code
        else if (error.Field is not null)
        {
            problemDetails.Errors = new Dictionary<string, FieldErrorDetail[]>
            {
                [error.Field] = [new FieldErrorDetail { Code = error.Code, Detail = error.Description }],
            };
        }

        // Copy any custom extensions from the Error to ProblemDetails
        if (error.Extensions is not { Count: > 0 })
        {
            return new ObjectResult(problemDetails)
            {
                StatusCode = error.StatusCode,
                ContentTypes = { "application/problem+json" },
            };
        }

        foreach ((string key, object? value) in error.Extensions)
            problemDetails.Extensions[key] = value;

        return new ObjectResult(problemDetails)
        {
            StatusCode = error.StatusCode,
            ContentTypes = { "application/problem+json" },
        };
    }

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            _ => "Error",
        };

    private static string GetTypeUri(int statusCode) =>
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
