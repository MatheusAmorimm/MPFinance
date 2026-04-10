using MPFinance.Domain.Entities;

namespace MPFinance.Domain.Interfaces;

public interface IEmailVerificationRepository
{
    Task AddAsync(EmailVerificationCode code);
    Task<EmailVerificationCode?> GetValidCodeAsync(Guid userId, string code);
    Task DeleteAllForUserAsync(Guid userId);
    Task<int> SaveChangesAsync();
}
