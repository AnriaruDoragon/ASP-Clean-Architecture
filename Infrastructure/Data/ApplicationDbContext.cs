using System.Reflection;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

/// <summary>
/// Application database context.
/// Add DbSet properties for your entities here.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    // Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    // Example
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events BEFORE saving (entities may be detached after save)
        List<IDomainEvent> domainEvents = CollectDomainEvents();

        // Save changes first
        int result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch events AFTER successful save to ensure data consistency
        await DispatchDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var entities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(List<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        if (_domainEventDispatcher is not null && domainEvents.Count > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }
    }
}
