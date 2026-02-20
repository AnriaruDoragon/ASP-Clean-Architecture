using System.Reflection;

namespace Application.Common.Models;

/// <summary>
/// Standardized error codes for API responses.
/// These codes are included in error responses for client-side handling and i18n.
/// Decorate each value with [ErrorInfo(statusCode, defaultMessage)].
/// </summary>
public enum ErrorCode
{
    // Validation errors (400)
    [ErrorInfo(400, "Invalid email or password.")]
    InvalidCredentials,

    [ErrorInfo(400, "Invalid or expired password reset token.")]
    InvalidPasswordResetToken,

    [ErrorInfo(400, "Invalid or expired email verification token.")]
    InvalidEmailVerificationToken,

    // Authentication errors (401)
    [ErrorInfo(401, "Authentication is required.")]
    NotAuthenticated,

    [ErrorInfo(401, "Invalid or expired access token.")]
    InvalidToken,

    [ErrorInfo(401, "Invalid or expired refresh token.")]
    InvalidRefreshToken,

    [ErrorInfo(401, "User not found.")]
    UserNotFound,

    /// <summary>
    /// Used by the FluentValidation pipeline. Always includes an <c>errors</c> dictionary in the response.
    /// Do not use directly in command handlers — use a specific error code instead.
    /// </summary>
    [ErrorInfo(400, "One or more validation errors occurred.")]
    ValidationFailed,

    // Generic errors
    [ErrorInfo(404, "The requested resource was not found.")]
    NotFound,

    [ErrorInfo(401, "You are not authorized to perform this action.")]
    Unauthorized,

    [ErrorInfo(403, "You do not have permission to access this resource.")]
    Forbidden,

    // Conflict errors (409)
    [ErrorInfo(409, "Email is already registered.")]
    EmailTaken,

    [ErrorInfo(409, "Email is already verified.")]
    EmailAlreadyVerified,

    // Server errors (500)
    [ErrorInfo(500, "An unexpected error occurred.")]
    InternalServerError,
}

/// <summary>
/// Attribute to define default HTTP status code and message for an <see cref="ErrorCode"/> value.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ErrorInfoAttribute(int statusCode, string defaultMessage) : Attribute
{
    public int StatusCode { get; } = statusCode;
    public string DefaultMessage { get; } = defaultMessage;
}

/// <summary>
/// Extension methods for <see cref="ErrorCode"/> enum.
/// </summary>
public static class ErrorCodeExtensions
{
    private static readonly Dictionary<ErrorCode, ErrorInfoAttribute> s_cache = [];

    static ErrorCodeExtensions()
    {
        foreach (ErrorCode code in Enum.GetValues<ErrorCode>())
        {
            ErrorInfoAttribute? attr = typeof(ErrorCode)
                .GetField(code.ToString())!
                .GetCustomAttribute<ErrorInfoAttribute>();

            s_cache[code] = attr ?? new ErrorInfoAttribute(500, "An error occurred.");
        }
    }

    extension(ErrorCode code)
    {
        public int GetStatusCode() => s_cache[code].StatusCode;

        public string GetDefaultMessage() => s_cache[code].DefaultMessage;
    }
}
