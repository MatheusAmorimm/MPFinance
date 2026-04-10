using MPFinance.Domain.Entities;

namespace MPFinance.Domain.Interfaces;

public interface ITransactionRepository : IBaseRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, int month, int year);
}