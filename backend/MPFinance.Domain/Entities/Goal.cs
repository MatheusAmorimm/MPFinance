namespace MPFinance.Domain.Entities;

public class Goal
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public DateTime Deadline { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CoverImageUrl { get; private set; }

    public Goal(Guid userId, string title, decimal targetAmount, DateTime deadline)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        TargetAmount = targetAmount;
        CurrentAmount = 0;
        Deadline = deadline;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deposit(decimal value) => CurrentAmount += value;

    public void UpdateDetails(string title, decimal targetAmount, DateTime deadline)
    {
        Title = title;
        TargetAmount = targetAmount;
        Deadline = deadline;
    }

    public void SetCoverImage(string url) => CoverImageUrl = url;
}