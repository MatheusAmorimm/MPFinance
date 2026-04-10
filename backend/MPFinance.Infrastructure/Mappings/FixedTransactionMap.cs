using MPFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MPFinance.Infrastructure.Mappings;

public class FixedTransactionMap : IEntityTypeConfiguration<FixedTransaction>
{
    public void Configure(EntityTypeBuilder<FixedTransaction> builder)
    {
        builder.ToTable("fixed_transactions");

        builder.HasKey(ft => ft.Id);
        builder.Property(ft => ft.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(ft => ft.Description).HasMaxLength(255);
        builder.Property(ft => ft.DayOfMonth).IsRequired();

        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(ft => ft.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
