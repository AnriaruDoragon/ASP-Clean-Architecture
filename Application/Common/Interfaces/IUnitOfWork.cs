namespace Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface.
/// Coordinates the work of multiple repositories and ensures
/// all changes are committed as a single transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <returns>The number of entities written to the database.</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
