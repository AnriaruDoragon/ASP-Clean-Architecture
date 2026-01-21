using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Event raised when a new product is created.
/// </summary>
public sealed record ProductCreatedEvent(Guid ProductId, string ProductName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
