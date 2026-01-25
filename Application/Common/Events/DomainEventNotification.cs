using Domain.Common;
using MediatR;

namespace Application.Common.Events;

/// <summary>
/// Wraps a domain event as a MediatR notification.
/// This allows domain events to be dispatched via MediatR without
/// adding MediatR dependency to the Domain layer.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event.</typeparam>
public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
