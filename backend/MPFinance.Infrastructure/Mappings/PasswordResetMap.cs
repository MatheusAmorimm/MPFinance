using MPFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MPFinance.Infrastructure.Mappings;

public class PasswordResetMap : IEntityTypeConfiguration<PasswordReset>
{
    public void Configure(EntityTypeBuilder<PasswordReset> builder)
    {
        builder.ToTable("password_resets");

        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Token).IsRequired().HasMaxLength(255);
        builder.Property(pr => pr.ExpiresAt).IsRequired();
    }
}
