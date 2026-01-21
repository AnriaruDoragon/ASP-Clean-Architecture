namespace Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain
/// that domain experts care about.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; }
}
