using MediatR;
using MPFinance.Domain.Entities;

namespace MPFinance.Domain.Events;

// O "Aviso" de que algo aconteceu
public record UserRegisteredEvent(User User) : INotification;