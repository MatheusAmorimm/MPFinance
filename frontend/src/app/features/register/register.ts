import {
  Component,
  ChangeDetectionStrategy,
  computed,
  signal,
  inject,
} from '@angular/core';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth';

function passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
  const pwd = (control.value as string) ?? '';
  const valid =
    pwd.length >= 8 &&
    /[A-Z]/.test(pwd) &&
    /[a-z]/.test(pwd) &&
    /[0-9]/.test(pwd) &&
    /[!@#$%^&*()\-_=+[\]{};:'",.<>?/\\|`~]/.test(pwd);
  return valid ? null : { passwordStrength: true };
}

function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const pwd     = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pwd === confirm ? null : { mismatch: true };
}

@Component({
  selector: 'app-register',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
})
export class Register {
  private fb          = inject(FormBuilder);
  private authService = inject(AuthService);
  private router      = inject(Router);

  form = this.fb.group(
    {
      name:            ['', [Validators.required, Validators.minLength(3)]],
      email:           ['', [Validators.required, Validators.email]],
      password:        ['', [Validators.required, passwordStrengthValidator]],
      confirmPassword: ['',  Validators.required],
    },
    { validators: passwordMatchValidator }
  );

  private passwordValue = toSignal(
    this.form.get('password')!.valueChanges,
    { initialValue: '' }
  );

  passwordRules = computed(() => {
    const pwd = this.passwordValue() ?? '';
    return {
      minLength:  pwd.length >= 8,
      hasUpper:   /[A-Z]/.test(pwd),
      hasLower:   /[a-z]/.test(pwd),
      hasNumber:  /[0-9]/.test(pwd),
      hasSpecial: /[!@#$%^&*()\-_=+[\]{};:'",.<>?/\\|`~]/.test(pwd),
    };
  });

  toastMessage = signal<string | null>(null);
  toastType    = signal<'success' | 'error'>('success');
  loading      = signal(false);

  private showToast(message: string, type: 'success' | 'error'): void {
    this.toastMessage.set(message);
    this.toastType.set(type);
    setTimeout(() => this.toastMessage.set(null), 4000);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    const { confirmPassword, ...userData } = this.form.value;

    this.authService.register(userData).subscribe({
      next: () => {
        this.loading.set(false);
        this.showToast('Conta criada! Verifique seu e-mail.', 'success');
        setTimeout(() =>
          this.router.navigate(['/verify-email'], {
            state: { email: this.form.get('email')?.value ?? '' },
          }), 1500);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        const message =
          (err as { error?: { message?: string } })?.error?.message ??
          'Erro ao criar conta. Tente novamente.';
        this.showToast(message, 'error');
      },
    });
  }
}
