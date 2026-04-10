using MPFinance.Domain.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace MPFinance.Infrastructure.Security;

public class BCryptHasher : IPasswordHasher
{
    // O BCrypt já cuida do Salt internamente
    public string Hash(string password) => BC.HashPassword(password);

    public bool Verify(string password, string passwordHash) => 
        BC.Verify(password, passwordHash);
}