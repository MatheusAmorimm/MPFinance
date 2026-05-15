export interface Category {
    id: string;
    name: string;
    type: 'income' | 'expense';
}

export interface Goal {
    id: string;
    title: string;
    targetAmount: number;
    currentAmount: number;
    deadline: string;
    createdAt: string;
    coverImageUrl: string | null;
}

export interface Transaction {
    id: string;
    description: string;
    amount: number;
    date: string;
    type: 'income' | 'expense' | 'goal';
    categoryName: string;
}

export interface CreateTransactionPayload {
    categoryId: string;
    amount: number;
    description: string;
    date: string;
    goalId?: string;
}

export interface UpdateTransactionPayload {
    amount: number;
    description: string;
    date: string;
}
