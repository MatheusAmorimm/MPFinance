using MPFinance.Domain.Interfaces;
using MPFinance.Domain.Factories;
using MPFinance.Domain.Entities;
using MPFinance.Domain.Enums;
using MPFinance.Application.DTOs;
using MPFinance.Domain.Events;
using MediatR;

namespace MPFinance.Application.Services;

public class FinancialFacade
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IGoalRepository _goalRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IFixedTransactionRepository _fixedTransactionRepo;
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IMediator _mediator;

    public FinancialFacade(
        ITransactionRepository transactionRepo,
        IGoalRepository goalRepo,
        ICategoryRepository categoryRepo,
        IFixedTransactionRepository fixedTransactionRepo,
        IUserRepository userRepo,
        IPasswordHasher hasher,
        IMediator mediator)
    {
        _transactionRepo = transactionRepo;
        _goalRepo = goalRepo;
        _categoryRepo = categoryRepo;
        _fixedTransactionRepo = fixedTransactionRepo;
        _userRepo = userRepo;
        _hasher = hasher;
        _mediator = mediator;
    }

    // ─── Queries ─────────────────────────────────────────────────────────────

    public async Task<HomeSummaryResponse> GetMonthlySummaryAsync(Guid userId, int month, int year, int currentDay)
    {
        var all = await BuildTransactionDtosAsync(userId, month, year);

        decimal income      = all.Where(t => t.Type == "income").Sum(t => t.Amount);
        decimal expenses    = all.Where(t => t.Type == "expense").Sum(t => t.Amount);
        decimal goalInvest  = all.Where(t => t.Type == "goal").Sum(t => t.Amount);

        var fixedTransactions = await _fixedTransactionRepo.GetByUserIdAsync(userId);
        var upcomingBills = fixedTransactions
            .Where(f => f.DayOfMonth >= currentDay)
            .OrderBy(f => f.DayOfMonth)
            .Select(f => new UpcomingBillDto(f.Id, f.Description, f.Amount, f.DayOfMonth))
            .ToList();

        return new HomeSummaryResponse(
            income,
            expenses,
            goalInvest,
            income - expenses - goalInvest,
            upcomingBills,
            all.Take(5).ToList()
        );
    }

    /// <summary>
    /// Retorna todas as transações do mês com nome e tipo da categoria resolvidos.
    /// Usado pela tela de Lançamentos (visão planilha).
    /// </summary>
    public async Task<IEnumerable<RecentTransactionDto>> GetUserTransactionsAsync(Guid userId, int month, int year)
    {
        return await BuildTransactionDtosAsync(userId, month, year);
    }

    // ─── Commands ────────────────────────────────────────────────────────────

    public async Task CreateTransactionWithImpactAsync(
        Guid userId, Guid catId, decimal amount, string desc, DateTime date, Guid? goalId = null)
    {
        var transaction = TransactionFactory.CreateCommon(userId, catId, amount, desc, date, goalId);
        await _transactionRepo.AddAsync(transaction);

        if (goalId.HasValue)
        {
            var goal = await _goalRepo.GetByIdAsync(goalId.Value);
            if (goal != null)
            {
                goal.Deposit(amount);
                _goalRepo.Update(goal);
            }
        }

        await _transactionRepo.SaveChangesAsync();
    }

    public async Task<bool> UpdateTransactionAsync(Guid userId, Guid transactionId, decimal amount, string desc, DateTime date)
    {
        var transaction = await _transactionRepo.GetByIdAsync(transactionId);
        if (transaction == null || transaction.UserId != userId) return false;

        if (transaction.GoalId.HasValue)
        {
            var goal = await _goalRepo.GetByIdAsync(transaction.GoalId.Value);
            if (goal != null)
            {
                var diff = amount - transaction.Amount;
                if (diff > 0) goal.Deposit(diff);
                else if (diff < 0) goal.Withdraw(-diff);
                _goalRepo.Update(goal);
            }
        }

        transaction.Update(amount, desc, date);
        _transactionRepo.Update(transaction);
        await _transactionRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTransactionAsync(Guid userId, Guid transactionId)
    {
        var transaction = await _transactionRepo.GetByIdAsync(transactionId);
        if (transaction == null || transaction.UserId != userId) return false;

        if (transaction.GoalId.HasValue)
        {
            var goal = await _goalRepo.GetByIdAsync(transaction.GoalId.Value);
            if (goal != null)
            {
                goal.Withdraw(transaction.Amount);
                _goalRepo.Update(goal);
            }
        }

        _transactionRepo.Delete(transaction);
        await _transactionRepo.SaveChangesAsync();
        return true;
    }

    public async Task RegisterUser(UserRequest request)
    {
        var user = new User(request.Name, request.Email, _hasher.Hash(request.Password));
        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();
        await _mediator.Publish(new UserRegisteredEvent(user));
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private async Task<List<RecentTransactionDto>> BuildTransactionDtosAsync(Guid userId, int month, int year)
    {
        var transactions = (await _transactionRepo.GetByUserIdAsync(userId, month, year)).ToList();
        var categoryMap  = (await _categoryRepo.GetAllAsync()).ToDictionary(c => c.Id);

        return transactions
            .Where(t => categoryMap.ContainsKey(t.CategoryId))
            .Select(t =>
            {
                var cat  = categoryMap[t.CategoryId];
                var type = t.IsGoalDeposit
                    ? "goal"
                    : (cat.Type == TransactionType.Income ? "income" : "expense");
                return new RecentTransactionDto(t.Id, t.Description, t.Amount, t.Date, type, cat.Name);
            })
            .OrderByDescending(t => t.Date)
            .ToList();
    }
}
