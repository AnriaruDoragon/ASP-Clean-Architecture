using Application.Common.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor that automatically sets audit fields on entities
/// when they are added or modified.
/// </summary>
public class AuditableEntityInterceptor(
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null)
            return;

        IEnumerable<EntityEntry<AuditableEntity>> entries = context.ChangeTracker
            .Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            DateTime now = dateTimeProvider.UtcNow;
            Guid? userId = currentUserService.UserId;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedBy = userId;
                    break;
            }
        }
    }
}
