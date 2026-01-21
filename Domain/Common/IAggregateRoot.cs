namespace Domain.Common;

/// <summary>
/// Marker interface for aggregate roots.
/// An aggregate root is the entry point to an aggregate - a cluster of
/// domain objects that are treated as a single unit for data changes.
///
/// Only aggregate roots should have repositories.
/// External objects should only hold references to aggregate roots.
/// </summary>
public interface IAggregateRoot;
