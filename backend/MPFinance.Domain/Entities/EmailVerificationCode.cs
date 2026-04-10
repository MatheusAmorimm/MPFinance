namespace MPFinance.Domain.Entities;

public class EmailVerificationCode
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Code { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public User? User { get; set; }

    public EmailVerificationCode(Guid userId, string code, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Code = code;
        ExpiresAt = expiresAt;
    }
}