namespace MPFinance.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EmailChangedAt { get; private set; }
    public string? PendingEmail { get; private set; }

    public User(string name, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        IsVerified = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail() => IsVerified = true;

    public void UpdatePassword(string newHash) => PasswordHash = newHash;

    public void StartEmailChange(string pendingEmail) => PendingEmail = pendingEmail;

    public void CompleteEmailChange()
    {
        if (string.IsNullOrWhiteSpace(PendingEmail)) return;
        Email = PendingEmail;
        PendingEmail = null;
        EmailChangedAt = DateTime.UtcNow;
    }

    public void ClearPendingEmail() => PendingEmail = null;
}
