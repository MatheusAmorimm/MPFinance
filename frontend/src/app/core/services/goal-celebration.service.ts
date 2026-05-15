import { Injectable, signal } from '@angular/core';
import { Goal } from '../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class GoalCelebrationService {
    private readonly STORAGE_KEY = 'mpfinance_celebrated_goals';
    private readonly queue: Goal[] = [];

    readonly celebratingGoal = signal<Goal | null>(null);

    celebrate(goal: Goal): void {
        if (this.hasCelebrated(goal.id)) return;
        if (this.celebratingGoal() !== null) {
            if (!this.queue.some(g => g.id === goal.id)) this.queue.push(goal);
            return;
        }
        this.celebratingGoal.set(goal);
    }

    dismiss(): void {
        const goal = this.celebratingGoal();
        if (goal) this.markCelebrated(goal.id);
        const next = this.queue.shift() ?? null;
        this.celebratingGoal.set(next);
    }

    hasCelebrated(goalId: string): boolean {
        try {
            const ids: string[] = JSON.parse(localStorage.getItem(this.STORAGE_KEY) ?? '[]');
            return ids.includes(goalId);
        } catch {
            return false;
        }
    }

    private markCelebrated(goalId: string): void {
        try {
            const ids: string[] = JSON.parse(localStorage.getItem(this.STORAGE_KEY) ?? '[]');
            if (!ids.includes(goalId)) {
                ids.push(goalId);
                localStorage.setItem(this.STORAGE_KEY, JSON.stringify(ids));
            }
        } catch { /* ignore */ }
    }
}
