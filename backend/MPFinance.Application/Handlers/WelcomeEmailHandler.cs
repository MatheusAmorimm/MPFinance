using MediatR;
using MPFinance.Domain.Events;

namespace MPFinance.Application.Handlers;

public class WelcomeEmailHandler : INotificationHandler<UserRegisteredEvent>
{
    public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Aqui entraria a lógica de envio de e-mail (usando seu IEmailService)
        Console.WriteLine($"[Observer] Enviando e-mail de boas-vindas para: {notification.User.Email}");
        return Task.CompletedTask;
    }
}