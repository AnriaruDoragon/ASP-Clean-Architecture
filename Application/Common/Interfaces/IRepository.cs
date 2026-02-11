using System.Linq.Expressions;
using Domain.Common;

namespace Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for aggregate roots.
/// Defines standard CRUD operations for data access.
/// </summary>
/// <typeparam name="T">The aggregate root type.</typeparam>
public interface IRepository<T>
    where T : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specified predicate.
    /// </summary>
    public Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    public void Update(T entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    public void Delete(T entity);

    /// <summary>
    /// Checks if any entity matches the specified predicate.
    /// </summary>
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specified predicate.
    /// </summary>
    public Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );
}
