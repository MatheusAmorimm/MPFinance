using MPFinance.Domain.Entities;

namespace MPFinance.Domain.Interfaces;

public interface IGoalRepository : IBaseRepository<Goal>
{
    Task<IEnumerable<Goal>> GetByUserIdAsync(Guid userId);
}
