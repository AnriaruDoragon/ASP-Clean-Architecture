using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("RefreshTokens");

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rt => rt.DeviceName)
            .HasMaxLength(256);

        builder.Property(rt => rt.UserAgent)
            .HasMaxLength(512);

        // Index for token lookup
        builder.HasIndex(rt => rt.Token);

        // Index for cleanup of expired/revoked tokens
        builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiresAt });

        // Foreign key to User (no navigation property to keep RefreshToken lightweight)
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
