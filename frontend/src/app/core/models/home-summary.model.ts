export interface UpcomingBill {
    id: string;
    description: string;
    amount: number;
    dayOfMonth: number;
}

export interface RecentTransaction {
    id: string;
    description: string;
    amount: number;
    date: string;
    type: 'income' | 'expense' | 'goal';
    categoryName: string;
}

export interface HomeSummary {
    totalIncome: number;
    totalExpenses: number;
    totalGoalInvestments?: number;
    balance: number;
    upcomingBills: UpcomingBill[];
    recentTransactions: RecentTransaction[];
}
