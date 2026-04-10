using MPFinance.Domain.Entities;
using MPFinance.Domain.Interfaces;
using MPFinance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MPFinance.Infrastructure.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly MPFinanceDbContext _context;

    public GoalRepository(MPFinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Goal?> GetByIdAsync(Guid id) => await _context.Goals.FindAsync(id);

    public async Task<IEnumerable<Goal>> GetAllAsync() => await _context.Goals.ToListAsync();

    public async Task AddAsync(Goal entity) => await _context.Goals.AddAsync(entity);

    public void Update(Goal entity) => _context.Goals.Update(entity);

    public void Delete(Goal entity) => _context.Goals.Remove(entity);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<Goal>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.Deadline)
            .ToListAsync();
    }
}
