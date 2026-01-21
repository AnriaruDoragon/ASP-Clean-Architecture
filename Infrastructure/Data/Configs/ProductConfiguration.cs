using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

/// <summary>
/// EF Core configuration for the Product entity.
/// </summary>
public class ProductConfiguration : AuditableEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.IsActive);
    }
}
