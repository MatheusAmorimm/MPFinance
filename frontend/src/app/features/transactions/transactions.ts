import {
    ChangeDetectionStrategy,
    Component,
    OnInit,
    computed,
    inject,
    signal,
} from '@angular/core';
import {
    FormBuilder,
    ReactiveFormsModule,
    Validators,
} from '@angular/forms';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { TransactionService } from '../../core/services/transaction.service';
import { Category, Goal, Transaction, CreateTransactionPayload } from '../../core/models/transaction.model';

export type TransactionMode = 'income' | 'expense' | 'goal';

@Component({
    selector: 'app-transactions',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [ReactiveFormsModule, CurrencyPipe, DatePipe],
    templateUrl: './transactions.html',
    styleUrl: './transactions.scss',
})
export class Transactions implements OnInit {
    private readonly service = inject(TransactionService);
    private readonly fb      = inject(FormBuilder);

    // ─── Estado de dados ─────────────────────────────────────────────────────
    readonly transactions = signal<Transaction[]>([]);
    readonly categories   = signal<Category[]>([]);
    readonly goals        = signal<Goal[]>([]);

    // ─── Estado da UI ────────────────────────────────────────────────────────
    readonly activeMode   = signal<TransactionMode>('expense');
    readonly currentMonth = signal(0);
    readonly currentYear  = signal(0);
    readonly loading      = signal(true);
    readonly submitting   = signal(false);
    readonly toastMessage = signal<string | null>(null);
    readonly toastType    = signal<'success' | 'error'>('success');

    // ─── Derivados ───────────────────────────────────────────────────────────
    readonly incomeCategories  = computed(() => this.categories().filter(c => c.type === 'income'));
    readonly expenseCategories = computed(() => this.categories().filter(c => c.type === 'expense'));
    readonly activeCategories  = computed(() =>
        this.activeMode() === 'income' ? this.incomeCategories() : this.expenseCategories()
    );

    readonly totalIncome   = computed(() =>
        this.transactions().filter(t => t.type === 'income').reduce((s, t) => s + t.amount, 0)
    );
    readonly totalExpenses = computed(() =>
        this.transactions().filter(t => t.type === 'expense').reduce((s, t) => s + t.amount, 0)
    );

    readonly monthLabel = computed(() => {
        const d = new Date(this.currentYear(), this.currentMonth() - 1, 1);
        const s = d.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
        return s.charAt(0).toUpperCase() + s.slice(1);
    });

    // ─── Formulário ──────────────────────────────────────────────────────────
    readonly form = this.fb.group({
        categoryId:  ['',   Validators.required],
        goalId:      [''],
        amount:      [null as number | null, [Validators.required, Validators.min(0.01)]],
        description: ['',   Validators.maxLength(255)],
        date:        ['',   Validators.required],
    });

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    ngOnInit(): void {
        const now = new Date();
        this.currentMonth.set(now.getMonth() + 1);
        this.currentYear.set(now.getFullYear());
        this.form.patchValue({ date: this.todayIso() });

        forkJoin({
            categories: this.service.getCategories(),
            goals:      this.service.getGoals(),
        }).subscribe({
            next: ({ categories, goals }) => {
                this.categories.set(categories);
                this.goals.set(goals);
                this.loading.set(false);
                this.loadTransactions();
            },
            error: () => {
                this.loading.set(false);
                this.showToast('Erro ao carregar dados iniciais.', 'error');
            },
        });
    }

    // ─── Navegação de mês ────────────────────────────────────────────────────
    previousMonth(): void {
        const d = new Date(this.currentYear(), this.currentMonth() - 2, 1); // -2 = mês anterior (0-indexed)
        this.currentMonth.set(d.getMonth() + 1);
        this.currentYear.set(d.getFullYear());
        this.loadTransactions();
    }

    nextMonth(): void {
        const d = new Date(this.currentYear(), this.currentMonth(), 1); // mês atual em 0-indexed já é o próximo
        this.currentMonth.set(d.getMonth() + 1);
        this.currentYear.set(d.getFullYear());
        this.loadTransactions();
    }

    // ─── Modo do toggle ──────────────────────────────────────────────────────
    setMode(mode: TransactionMode): void {
        this.activeMode.set(mode);
        this.form.reset({ date: this.todayIso() });

        const catCtrl  = this.form.get('categoryId')!;
        const goalCtrl = this.form.get('goalId')!;

        if (mode === 'goal') {
            catCtrl.clearValidators();
            goalCtrl.setValidators(Validators.required);
        } else {
            catCtrl.setValidators(Validators.required);
            goalCtrl.clearValidators();
        }

        catCtrl.updateValueAndValidity();
        goalCtrl.updateValueAndValidity();
    }

    // ─── Submit ──────────────────────────────────────────────────────────────
    onSubmit(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        const { categoryId, goalId, amount, description, date } = this.form.value;
        const mode = this.activeMode();

        let resolvedCategoryId = categoryId ?? '';

        if (mode === 'goal') {
            const fallback = this.incomeCategories()[0];
            if (!fallback) {
                this.showToast('Nenhuma categoria de receita disponível.', 'error');
                return;
            }
            resolvedCategoryId = fallback.id;
        }

        const payload: CreateTransactionPayload = {
            categoryId:  resolvedCategoryId,
            amount:      amount!,
            description: description ?? '',
            date:        `${date}T12:00:00.000Z`,
            goalId:      mode === 'goal' && goalId ? goalId : undefined,
        };

        this.submitting.set(true);
        this.service.createTransaction(payload).subscribe({
            next: () => {
                this.submitting.set(false);
                this.showToast('Lançamento registrado com sucesso!', 'success');
                this.form.reset({ date: this.todayIso() });
                this.setMode(mode); // restaura validators
                this.loadTransactions();
            },
            error: () => {
                this.submitting.set(false);
                this.showToast('Erro ao registrar o lançamento.', 'error');
            },
        });
    }

    // ─── Privados ────────────────────────────────────────────────────────────
    private loadTransactions(): void {
        this.service.getTransactions(this.currentMonth(), this.currentYear()).subscribe({
            next:  (data) => this.transactions.set(data),
            error: ()     => this.showToast('Erro ao carregar lançamentos.', 'error'),
        });
    }

    private showToast(message: string, type: 'success' | 'error'): void {
        this.toastMessage.set(message);
        this.toastType.set(type);
        setTimeout(() => this.toastMessage.set(null), 4000);
    }

    private todayIso(): string {
        const now = new Date();
        return now.toISOString().split('T')[0];
    }
}
