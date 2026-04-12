using MPFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MPFinance.Infrastructure.Mappings;

public class UserMap : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.IsVerified).HasDefaultValue(false);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(u => u.EmailChangedAt).IsRequired(false);
        builder.Property(u => u.PendingEmail).HasMaxLength(150).IsRequired(false);

        // Relacionamentos 1:N conforme a imagem
        builder.HasMany<Transaction>()
               .WithOne()
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<FixedTransaction>()
               .WithOne()
               .HasForeignKey(ft => ft.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Goal>()
               .WithOne()
               .HasForeignKey(g => g.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<PasswordReset>()
               .WithOne(pr => pr.User)
               .HasForeignKey(pr => pr.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<EmailVerificationCode>()
               .WithOne(e => e.User)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}