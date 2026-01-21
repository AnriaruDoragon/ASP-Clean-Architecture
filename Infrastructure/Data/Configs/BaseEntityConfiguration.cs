using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

/// <summary>
/// Base configuration for all entities inheriting from BaseEntity.
/// Apply this configuration by calling Configure in your entity configurations.
/// </summary>
public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever(); // Id is set by the entity

        // Ignore domain events - they are not persisted
        builder.Ignore(e => e.DomainEvents);
    }
}

/// <summary>
/// Base configuration for auditable entities.
/// Extends BaseEntityConfiguration with audit field configurations.
/// </summary>
public abstract class AuditableEntityConfiguration<T> : BaseEntityConfiguration<T>
    where T : AuditableEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.CreatedAt)
            .IsRequired();
    }
}
