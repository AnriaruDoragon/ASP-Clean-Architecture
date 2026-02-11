using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class UserConfiguration : AuditableEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);

        builder.Property(u => u.EmailVerified).IsRequired().HasDefaultValue(false);

        // Store roles as comma-separated integers
        builder
            .Property(u => u.Roles)
            .HasConversion(
                roles => string.Join(',', roles.Select(r => (int)r)),
                value =>
                    value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(v => (Role)int.Parse(v)).ToList()
            )
            .HasColumnName("Roles")
            .Metadata.SetValueComparer(
                new ValueComparer<IReadOnlyList<Role>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                )
            );

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
