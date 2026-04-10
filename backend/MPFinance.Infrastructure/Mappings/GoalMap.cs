using MPFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MPFinance.Infrastructure.Mappings;

public class GoalMap : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Title).IsRequired().HasMaxLength(150);
        builder.Property(g => g.TargetAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(g => g.CurrentAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(g => g.Deadline).IsRequired();
        builder.Property(g => g.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
