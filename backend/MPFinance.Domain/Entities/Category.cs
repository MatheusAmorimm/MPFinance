using MPFinance.Domain.Enums;

namespace MPFinance.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public TransactionType Type { get; private set; }

    public Category(string name, TransactionType type)
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
    }
}