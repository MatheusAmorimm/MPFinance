using MPFinance.Domain.Entities;

namespace MPFinance.Domain.Factories;

public static class TransactionFactory
{
    // Método que cria uma transação comum ou prepara os dados para uma fixa
    public static Transaction CreateCommon(Guid userId, Guid catId, decimal amount, string desc, DateTime date)
    {
        return new Transaction(userId, catId, amount, desc, date, isFixed: false);
    }

    public static FixedTransaction CreateFixed(Guid userId, Guid catId, decimal amount, string desc, int day)
    {
        return new FixedTransaction(userId, catId, amount, desc, day);
    }
}