namespace MPFinance.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Construtor para garantir que um usuário nunca nasça "inválido"
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
}