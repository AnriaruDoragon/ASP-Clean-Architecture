using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Event raised when a product runs out of stock.
/// </summary>
public sealed record ProductOutOfStockEvent(Guid ProductId, string ProductName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
