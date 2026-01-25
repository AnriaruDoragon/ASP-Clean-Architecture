using Application.Common.Events;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.Events;

/// <summary>
/// Example domain event handler for ProductCreatedEvent.
/// Demonstrates how to handle domain events in the Application layer.
/// </summary>
public sealed class ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<ProductCreatedEvent>>
{
    public Task Handle(DomainEventNotification<ProductCreatedEvent> notification, CancellationToken cancellationToken)
    {
        ProductCreatedEvent domainEvent = notification.DomainEvent;

        logger.LogInformation(
            "Product created: {ProductId} - {ProductName} at {OccurredOn}",
            domainEvent.ProductId,
            domainEvent.ProductName,
            domainEvent.OccurredOn);

        // Add your business logic here, for example:
        // - Send notification emails
        // - Update search indexes
        // - Publish to message queue
        // - Invalidate caches

        return Task.CompletedTask;
    }
}
