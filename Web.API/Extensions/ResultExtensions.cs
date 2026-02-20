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
        result.IsSuccess ? ToSuccessResult(result.StatusCode) : ToProblemDetails(result.Error);

    extension<T>(Result<T> result)
    {
        /// <summary>
        /// Converts a Result&lt;T&gt; to an appropriate IActionResult.
        /// Uses the status code carried by the result (200, 201, or 202).
        /// </summary>
        public IActionResult ToActionResult() =>
            result.IsSuccess
                ? new ObjectResult(result.Value) { StatusCode = result.StatusCode }
                : ToProblemDetails(result.Error);

        /// <summary>
        /// Converts a Result&lt;T&gt; to an appropriate IActionResult with a custom success response.
        /// </summary>
        public IActionResult ToActionResult(Func<T, IActionResult> onSuccess) =>
            result.IsSuccess ? onSuccess(result.Value) : ToProblemDetails(result.Error);
    }

    private static IActionResult ToSuccessResult(int statusCode) =>
        statusCode switch
        {
            201 => new StatusCodeResult(201),
            202 => new StatusCodeResult(202),
            _ => new OkResult(),
        };

    private static ObjectResult ToProblemDetails(Error error)
    {
        _ = Enum.TryParse(error.Code, out ErrorCode errorCode);

        var problemDetails = new ErrorProblemDetails
        {
            Type = ProblemDetailsHelper.GetTypeUri(error.StatusCode),
            Title = ProblemDetailsHelper.GetTitle(error.StatusCode),
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
        if (error.Extensions is { Count: > 0 })
        {
            foreach ((string key, object? value) in error.Extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = error.StatusCode,
            ContentTypes = { "application/problem+json" },
        };
    }
}
