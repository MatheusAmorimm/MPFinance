using MPFinance.Domain.Entities;
using MPFinance.Domain.Interfaces;
using MPFinance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MPFinance.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MPFinanceDbContext _context;

    public UserRepository(MPFinanceDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id) => await _context.Users.FindAsync(id);

    public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.ToListAsync();

    public async Task AddAsync(User entity) => await _context.Users.AddAsync(entity);

    public void Update(User entity) => _context.Users.Update(entity);

    public void Delete(User entity) => _context.Users.Remove(entity);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
