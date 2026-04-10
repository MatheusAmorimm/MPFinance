using MPFinance.Domain.Entities;
using MPFinance.Domain.Enums;
using MPFinance.Domain.Interfaces;
using MPFinance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MPFinance.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly MPFinanceDbContext _context;

    public CategoryRepository(MPFinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id) => await _context.Categories.FindAsync(id);

    public async Task<IEnumerable<Category>> GetAllAsync() => await _context.Categories.ToListAsync();

    public async Task AddAsync(Category entity) => await _context.Categories.AddAsync(entity);

    public void Update(Category entity) => _context.Categories.Update(entity);

    public void Delete(Category entity) => _context.Categories.Remove(entity);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type)
    {
        return await _context.Categories
            .Where(c => c.Type == type)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
