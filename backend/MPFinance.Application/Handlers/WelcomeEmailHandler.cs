using System.Security.Cryptography;
using MediatR;
using MPFinance.Domain.Entities;
using MPFinance.Domain.Events;
using MPFinance.Domain.Interfaces;

namespace MPFinance.Application.Handlers;

public class WelcomeEmailHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IEmailVerificationRepository _verificationRepo;
    private readonly IEmailService _emailService;

    public WelcomeEmailHandler(
        IEmailVerificationRepository verificationRepo,
        IEmailService emailService)
    {
        _verificationRepo = verificationRepo;
        _emailService     = emailService;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var user = notification.User;

        // Limpa códigos antigos do usuário antes de gerar um novo
        await _verificationRepo.DeleteAllForUserAsync(user.Id);

        // Gera código de 6 dígitos e salva com validade de 15 minutos
        var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var verificationCode = new EmailVerificationCode(user.Id, code, DateTime.UtcNow.AddMinutes(15));

        await _verificationRepo.AddAsync(verificationCode);
        await _verificationRepo.SaveChangesAsync();

        try
        {
            await _emailService.SendVerificationEmailAsync(user.Email, user.Name, code);
        }
        catch (Exception ex)
        {
            // Falha no envio não deve derrubar o fluxo de cadastro
            Console.WriteLine($"[Email] Falha ao enviar verificação para {user.Email}: {ex.Message}");
        }
    }
}
