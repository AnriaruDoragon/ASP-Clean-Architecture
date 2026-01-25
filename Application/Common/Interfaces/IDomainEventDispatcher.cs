using Domain.Common;

namespace Application.Common.Interfaces;

/// <summary>
/// Dispatches domain events to their handlers.
/// Implemented in Infrastructure layer using MediatR.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a domain event to all registered handlers.
    /// </summary>
    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches multiple domain events to their handlers.
    /// </summary>
    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
