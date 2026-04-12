import {
  Component,
  ChangeDetectionStrategy,
  signal,
  inject,
  effect,
  OnDestroy,
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { DOCUMENT } from '@angular/common';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-verify-email',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './verify-email.html',
})
export class VerifyEmail implements OnDestroy {
  private fb          = inject(FormBuilder);
  private authService = inject(AuthService);
  private router      = inject(Router);
  private doc         = inject(DOCUMENT);

  email = signal<string>(
    (this.doc.defaultView?.history.state as { email?: string })?.email ?? ''
  );

  readonly digits = [0, 1, 2, 3, 4, 5] as const;

  form = this.fb.group({
    d0: ['', [Validators.required, Validators.pattern('[0-9]')]],
    d1: ['', [Validators.required, Validators.pattern('[0-9]')]],
    d2: ['', [Validators.required, Validators.pattern('[0-9]')]],
    d3: ['', [Validators.required, Validators.pattern('[0-9]')]],
    d4: ['', [Validators.required, Validators.pattern('[0-9]')]],
    d5: ['', [Validators.required, Validators.pattern('[0-9]')]],
  });

  toastMessage   = signal<string | null>(null);
  toastType      = signal<'success' | 'error'>('success');
  loading        = signal(false);
  hasCodeError   = signal(false);
  resendCooldown = signal(0);

  private cooldownInterval: ReturnType<typeof setInterval> | null = null;

  constructor() {
    // Redireciona se não houver e-mail no state
    effect(() => {
      if (!this.email()) this.router.navigate(['/register']);
    });
  }

  ngOnDestroy(): void {
    if (this.cooldownInterval) clearInterval(this.cooldownInterval);
  }

  onDigitInput(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    const digit = input.value.replace(/\D/g, '').slice(-1);
    this.form.patchValue({ [`d${index}`]: digit }, { emitEvent: false });
    input.value = digit;
    this.hasCodeError.set(false);
    if (digit && index < 5) {
      this.doc.getElementById(`digit-${index + 1}`)?.focus();
    }
  }

  onKeydown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Backspace' && !this.form.get(`d${index}`)?.value && index > 0) {
      this.doc.getElementById(`digit-${index - 1}`)?.focus();
    }
  }

  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const digits = (event.clipboardData?.getData('text') ?? '')
      .replace(/\D/g, '')
      .slice(0, 6)
      .split('');

    digits.forEach((d, i) => {
      this.form.patchValue({ [`d${i}`]: d }, { emitEvent: false });
      const input = this.doc.getElementById(`digit-${i}`) as HTMLInputElement | null;
      if (input) input.value = d;
    });

    const focusIdx = Math.min(digits.length, 5);
    this.doc.getElementById(`digit-${focusIdx}`)?.focus();
  }

  private get code(): string {
    return this.digits.map(i => this.form.get(`d${i}`)?.value ?? '').join('');
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.hasCodeError.set(false);

    this.authService.verifyEmail(this.email(), this.code).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/home']);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.hasCodeError.set(true);
        const msg =
          (err as { error?: { message?: string } })?.error?.message ??
          'Código inválido ou expirado.';
        this.showToast(msg, 'error');
      },
    });
  }

  resend(): void {
    if (this.resendCooldown() > 0) return;

    this.authService.resendVerification(this.email()).subscribe({
      next: () => {
        this.showToast('Novo código enviado!', 'success');
        this.startCooldown();
      },
      error: () => this.showToast('Erro ao reenviar. Tente novamente.', 'error'),
    });
  }

  private showToast(message: string, type: 'success' | 'error'): void {
    this.toastMessage.set(message);
    this.toastType.set(type);
    setTimeout(() => this.toastMessage.set(null), 4000);
  }

  private startCooldown(): void {
    this.resendCooldown.set(60);
    this.cooldownInterval = setInterval(() => {
      const current = this.resendCooldown();
      if (current <= 1) {
        clearInterval(this.cooldownInterval!);
        this.cooldownInterval = null;
        this.resendCooldown.set(0);
      } else {
        this.resendCooldown.set(current - 1);
      }
    }, 1000);
  }
}
