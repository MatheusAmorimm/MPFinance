namespace MPFinance.Application.DTOs;

public record HomeSummaryResponse(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal TotalGoalInvestments,
    decimal Balance,
    IEnumerable<UpcomingBillDto> UpcomingBills,
    IEnumerable<RecentTransactionDto> RecentTransactions
);

public record UpcomingBillDto(
    Guid Id,
    string Description,
    decimal Amount,
    int DayOfMonth
);

public record RecentTransactionDto(
    Guid Id,
    string Description,
    decimal Amount,
    DateTime Date,
    string Type,
    string CategoryName
);
