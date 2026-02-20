using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common.Models;
using Domain.Exceptions;
using Web.API.Models;

namespace Web.API.Middlewares;

/// <summary>
/// Global exception handling middleware that converts exceptions to RFC 7807 Problem Details responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);

            // Intercept bare 401/403 responses from auth middleware (no body written)
            if (!context.Response.HasStarted && context.Response.ContentLength is null or 0)
            {
                switch (context.Response.StatusCode)
                {
                    case StatusCodes.Status401Unauthorized:
                        await WriteErrorResponseAsync(context, ErrorCode.NotAuthenticated);
                        break;
                    case StatusCodes.Status403Forbidden:
                        await WriteErrorResponseAsync(context, ErrorCode.Forbidden);
                        break;
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ErrorCode errorCode = exception switch
        {
            NotFoundException => ErrorCode.NotFound,
            DomainException => ErrorCode.InternalServerError,
            UnauthorizedAccessException => ErrorCode.Unauthorized,
            _ => ErrorCode.InternalServerError,
        };

        await WriteErrorResponseAsync(context, errorCode, exception.Message);
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, ErrorCode errorCode, string? detail = null)
    {
        string correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;
        int statusCode = errorCode.GetStatusCode();

        var problemDetails = new ErrorProblemDetails
        {
            Type = ProblemDetailsHelper.GetTypeUri(statusCode),
            Status = statusCode,
            Title = ProblemDetailsHelper.GetTitle(statusCode),
            Detail = detail ?? errorCode.GetDefaultMessage(),
            Instance = context.Request.Path,
            ErrorCode = errorCode,
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = correlationId;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, s_jsonOptions));
    }
}

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}
