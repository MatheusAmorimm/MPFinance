using MPFinance.Domain.Entities;
using MPFinance.Domain.Interfaces;
using MPFinance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MPFinance.Infrastructure.Repositories;

public class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly MPFinanceDbContext _context;

    public EmailVerificationRepository(MPFinanceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailVerificationCode code)
        => await _context.EmailVerificationCodes.AddAsync(code);

    public async Task<EmailVerificationCode?> GetValidCodeAsync(Guid userId, string code)
        => await _context.EmailVerificationCodes
            .FirstOrDefaultAsync(e =>
                e.UserId == userId &&
                e.Code == code &&
                e.ExpiresAt > DateTime.UtcNow);

    public async Task DeleteAllForUserAsync(Guid userId)
    {
        var codes = await _context.EmailVerificationCodes
            .Where(e => e.UserId == userId)
            .ToListAsync();

        _context.EmailVerificationCodes.RemoveRange(codes);
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
