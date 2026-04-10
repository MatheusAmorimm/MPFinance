namespace MPFinance.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public bool IsFixed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Transaction(Guid userId, Guid categoryId, decimal amount, string description, DateTime date, bool isFixed = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CategoryId = categoryId;
        Amount = amount;
        Description = description;
        Date = date;
        IsFixed = isFixed;
        CreatedAt = DateTime.UtcNow;
    }
}