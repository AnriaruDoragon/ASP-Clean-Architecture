using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Common.Interfaces;

/// <summary>
/// Database context interface for dependency inversion.
/// Defined in Application layer, implemented in Infrastructure.
/// Add DbSet properties here as you create entities.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Provides access to database-related information and operations.
    /// </summary>
    public DatabaseFacade Database { get; }

    // Auth
    public DbSet<User> Users { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; }

    // Example
    public DbSet<Product> Products { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
