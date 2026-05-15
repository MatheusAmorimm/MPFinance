using MPFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MPFinance.Infrastructure.Mappings;

public class TransactionMap : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(255);
        builder.Property(t => t.Date).IsRequired();
        builder.Property(t => t.IsFixed).HasDefaultValue(false);
        builder.Property(t => t.GoalId)
               .HasColumnType("char(36)")
               .IsRequired(false);

        builder.Property(t => t.IsGoalDeposit)
               .HasColumnType("tinyint(1)")
               .HasDefaultValue(false);

        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(t => t.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}