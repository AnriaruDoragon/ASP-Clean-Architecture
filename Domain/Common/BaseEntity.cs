namespace Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides identity and domain event support.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched when the entity is persisted.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Removes a domain event from the collection.
    /// </summary>
    protected void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <summary>
    /// Clears all domain events. Called after events are dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
