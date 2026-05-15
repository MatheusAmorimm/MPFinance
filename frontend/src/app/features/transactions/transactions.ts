import {
    ChangeDetectionStrategy,
    Component,
    OnInit,
    computed,
    effect,
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
import { TutorialService } from '../../core/services/tutorial.service';
import { GoalCelebrationService } from '../../core/services/goal-celebration.service';
import { DatePickerComponent } from '../../shared/components/date-picker/date-picker';

export type TransactionMode = 'income' | 'expense' | 'goal';

const BILL_CATEGORY_NAMES = [
    'Aluguel & Moradia',
    'Condomínio',
    'Conta de Água',
    'Conta de Luz',
    'Empréstimo & Parcelas',
    'Gás de Cozinha',
    'Streaming & Assinaturas',
    'Internet & Celular',
    'Educação & Faculdade',
];

@Component({
    selector: 'app-transactions',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [ReactiveFormsModule, CurrencyPipe, DatePipe, DatePickerComponent],
    templateUrl: './transactions.html',
    styleUrl: './transactions.scss',
})
export class Transactions implements OnInit {
    private readonly service         = inject(TransactionService);
    private readonly fb              = inject(FormBuilder);
    private readonly tutorialService = inject(TutorialService);
    private readonly goalCelebration = inject(GoalCelebrationService);

    // ─── Estado de dados ─────────────────────────────────────────────────────
    readonly transactions = signal<Transaction[]>([]);
    readonly categories   = signal<Category[]>([]);
    readonly goals        = signal<Goal[]>([]);

    // ─── Estado da UI ────────────────────────────────────────────────────────
    readonly activeMode         = signal<TransactionMode>('expense');
    readonly selectedCategoryId = signal('');
    readonly currentMonth       = signal(0);
    readonly currentYear        = signal(0);
    readonly loading            = signal(true);
    readonly submitting         = signal(false);
    readonly toastMessage       = signal<string | null>(null);
    readonly toastType          = signal<'success' | 'error'>('success');

    // ─── Estado de edição/exclusão ───────────────────────────────────────────
    readonly editingTransaction  = signal<Transaction | null>(null);
    readonly editSubmitting      = signal(false);
    readonly deletingId          = signal<string | null>(null);
    readonly pendingDeleteTx     = signal<Transaction | null>(null);

    // ─── Derivados ───────────────────────────────────────────────────────────
    readonly incomeCategories  = computed(() => this.categories().filter(c => c.type === 'income'));
    readonly expenseCategories = computed(() => this.categories().filter(c => c.type === 'expense'));
    readonly activeCategories  = computed(() =>
        this.activeMode() === 'income' ? this.incomeCategories() : this.expenseCategories()
    );

    readonly showDueDate = computed(() => {
        if (this.activeMode() !== 'expense') return false;
        const cat = this.categories().find(c => c.id === this.selectedCategoryId());
        return cat ? BILL_CATEGORY_NAMES.includes(cat.name) : false;
    });

    readonly totalIncome     = computed(() =>
        this.transactions().filter(t => t.type === 'income').reduce((s, t) => s + t.amount, 0)
    );
    readonly totalExpenses   = computed(() =>
        this.transactions().filter(t => t.type === 'expense').reduce((s, t) => s + t.amount, 0)
    );
    readonly totalGoalInvest = computed(() =>
        this.transactions().filter(t => t.type === 'goal').reduce((s, t) => s + t.amount, 0)
    );

    readonly monthLabel = computed(() => {
        const d = new Date(this.currentYear(), this.currentMonth() - 1, 1);
        const s = d.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
        return s.charAt(0).toUpperCase() + s.slice(1);
    });

    // ─── Formulários ─────────────────────────────────────────────────────────
    readonly form = this.fb.group({
        categoryId:  ['',   Validators.required],
        goalId:      [''],
        amount:      [null as number | null, [Validators.required, Validators.min(0.01)]],
        description: ['',   Validators.maxLength(255)],
        date:        ['',   Validators.required],
    });

    readonly editForm = this.fb.group({
        amount:      [null as number | null, [Validators.required, Validators.min(0.01)]],
        description: ['',   Validators.maxLength(255)],
        date:        ['',   Validators.required],
    });

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    constructor() {
        effect(() => {
            const dateCtrl = this.form.get('date')!;
            if (this.showDueDate()) {
                dateCtrl.setValidators(Validators.required);
            } else {
                dateCtrl.clearValidators();
            }
            dateCtrl.updateValueAndValidity({ emitEvent: false });
        });
    }

    ngOnInit(): void {
        this.tutorialService.startFor('transactions');
        const now = new Date();
        this.currentMonth.set(now.getMonth() + 1);
        this.currentYear.set(now.getFullYear());
        this.form.patchValue({ date: this.todayIso() });

        this.form.get('categoryId')!.valueChanges.subscribe(v => {
            this.selectedCategoryId.set(v ?? '');
        });

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
        const d = new Date(this.currentYear(), this.currentMonth() - 2, 1);
        this.currentMonth.set(d.getMonth() + 1);
        this.currentYear.set(d.getFullYear());
        this.loadTransactions();
    }

    nextMonth(): void {
        const d = new Date(this.currentYear(), this.currentMonth(), 1);
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

    // ─── Submit (novo lançamento) ─────────────────────────────────────────────
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
                this.setMode(mode);
                this.loadTransactions();
                if (mode === 'goal' && goalId) {
                    this.service.getGoals().subscribe(goals => {
                        const updated = goals.find(g => g.id === goalId);
                        if (updated && updated.currentAmount >= updated.targetAmount) {
                            this.goalCelebration.celebrate(updated);
                        }
                    });
                }
            },
            error: () => {
                this.submitting.set(false);
                this.showToast('Erro ao registrar o lançamento.', 'error');
            },
        });
    }

    // ─── Edição ───────────────────────────────────────────────────────────────
    openEdit(tx: Transaction): void {
        this.editingTransaction.set(tx);
        this.editForm.setValue({
            amount:      tx.amount,
            description: tx.description ?? '',
            date:        tx.date.substring(0, 10),
        });
    }

    closeEdit(): void {
        this.editingTransaction.set(null);
    }

    onEditSubmit(): void {
        if (this.editForm.invalid) {
            this.editForm.markAllAsTouched();
            return;
        }

        const tx = this.editingTransaction();
        if (!tx) return;

        const { amount, description, date } = this.editForm.value;

        this.editSubmitting.set(true);
        this.service.updateTransaction(tx.id, {
            amount:      amount!,
            description: description ?? '',
            date:        `${date}T12:00:00.000Z`,
        }).subscribe({
            next: () => {
                this.editSubmitting.set(false);
                this.editingTransaction.set(null);
                this.showToast('Lançamento atualizado!', 'success');
                this.loadTransactions();
            },
            error: () => {
                this.editSubmitting.set(false);
                this.showToast('Erro ao atualizar o lançamento.', 'error');
            },
        });
    }

    // ─── Exclusão ────────────────────────────────────────────────────────────
    onDelete(tx: Transaction): void {
        this.pendingDeleteTx.set(tx);
    }

    cancelDeleteTx(): void {
        this.pendingDeleteTx.set(null);
    }

    executeDeleteTx(): void {
        const tx = this.pendingDeleteTx();
        if (!tx) return;

        this.deletingId.set(tx.id);
        this.service.deleteTransaction(tx.id).subscribe({
            next: () => {
                this.deletingId.set(null);
                this.pendingDeleteTx.set(null);
                this.transactions.update(list => list.filter(t => t.id !== tx.id));
                this.showToast('Lançamento excluído.', 'success');
            },
            error: () => {
                this.deletingId.set(null);
                this.pendingDeleteTx.set(null);
                this.showToast('Erro ao excluir o lançamento.', 'error');
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
