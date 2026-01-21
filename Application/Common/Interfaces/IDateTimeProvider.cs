namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction for date/time operations.
/// Allows for easier testing by mocking time-dependent operations.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    public DateTime UtcNow { get; }
}
