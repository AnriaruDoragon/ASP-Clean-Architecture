using Application.Common.Events;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Products.Events;

/// <summary>
/// Example domain event handler for ProductOutOfStockEvent.
/// Demonstrates handling stock-related business events.
/// </summary>
public sealed class ProductOutOfStockEventHandler(ILogger<ProductOutOfStockEventHandler> logger)
    : INotificationHandler<DomainEventNotification<ProductOutOfStockEvent>>
{
    public Task Handle(DomainEventNotification<ProductOutOfStockEvent> notification, CancellationToken cancellationToken)
    {
        ProductOutOfStockEvent domainEvent = notification.DomainEvent;

        logger.LogWarning(
            "Product out of stock: {ProductId} - {ProductName} at {OccurredOn}",
            domainEvent.ProductId,
            domainEvent.ProductName,
            domainEvent.OccurredOn);

        // Add your business logic here, for example:
        // - Notify inventory management
        // - Alert purchasing department
        // - Update product availability status
        // - Send notifications to subscribed customers

        return Task.CompletedTask;
    }
}
