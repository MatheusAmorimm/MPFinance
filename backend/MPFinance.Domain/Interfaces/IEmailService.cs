namespace MPFinance.Domain.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string userName, string code);
    Task SendPasswordChangeCodeAsync(string toEmail, string userName, string code);
    Task SendEmailChangeCodeAsync(string toEmail, string userName, string code, bool isNewEmail);
}
