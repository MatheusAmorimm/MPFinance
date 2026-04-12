import {
    ChangeDetectionStrategy,
    Component,
    computed,
    inject,
    OnInit,
    signal,
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { GoalService } from '../../core/services/goal.service';
import { Goal } from '../../core/models/transaction.model';
import { TutorialService } from '../../core/services/tutorial.service';
import { DatePickerComponent } from '../../shared/components/date-picker/date-picker';

@Component({
    selector: 'app-goals',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [CommonModule, ReactiveFormsModule, CurrencyPipe, DatePipe, DatePickerComponent],
    templateUrl: './goals.html',
    styleUrl: './goals.scss',
})
export class Goals implements OnInit {
    private goalService      = inject(GoalService);
    private fb               = inject(FormBuilder);
    private tutorialService  = inject(TutorialService);

    goals = signal<Goal[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);
    showModal = signal(false);
    editingGoal = signal<Goal | null>(null);
    submitting = signal(false);
    deletingId = signal<string | null>(null);
    toastMessage = signal<string | null>(null);
    toastType = signal<'success' | 'error'>('success');

    // Preview of selected image before upload
    coverPreview = signal<string | null>(null);
    private pendingFile: File | null = null;

    form = this.fb.group({
        title:        ['', [Validators.required, Validators.maxLength(150)]],
        targetAmount: [null as number | null, [Validators.required, Validators.min(1)]],
        deadline:     ['', Validators.required],
    });

    modalTitle = computed(() => this.editingGoal() ? 'Editar Meta' : 'Nova Meta');

    // ─── Monthly recommendation ───────────────────────────────────────────────
    monthlyRecommendation(goal: Goal): number {
        const now = new Date();
        const deadline = new Date(goal.deadline);
        const monthsLeft =
            (deadline.getFullYear() - now.getFullYear()) * 12 +
            (deadline.getMonth() - now.getMonth());
        const remaining = goal.targetAmount - goal.currentAmount;
        if (remaining <= 0 || monthsLeft <= 0) return 0;
        return remaining / monthsLeft;
    }

    progressPercent(goal: Goal): number {
        if (goal.targetAmount === 0) return 0;
        return Math.min(100, (goal.currentAmount / goal.targetAmount) * 100);
    }

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    ngOnInit(): void {
        this.tutorialService.startFor('goals');
        this.loadGoals();
    }

    private loadGoals(): void {
        this.loading.set(true);
        this.goalService.getGoals().subscribe({
            next: goals => {
                this.goals.set(goals);
                this.loading.set(false);
            },
            error: () => {
                this.error.set('Erro ao carregar metas. Tente novamente.');
                this.loading.set(false);
            },
        });
    }

    // ─── Modal ────────────────────────────────────────────────────────────────
    openCreate(): void {
        this.editingGoal.set(null);
        this.coverPreview.set(null);
        this.pendingFile = null;
        this.form.reset();
        this.showModal.set(true);
    }

    openEdit(goal: Goal): void {
        this.editingGoal.set(goal);
        this.coverPreview.set(goal.coverImageUrl);
        this.pendingFile = null;
        this.form.setValue({
            title:        goal.title,
            targetAmount: goal.targetAmount,
            deadline:     goal.deadline.substring(0, 10),
        });
        this.showModal.set(true);
    }

    closeModal(): void {
        this.showModal.set(false);
    }

    onCoverFileChange(event: Event): void {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];
        if (!file) return;
        this.pendingFile = file;
        const reader = new FileReader();
        reader.onload = e => this.coverPreview.set(e.target?.result as string);
        reader.readAsDataURL(file);
    }

    onSubmit(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        this.submitting.set(true);
        const { title, targetAmount, deadline } = this.form.getRawValue();
        const payload = { title: title!, targetAmount: targetAmount!, deadline: deadline! };

        const editing = this.editingGoal();

        const save$ = editing
            ? this.goalService.updateGoal(editing.id, payload)
            : this.goalService.createGoal(payload);

        save$.subscribe({
            next: savedGoal => {
                if (this.pendingFile) {
                    this.goalService.uploadCoverImage(savedGoal.id, this.pendingFile).subscribe({
                        next: ({ url }) => {
                            savedGoal = { ...savedGoal, coverImageUrl: url };
                            this.upsertGoal(savedGoal);
                            this.submitting.set(false);
                            this.showModal.set(false);
                            this.showToast(editing ? 'Meta atualizada!' : 'Meta criada!', 'success');
                        },
                        error: () => {
                            this.upsertGoal(savedGoal);
                            this.submitting.set(false);
                            this.showModal.set(false);
                            this.showToast('Meta salva, mas erro ao enviar imagem.', 'error');
                        },
                    });
                } else {
                    this.upsertGoal(savedGoal);
                    this.submitting.set(false);
                    this.showModal.set(false);
                    this.showToast(editing ? 'Meta atualizada!' : 'Meta criada!', 'success');
                }
            },
            error: () => {
                this.submitting.set(false);
                this.showToast('Erro ao salvar meta.', 'error');
            },
        });
    }

    private upsertGoal(goal: Goal): void {
        const editing = this.editingGoal();
        if (editing) {
            this.goals.update(list => list.map(g => g.id === goal.id ? goal : g));
        } else {
            this.goals.update(list => [...list, goal]);
        }
    }

    // ─── Delete ───────────────────────────────────────────────────────────────
    confirmDelete(goal: Goal): void {
        if (!confirm(`Excluir a meta "${goal.title}"? Esta ação não pode ser desfeita.`)) return;

        this.deletingId.set(goal.id);
        this.goalService.deleteGoal(goal.id).subscribe({
            next: () => {
                this.goals.update(list => list.filter(g => g.id !== goal.id));
                this.deletingId.set(null);
                this.showToast('Meta excluída.', 'success');
            },
            error: () => {
                this.deletingId.set(null);
                this.showToast('Erro ao excluir meta.', 'error');
            },
        });
    }

    // ─── Toast ────────────────────────────────────────────────────────────────
    private showToast(msg: string, type: 'success' | 'error'): void {
        this.toastMessage.set(msg);
        this.toastType.set(type);
        setTimeout(() => this.toastMessage.set(null), 3500);
    }
}
