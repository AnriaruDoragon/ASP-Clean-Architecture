using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class EmailVerificationTokenConfiguration : BaseEntityConfiguration<EmailVerificationToken>
{
    public override void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("EmailVerificationTokens");

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Index for token lookup
        builder.HasIndex(t => t.Token);

        // Index for cleanup
        builder.HasIndex(t => new { t.UserId, t.IsUsed, t.ExpiresAt });

        // Foreign key to User
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
