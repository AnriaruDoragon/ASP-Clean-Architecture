using Application.Common.Events;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;

namespace Infrastructure.Events;

/// <summary>
/// Dispatches domain events via MediatR's publish mechanism.
/// </summary>
public sealed class DomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Type notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        object notification = Activator.CreateInstance(notificationType, domainEvent)!;

        await publisher.Publish(notification, cancellationToken);
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
