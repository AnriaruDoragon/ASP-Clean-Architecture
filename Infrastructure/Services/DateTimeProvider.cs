using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Default implementation of IDateTimeProvider using system time.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
