namespace MPFinance.Domain.Entities;

public class FixedTransaction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public int DayOfMonth { get; private set; }

    public FixedTransaction(Guid userId, Guid categoryId, decimal amount, string description, int dayOfMonth)
    {
        if (dayOfMonth < 1 || dayOfMonth > 31) throw new ArgumentException("Dia inválido.");
        
        Id = Guid.NewGuid();
        UserId = userId;
        CategoryId = categoryId;
        Amount = amount;
        Description = description;
        DayOfMonth = dayOfMonth;
    }
}