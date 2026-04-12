using MPFinance.Domain.Entities;
using MPFinance.Domain.Interfaces;
using MPFinance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MPFinance.Infrastructure.Repositories;

public class FixedTransactionRepository : IFixedTransactionRepository
{
    private readonly MPFinanceDbContext _context;

    public FixedTransactionRepository(MPFinanceDbContext context)
    {
        _context = context;
    }

    public async Task<FixedTransaction?> GetByIdAsync(Guid id) =>
        await _context.FixedTransactions.FindAsync(id);

    public async Task<IEnumerable<FixedTransaction>> GetAllAsync() =>
        await _context.FixedTransactions.ToListAsync();

    public async Task AddAsync(FixedTransaction entity) =>
        await _context.FixedTransactions.AddAsync(entity);

    public void Update(FixedTransaction entity) =>
        _context.FixedTransactions.Update(entity);

    public void Delete(FixedTransaction entity) =>
        _context.FixedTransactions.Remove(entity);

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public async Task<IEnumerable<FixedTransaction>> GetByUserIdAsync(Guid userId)
    {
        return await _context.FixedTransactions
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.DayOfMonth)
            .ToListAsync();
    }
}
