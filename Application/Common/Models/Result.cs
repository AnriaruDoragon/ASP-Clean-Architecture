namespace Application.Common.Models;

/// <summary>
/// Represents the result of an operation that may succeed or fail.
/// Use this instead of throwing exceptions for expected failure cases.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error, int statusCode = 200)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    /// <summary>
    /// The HTTP status code for this result.
    /// For successes: 200, 201, or 202. For failures: driven by <see cref="Error.StatusCode"/>.
    /// </summary>
    public int StatusCode { get; }

    public static Result Success(int statusCode = 200) => new(true, Error.None, statusCode);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value, int statusCode = 200) =>
        new(value, true, Error.None, statusCode);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Result<TValue> : Result
{
    protected internal Result(TValue? value, bool isSuccess, Error error, int statusCode = 200)
        : base(isSuccess, error, statusCode)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value if the result is successful.
    /// Throws if the result is a failure.
    /// </summary>
    public TValue Value =>
        IsSuccess ? field! : throw new InvalidOperationException("Cannot access value of a failed result.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}
