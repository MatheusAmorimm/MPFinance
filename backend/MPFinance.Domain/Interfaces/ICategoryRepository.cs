using MPFinance.Domain.Entities;
using MPFinance.Domain.Enums;

namespace MPFinance.Domain.Interfaces;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type);
}
