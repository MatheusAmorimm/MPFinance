using MPFinance.Domain.Entities;
using MPFinance.Domain.Interfaces;
using MPFinance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MPFinance.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly MPFinanceDbContext _context;

    public TransactionRepository(MPFinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id) => await _context.Transactions.FindAsync(id);

    public async Task AddAsync(Transaction entity) => await _context.Transactions.AddAsync(entity);

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, int month, int year)
    {
        return await _context.Transactions
            .Where(t => t.UserId == userId && t.Date.Month == month && t.Date.Year == year)
            .OrderByDescending(t => t.Date)
                .ToListAsync();
    }

    // ... Outros métodos do IBaseRepository
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<Transaction>> GetAllAsync() => await _context.Transactions.ToListAsync();
    public void Update(Transaction entity) => _context.Transactions.Update(entity);
    public void Delete(Transaction entity) => _context.Transactions.Remove(entity);
}