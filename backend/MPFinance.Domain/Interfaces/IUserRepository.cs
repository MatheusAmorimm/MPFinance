using MPFinance.Domain.Entities;

namespace MPFinance.Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
