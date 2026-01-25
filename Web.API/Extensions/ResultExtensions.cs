using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Extensions;

/// <summary>
/// Extension methods to convert Result objects to IActionResult responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an appropriate IActionResult.
    /// </summary>
    public static IActionResult ToActionResult(this Result result)
        => result.IsSuccess ? new OkResult() : ToProblemDetails(result.Error);

    /// <summary>
    /// Converts a Result&lt;T&gt; to an appropriate IActionResult.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result)
        => result.IsSuccess ? new OkObjectResult(result.Value) : ToProblemDetails(result.Error);

    /// <summary>
    /// Converts a Result&lt;T&gt; to an appropriate IActionResult with a custom success response.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult> onSuccess)
        => result.IsSuccess ? onSuccess(result.Value) : ToProblemDetails(result.Error);

    private static IActionResult ToProblemDetails(Error error)
    {
        // Handle validation errors with field-level details
        if (error is ValidationError validationError)
        {
            var validationProblemDetails = new ValidationProblemDetails(
                validationError.Errors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value))
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest
            };

            return new ObjectResult(validationProblemDetails)
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentTypes = { "application/problem+json" }
            };
        }

        int statusCode = error.Code switch
        {
            "Error.NotFound" => StatusCodes.Status404NotFound,
            "Error.Validation" => StatusCodes.Status400BadRequest,
            "Error.Conflict" => StatusCodes.Status409Conflict,
            "Error.Unauthorized" => StatusCodes.Status401Unauthorized,
            "Error.Forbidden" => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(error.Code),
            Detail = error.Description,
            Extensions = { ["errorCode"] = error.Code }
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static string GetTitle(string errorCode) => errorCode switch
    {
        "Error.NotFound" => "Not Found",
        "Error.Validation" => "Validation Error",
        "Error.Conflict" => "Conflict",
        "Error.Unauthorized" => "Unauthorized",
        "Error.Forbidden" => "Forbidden",
        _ => "Error"
    };
}
