using MPFinance.Domain.Entities;
using MPFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MPFinance.Infrastructure.Mappings;

public class CategoryMap : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Type)
               .IsRequired()
               .HasConversion(new ValueConverter<TransactionType, string>(
                   v => v.ToString().ToLower(),
                   v => Enum.Parse<TransactionType>(v, ignoreCase: true)
               ))
               .HasColumnType("ENUM('income', 'expense')");
    }
}
