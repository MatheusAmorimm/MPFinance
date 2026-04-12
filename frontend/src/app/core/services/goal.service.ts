import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Goal } from '../models/transaction.model';

const API = '/api/goal';

@Injectable({ providedIn: 'root' })
export class GoalService {
    private http = inject(HttpClient);

    getGoals(): Observable<Goal[]> {
        return this.http.get<Goal[]>(API);
    }

    createGoal(payload: { title: string; targetAmount: number; deadline: string }): Observable<Goal> {
        return this.http.post<Goal>(API, payload);
    }

    updateGoal(id: string, payload: { title: string; targetAmount: number; deadline: string }): Observable<Goal> {
        return this.http.put<Goal>(`${API}/${id}`, payload);
    }

    deleteGoal(id: string): Observable<void> {
        return this.http.delete<void>(`${API}/${id}`);
    }

    uploadCoverImage(id: string, file: File): Observable<{ url: string }> {
        const form = new FormData();
        form.append('file', file);
        return this.http.post<{ url: string }>(`${API}/${id}/cover-image`, form);
    }
}
