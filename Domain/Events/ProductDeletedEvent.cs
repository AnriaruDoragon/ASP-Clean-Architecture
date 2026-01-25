using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Event raised when a product is deleted (soft delete).
/// </summary>
public sealed record ProductDeletedEvent(Guid ProductId, string ProductName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
