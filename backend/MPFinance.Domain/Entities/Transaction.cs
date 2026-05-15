namespace MPFinance.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? GoalId { get; set; }
    public bool IsGoalDeposit { get; set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public bool IsFixed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Transaction() { Description = string.Empty; }

    public void Update(decimal amount, string description, DateTime date)
    {
        Amount      = amount;
        Description = description;
        Date        = date;
    }

    public Transaction(Guid userId, Guid categoryId, decimal amount, string description, DateTime date, bool isFixed = false, Guid? goalId = null)
    {
        Id             = Guid.NewGuid();
        UserId         = userId;
        CategoryId     = categoryId;
        GoalId         = goalId;
        IsGoalDeposit  = goalId.HasValue;
        Amount         = amount;
        Description    = description;
        Date           = date;
        IsFixed        = isFixed;
        CreatedAt      = DateTime.UtcNow;
    }
}