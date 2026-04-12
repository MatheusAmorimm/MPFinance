using MPFinance.Domain.Entities;

namespace MPFinance.Domain.Interfaces;

public interface IFixedTransactionRepository : IBaseRepository<FixedTransaction>
{
    Task<IEnumerable<FixedTransaction>> GetByUserIdAsync(Guid userId);
}
