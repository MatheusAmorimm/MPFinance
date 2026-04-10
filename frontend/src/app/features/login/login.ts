import {
  Component,
  ChangeDetectionStrategy,
  signal,
  inject,
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-login',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
})
export class Login {
  private fb          = inject(FormBuilder);
  private authService = inject(AuthService);
  private router      = inject(Router);

  form = this.fb.group({
    email:      ['', [Validators.required, Validators.email]],
    password:   ['',  Validators.required],
    rememberMe: [false],
  });

  hasAuthError = signal(false);
  toastMessage = signal<string | null>(null);
  toastType    = signal<'success' | 'error'>('success');
  loading      = signal(false);

  private showToast(message: string, type: 'success' | 'error'): void {
    this.toastMessage.set(message);
    this.toastType.set(type);
    setTimeout(() => this.toastMessage.set(null), 4000);
  }

  onLogin(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.hasAuthError.set(false);
    this.loading.set(true);
    const { email, password, rememberMe } = this.form.value;

    this.authService.login({ email, password }, rememberMe ?? false).subscribe({
      next: () => {
        this.loading.set(false);
        this.showToast('Login realizado com sucesso!', 'success');
        setTimeout(() => this.router.navigate(['/home']), 1500);
      },
      error: (err: unknown) => {
        this.loading.set(false);

        if ((err as { status?: number })?.status === 403) {
          this.router.navigate(['/verify-email'], {
            state: { email: this.form.get('email')?.value ?? '' },
          });
          return;
        }

        this.hasAuthError.set(true);
        this.showToast('Senha ou E-mail incorretos.', 'error');
      },
    });
  }
}
