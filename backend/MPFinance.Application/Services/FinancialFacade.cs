using MPFinance.Domain.Interfaces;
using MPFinance.Domain.Factories;
using MPFinance.Domain.Entities;
using MPFinance.Application.DTOs;
using MPFinance.Domain.Events;
using MediatR;

namespace MPFinance.Application.Services;

public class FinancialFacade
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IGoalRepository _goalRepo;
    private readonly ICategoryRepository _categoryRepo;

    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IMediator _mediator;

    public FinancialFacade(
        ITransactionRepository transactionRepo,
        IGoalRepository goalRepo,
        ICategoryRepository categoryRepo,
        IUserRepository userRepo,
        IPasswordHasher hasher,
        IMediator mediator)
    {
        _transactionRepo = transactionRepo;
        _goalRepo = goalRepo;
        _categoryRepo = categoryRepo;
        _userRepo = userRepo;
        _hasher = hasher;
        _mediator = mediator;
    }

    /// <summary>
    /// Facade para realizar um lançamento e atualizar metas se necessário
    /// </summary>
    public async Task CreateTransactionWithImpactAsync(Guid userId, Guid catId, decimal amount, string desc, DateTime date, Guid? goalId = null)
    {
        // 1. Criar a transação usando a Factory (Pattern de Criação)
        var transaction = TransactionFactory.CreateCommon(userId, catId, amount, desc, date);

        // 2. Salvar a transação
        await _transactionRepo.AddAsync(transaction);

        // 3. Se houver vínculo com Meta, atualizar o progresso (Orquestração do Facade)
        if (goalId.HasValue)
        {
            var goal = await _goalRepo.GetByIdAsync(goalId.Value);
            if (goal != null)
            {
                goal.Deposit(amount); // Regra de negócio encapsulada na entidade
                _goalRepo.Update(goal);
            }
        }

        // 4. Persistir todas as mudanças (Atomicidade)
        await _transactionRepo.SaveChangesAsync();
    }

    public async Task RegisterUser(UserRequest request)
    {
        var user = new User(request.Name, request.Email, _hasher.Hash(request.Password));
        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        // O Observer entra em ação aqui:
        await _mediator.Publish(new UserRegisteredEvent(user));
    }
}