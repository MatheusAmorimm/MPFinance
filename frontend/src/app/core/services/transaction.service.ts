import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category, Goal, Transaction, CreateTransactionPayload } from '../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class TransactionService {
    private readonly http = inject(HttpClient);

    getTransactions(month: number, year: number): Observable<Transaction[]> {
        return this.http.get<Transaction[]>(`/api/transaction?month=${month}&year=${year}`);
    }

    createTransaction(payload: CreateTransactionPayload): Observable<{ message: string }> {
        return this.http.post<{ message: string }>('/api/transaction', payload);
    }

    getCategories(): Observable<Category[]> {
        return this.http.get<Category[]>('/api/category');
    }

    getGoals(): Observable<Goal[]> {
        return this.http.get<Goal[]>('/api/goal');
    }
}
